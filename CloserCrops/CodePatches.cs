using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Object = StardewValley.Object;

namespace CloserCrops
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public static class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !Config.MultiplyPlantAndHarvest || !IsCloserCropseed(__instance))
                    return true;
                Vector2 placementTile = new(x / 64, y / 64);

                if (!location.terrainFeatures.TryGetValue(placementTile, out var tf) || tf is not HoeDirt dirt || dirt.crop?.netSeedIndex.Value != __instance.ItemId || dirt.crop.currentPhase.Value > 0)
                    return true;
                int num = 1;
                if(dirt.crop.modData.TryGetValue(numberKey, out var str) && int.TryParse(str, out num))
                {
                    if (num == 4)
                        return true;
                }
                else
                {
                    dirt.crop.modData[numberKey] = "1";
                }
                var add = Math.Min(__instance.Stack, 4 - num);
                __instance.Stack -= add - 1;
                location.playSound("dirtyHit", placementTile);
                dirt.crop.modData[numberKey] = (num + add).ToString();
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.canBePlacedHere))]
        public static class Object_canBePlacedHere_Patch
        {
            public static bool Prefix(Object __instance, GameLocation l, Vector2 tile, CollisionMask collisionMask, bool showError, ref bool __result)
            {
                if (!Config.ModEnabled || !Config.MultiplyPlantAndHarvest || !IsCloserCropseed(__instance) || !l.terrainFeatures.TryGetValue(tile, out var tf) || tf is not HoeDirt dirt || dirt.crop?.netSeedIndex.Value != __instance.ItemId || dirt.crop.currentPhase.Value > 0)
                    return true;
                int num = 1;
                if(dirt.crop.modData.TryGetValue(numberKey, out var str) && int.TryParse(str, out num))
                {
                    if (num == 4)
                        return true;
                }
                else
                {
                    dirt.crop.modData[numberKey] = "1";
                }
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.performUseAction))]
        public static class HoeDirt_performUseAction_Patch
        {
            public static bool Prefix(HoeDirt __instance, ref bool __result)
            {
                if (!Config.ModEnabled || !Config.MultiplyPlantAndHarvest || !IsMiniCrop(__instance.crop) || __instance.crop.netSeedIndex.Value != Game1.player?.ActiveObject?.ItemId || __instance.crop.currentPhase.Value > 0)
                    return true;
                int num = 1;
                if (__instance.crop.modData.TryGetValue(numberKey, out var str) && int.TryParse(str, out num))
                {
                    if (num == 4)
                        return true;
                }
                var add = Math.Min(Game1.player.ActiveObject.Stack, 4 - num);
                Game1.player.ActiveObject.Stack -= add - 1;
                __instance.Location.playSound("dirtyHit", __instance.Tile);
                __instance.crop.modData[numberKey] = (num + add).ToString();
                __result = true; 
                return false;
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.plant))]
        public static class HoeDirt_plant_Patch
        {
            public static void Postfix(HoeDirt __instance, string itemId, Farmer who, bool isFertilizer, ref bool __result)
            {
                if (!Config.ModEnabled || !Config.MultiplyPlantAndHarvest || !__result || who is null || isFertilizer || !IsMiniCrop(__instance.crop) || who.ActiveObject?.ItemId != __instance.crop.netSeedIndex.Value || __instance.crop.modData.ContainsKey(numberKey))
                    return;

                var add = Math.Min(who.ActiveObject.Stack, 4);
                who.ActiveObject.Stack -= add - 1;
                __instance.crop.modData[numberKey] = add.ToString();
            }
        }
        [HarmonyPatch(typeof(Crop), nameof(Crop.harvest))]
        public static class Crop_harvest_Patch
        {
            private static bool skip = false;
            public static bool Prefix(Crop __instance, int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester, bool isForcedScytheHarvest, ref bool __result)
            {
                if (!Config.ModEnabled || skip || !Config.MultiplyPlantAndHarvest || !IsMiniCrop(__instance) || !__instance.modData.TryGetValue(numberKey, out var str) || !int.TryParse(str, out var num))
                    return true;
                skip = true;
                for(int i = 0; i < num; i++)
                {
                    __result = __instance.harvest(xTile, yTile, soil, junimoHarvester, isForcedScytheHarvest);
                    if (!__result)
                    {
                        skip = false;
                        return false;
                    }
                }
                skip = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(Crop), nameof(Crop.draw))]
        public static class Crop_draw_Patch
        {
            private static bool skip = false;
            public static bool Prefix(Crop __instance, SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
            {
                if (!Config.ModEnabled || skip || !IsMiniCrop(__instance))
                    return true;
                int num = 1;
                if (!Config.MultiplyPlantAndHarvest)
                {
                    num = 4;
                }
                else if (!__instance.modData.TryGetValue(numberKey, out var str) || !int.TryParse(str, out num))
                {
                    num = 1;
                    __instance.modData[numberKey] = "1";
                }
                var layerDepth = __instance.layerDepth;
                var coloredLayerDepth = __instance.coloredLayerDepth;
                var drawPosition = __instance.drawPosition;
                skip = true;
                var tempDrawPosition = drawPosition - new Vector2(16, 24);
                __instance.drawPosition = tempDrawPosition;
                __instance.draw(b, tileLocation, toTint, rotation);
                if(num > 1)
                {
                    __instance.layerDepth = layerDepth + 0.5f / 10000f;
                    __instance.coloredLayerDepth = coloredLayerDepth + 1 / 10000f;
                    __instance.drawPosition = tempDrawPosition + new Vector2(32, 0);
                    __instance.draw(b, tileLocation, toTint, rotation);
                }
                if (num > 2)
                {
                    __instance.layerDepth = layerDepth + 32f /10000f;
                    __instance.coloredLayerDepth = coloredLayerDepth + 32 / 10000f;
                    __instance.drawPosition = tempDrawPosition + new Vector2(0, 32);
                    __instance.draw(b, tileLocation, toTint, rotation);
                }
                if (num > 3)
                {
                    __instance.layerDepth = layerDepth + 32.5f / 10000f;
                    __instance.coloredLayerDepth = coloredLayerDepth + 33 / 10000f;
                    __instance.drawPosition = tempDrawPosition + new Vector2(32, 32);
                    __instance.draw(b, tileLocation, toTint, rotation);
                }
                __instance.layerDepth = layerDepth;
                __instance.coloredLayerDepth = coloredLayerDepth;
                __instance.drawPosition = drawPosition;
                skip = false;
                return false;

            }

            public static IEnumerable<CodeInstruction> XTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Crop.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 4 && codes[i].opcode == OpCodes.Ldc_R4 && codes[i].operand is float f && f == 4)
                    {
                        SMonitor.Log("Adding scale");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckScale))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}