﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CraftFromContainers
{
    public partial class ModEntry
    {
        public static bool skip = false;
        private static bool IsValidToPull()
        {
            if (skip)
                return false;
            if (Environment.StackTrace.Contains("CarpenterMenu") && Config.EnableForBuilding)
                return true;
            if (Environment.StackTrace.Contains("CraftingPage") && Config.EnableForCrafting)
                return true;
            return false;
        }
        private static void ConsumeAll(List<KeyValuePair<int, int>> list, List<Chest> additional_materials)
        {
            Dictionary<int, int> missing = new();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var index = list[i].Key;
                int required_count = list[i].Value;
                bool foundInBackpack = false;
                for (int j = Game1.player.Items.Count - 1; j >= 0; j--)
                {
                    if (Game1.player.Items[j] != null && Game1.player.Items[j] is Object && !(Game1.player.Items[j] as Object).bigCraftable.Value && (((Object)Game1.player.Items[j]).ParentSheetIndex == index || ((Object)Game1.player.Items[j]).Category == index || CraftingRecipe.isThereSpecialIngredientRule((Object)Game1.player.Items[j], index)))
                    {
                        int toRemove = required_count;
                        required_count -= Game1.player.Items[j].Stack;
                        Game1.player.Items[j].Stack -= toRemove;
                        if (Game1.player.Items[j].Stack <= 0)
                        {
                            Game1.player.Items[j] = null;
                        }
                        if (required_count <= 0)
                        {
                            foundInBackpack = true;
                            break;
                        }
                    }
                }
                if (additional_materials != null && !foundInBackpack)
                {
                    for (int c = 0; c < additional_materials.Count; c++)
                    {
                        Chest chest = additional_materials[c];
                        if (chest != null)
                        {
                            bool removedItem = false;
                            for (int k = chest.items.Count - 1; k >= 0; k--)
                            {
                                if (chest.items[k] != null && chest.items[k] is Object && (((Object)chest.items[k]).ParentSheetIndex == index || ((Object)chest.items[k]).Category == index || CraftingRecipe.isThereSpecialIngredientRule((Object)chest.items[k], index)))
                                {
                                    int removed_count = Math.Min(required_count, chest.items[k].Stack);
                                    required_count -= removed_count;
                                    chest.items[k].Stack -= removed_count;
                                    if (chest.items[k].Stack <= 0)
                                    {
                                        chest.items[k] = null;
                                        removedItem = true;
                                    }
                                    if (required_count <= 0)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (removedItem)
                            {
                                chest.clearNulls();
                            }
                            if (required_count <= 0)
                            {
                                break;
                            }
                        }
                    }
                }
                if (required_count > 0)
                {
                    missing.Add(index, required_count);
                }
            }
            ConsumeMissing(missing);
        }

        private static void ConsumeMissing(Dictionary<int, int> missing)
        {
            foreach (var kvp in missing)
            {
                int found = 0;
                foreach (var l in Game1.locations)
                {
                    foreach (Object obj in l.objects.Values)
                    {
                        if (obj is not null)
                        {
                            NetObjectList<Item> items;
                            if (obj is StorageFurniture)
                            {
                                items = (obj as StorageFurniture).heldItems;
                            }
                            else if (obj is Chest)
                            {
                                items = (obj as Chest).items;
                            }
                            else
                                continue;

                            var amount = Game1.player.getItemCountInList(items, kvp.Key, 0);
                            if (amount <= 0)
                                continue;
                            var min = Math.Min(kvp.Value, amount);
                            SMonitor.Log($"Consuming {kvp.Key}x{min} from {obj.Name}");
                            ConsumeObject(items, kvp.Key, min);
                            found += amount;
                            if (found >= kvp.Value)
                            {
                                goto done;
                            }
                        }
                    }
                }
            done:
                continue;
            }
        }

        private static void ConsumeObject(IList<Item> items, int index, int quantity)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i] != null && items[i] is Object && ((Object)items[i]).ParentSheetIndex == index)
                {
                    int toRemove = quantity;
                    quantity -= items[i].Stack;
                    items[i].Stack -= toRemove;
                    if (items[i].Stack <= 0)
                    {
                        items[i] = null;
                    }
                    if (quantity <= 0)
                    {
                        return;
                    }
                }
            }
        }
        private static bool CheckAmount(int itemIndex, int quantity, int minPrice)
        {
            int found = 0;
            foreach (var l in Game1.locations)
            {
                foreach (Object obj in l.objects.Values)
                {
                    if (obj is not null)
                    {
                        if (obj is StorageFurniture)
                        {
                            found += Game1.player.getItemCountInList((obj as StorageFurniture).heldItems, itemIndex, minPrice);
                        }
                        else if (obj is Chest)
                        {
                            found += Game1.player.getItemCountInList((obj as Chest).items, itemIndex, minPrice);
                        }
                        if (found >= quantity)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private static bool CheckAllAmounts(List<KeyValuePair<int, int>> recipeList, IList<Item> extraToCheck)
        {
            foreach (KeyValuePair<int, int> kvp in recipeList)
            {
                int required_count = kvp.Value;
                required_count -= Game1.player.getItemCount(kvp.Key, 5);
                if (required_count > 0)
                {
                    if (extraToCheck != null)
                    {
                        required_count -= Game1.player.getItemCountInList(extraToCheck, kvp.Key, 5);
                        if (required_count <= 0)
                        {
                            continue;
                        }
                    }
                    if (!CheckAmount(kvp.Key, required_count, 5))
                        return false;
                }
            }
            return true;
        }
    }
}