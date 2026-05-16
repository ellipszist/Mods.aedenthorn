using StardewValley;
using StardewValley.Audio;
using StardewValley.Objects;
using System;
using System.Linq;

namespace FenceRepair
{
    public partial class ModEntry
    {
        public static bool TryRepairFence(GameLocation location, Fence fence, Farmer who, bool probe)
        {
            if (Config.RequireMats)
            {
                Item sample = null;
                var recipe = new CraftingRecipe(fence.Name, false);
                if (recipe.name != fence.Name || recipe.recipeList.Count != 1)
                {
                    lastCheck = false;
                    return false;
                }
                var kvp = recipe.recipeList.First();
                int amount = kvp.Value;

                if(CraftingRecipe.ItemMatchesForCrafting(who.ActiveItem, kvp.Key))
                {
                    int count = Math.Min(amount, who.ActiveItem.Stack);
                    amount -= count;
                    if (!probe)
                    {
                        if (sample is null)
                            sample = who.ActiveItem;
                        who.ActiveItem = who.ActiveItem.ConsumeStack(count);
                    }
                }
                if (amount > 0 && Config.TakeMatsFromInventory)
                {
                    for (int i = 0; i < who.Items.Count; i++)
                    {
                        var item = who.Items[i];
                        if (CraftingRecipe.ItemMatchesForCrafting(item, kvp.Key))
                        {
                            int count = Math.Min(amount, item.Stack);
                            amount -= count;
                            if (!probe)
                            {
                                if (sample is null)
                                    sample = item;
                                who.Items[i] = item.ConsumeStack(count);
                            }
                        }
                        if (amount <= 0)
                            break;
                    }
                }
                if (amount > 0 && Config.TakeMatsFromChests)
                {
                    foreach (var o in who.currentLocation.Objects.Values)
                    {
                        if (o is not Chest c)
                            continue;
                        for (int i = 0; i < c.Items.Count; i++)
                        {
                            var item = c.Items[i];
                            if (CraftingRecipe.ItemMatchesForCrafting(item, kvp.Key))
                            {
                                int count = Math.Min(amount, item.Stack);
                                amount -= count;
                                if (!probe)
                                {
                                    if (sample is null)
                                        sample = item;

                                    c.Items[i] = item.ConsumeStack(count);
                                }
                            }
                            if (amount <= 0)
                                break;
                        }
                    }
                }
                if (amount > 0)
                {
                    lastCheck = false;
                    return false;
                }
                if (!probe && sample != null)
                {
                    who.ShowItemReceivedHudMessage(sample, kvp.Value);
                }
            }
            if (!probe)
            {

                string repair_sound = fence.GetRepairSound();
                if (!string.IsNullOrEmpty(repair_sound))
                {
                    location.playSound(repair_sound, null, null, SoundContext.Default);
                }
                fence.repairQueued.Value = true;
                lastMouseTile = new(-1, -1);
            }
            else
            {
                lastCheck = true;
            }
            return true;
        }
    }
}