using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace GiantCropInteriors
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GiantCrop), nameof(GiantCrop.draw), new Type[] { typeof(SpriteBatch) })]
        public class GiantCrop_draw_Patch
        {
            public static void Postfix(GiantCrop __instance)
            {
                if (__instance.Location is null)
                    return;
                if (!Config.ModEnabled || !BuildingDict.ContainsKey(__instance.Id))
                {
                    if (__instance.modData.ContainsKey(builtAtKey) && __instance.Location.getBuildingAt(__instance.Tile) is Building old)
                    {
                        __instance.Location.buildings.Remove(old);
                    }
                    return;
                }
                if (__instance.Location.buildings.FirstOrDefault(i => (int)__instance.Tile.X == i.tileX.Value && (int)__instance.Tile.Y  == i.tileY.Value) is Building exists)
                {
                    if(exists.buildingType.Value == modPrefix + __instance.Id)
                    {
                        return;
                    }
                    __instance.Location.buildings.Remove(exists);
                }
                else if (__instance.modData.TryGetValue(builtAtKey, out var str) && __instance.Location.buildings.FirstOrDefault(i => str == $"{i.tileX},{i.tileY}") is Building moved && moved.buildingType.Value == modPrefix + __instance.Id)
                {
                    moved.tileX.Value = (int)__instance.Tile.X;
                    moved.tileY.Value = (int)__instance.Tile.Y;
                    return;
                }
                Building b = new Building(modPrefix + __instance.Id, __instance.Tile);
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
                if (__instance.GetParentLocation().resourceClumps.FirstOrDefault(rc => rc is GiantCrop g && g.Id == cropType && g.Tile.X == __instance.tileX.Value && g.Tile.Y == __instance.tileY.Value) == null)
                {
                    ToRemove.Add(__instance);
                    SHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
                }
            }
        }

        [HarmonyPatch(typeof(GiantCrop), nameof(GiantCrop.performToolAction))]
        public class GiantCrop_performToolAction_Patch
        {
            public static bool Prefix(GiantCrop __instance)
            {
                if (!BuildingDict.ContainsKey(__instance.Id))
                    return true;
                var x = __instance.Location.buildings.FirstOrDefault(i => (int)__instance.Tile.X == i.tileX.Value && (int)__instance.Tile.Y == i.tileY.Value);
                if (__instance.Location.buildings.FirstOrDefault(i => (int)__instance.Tile.X == i.tileX.Value && (int)__instance.Tile.Y == i.tileY.Value) is Building exists && exists.buildingType.Value == modPrefix + __instance.Id && exists.GetIndoors() is GameLocation l && (l.farmers.Any() || l.characters.Any() || l.animals.Any() || ((l.furniture.Any() || l.objects.Values.Where(o => o.Fragility != 2).Any()) && Config.ProtectObjects)))
                {
                    Game1.showRedMessage(SHelper.Translation.Get("not-empty"));
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(DecoratableLocation), "IsFloorableTile")]
        public class DecoratableLocation_IsFloorableTile_Patch
        {
            public static void Postfix(DecoratableLocation __instance, int x, int y, ref bool __result)
            {
                if (!__result)
                    return;

                string floor_id = __instance.GetFloorID(x, y);
                __result = floor_id != null;
            }
        }

    }
}