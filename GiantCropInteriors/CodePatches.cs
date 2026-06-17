using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GiantCropInteriors
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GiantCrop), nameof(GiantCrop.draw), new Type[] { typeof(SpriteBatch) })]
        public class CollectionsPage_draw_Patch
        {
            public static void Postfix(GiantCrop __instance)
            {
                if (__instance.Location is null)
                    return;
                if (!Config.ModEnabled || !BuildingDict.TryGetValue(__instance.Id, out var name))
                {
                    if (__instance.modData.ContainsKey(builtAtKey) && __instance.Location.getBuildingAt(__instance.Tile) is Building old)
                    {
                        __instance.Location.buildings.Remove(old);
                    }
                    return;
                }
                var asd = new Building(name, __instance.Tile);
                var d = Game1.buildingData;
                if (__instance.Location.buildings.FirstOrDefault(i => (int)__instance.Tile.X == i.tileX.Value && (int)__instance.Tile.Y  == i.tileY.Value) is Building exists && exists.buildingType.Value == name)
                {
                    return;
                }

                if (__instance.modData.TryGetValue(builtAtKey, out var str) && __instance.Location.buildings.FirstOrDefault(i => str == $"{i.tileX},{i.tileY}") is Building moved && moved.buildingType.Value == name)
                {
                    moved.tileX.Value = (int)__instance.Tile.X;
                    moved.tileY.Value = (int)__instance.Tile.Y;
                    return;
                }
                Building b = new Building(name, __instance.Tile);
                b.FinishConstruction(true);
                b.LoadFromBuildingData(b.GetData(), false, true);
                b.load();
                b.modData[cropKey] = __instance.Id;
                __instance.modData[builtAtKey] = $"{__instance.Tile.X},{__instance.Tile.Y}";
                __instance.Location.buildings.Add(b);
            }
        }

        [HarmonyPatch(typeof(Building), nameof(Building.draw), new Type[] { typeof(SpriteBatch) })]
        public class Building_draw_Patch
        {
            public static void Postfix(Building __instance)
            {
                if (__instance.GetParentLocation() is null)
                    return;
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(cropKey, out var cropType))
                    return;
                if (__instance.GetParentLocation().terrainFeatures.Pairs.FirstOrDefault(kvp => kvp.Value is GiantCrop g && g.Id == cropType && kvp.Key.X == __instance.tileX.Value && kvp.Key.Y == __instance.tileY.Value).Value == null)
                {
                    __instance.GetParentLocation().buildings.Remove(__instance);
                }
            }
        }
    }
}