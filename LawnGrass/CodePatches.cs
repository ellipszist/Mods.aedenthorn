using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LawnGrass
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Grass), nameof(Grass.draw))]
        public static class Grass_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Grass.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi == AccessTools.PropertyGetter(typeof(Color), nameof(Color.White)))
                    {
                        SMonitor.Log("Intercepting source rect");
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetSourceRect))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                    else if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi2 && mi2 == AccessTools.Method(typeof(Game1), nameof(Game1.GlobalToLocal), new Type[] { typeof(xTile.Dimensions.Rectangle), typeof(Vector2) }))
                    {
                        SMonitor.Log("Intercepting position");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetPos))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Grass), new Type[] { typeof(int), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public static class Grass_Patch
        {
            public static void Prefix(int which, ref int numberOfWeeds)
            {
                if (Config.ModEnabled && which == 1 && SHelper.Input.IsDown(Config.ModKey))
                    numberOfWeeds = 0;
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.performToolAction))]
        public static class Grass_performToolAction_Patch
        {
            public static void Prefix(Grass __instance)
            {
                if (!Config.ModEnabled)
                    return;
                lastWeedCount.Value = __instance.numberOfWeeds.Value;
            }
            public static void Postfix(Grass __instance, Tool t, Vector2 tileLocation, ref bool __result)
            {
                if (!Config.ModEnabled || __instance.grassType.Value != 1)
                    return;
                __instance.numberOfWeeds.Value = Math.Max(0, __instance.numberOfWeeds.Value);
                if (__result && t is not Pickaxe)
                {
                    __result = false;
                }
                else if(!__result && t is Pickaxe)
                {
                    Game1.createItemDebris(ItemRegistry.Create("(O)297"), __instance.Tile * 64, 0, __instance.Location);
                    __result = true;
                }
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.dayUpdate))]
        public static class Grass_dayUpdate_Patch
        {
            public static void Prefix(Grass __instance)
            {
                if (!Config.ModEnabled)
                    return;
                lastWeedCount.Value = __instance.numberOfWeeds.Value;
            }
            public static void Postfix(Grass __instance)
            {
                if (!Config.ModEnabled || __instance.grassType.Value != 1)
                    return;
                if(__instance.numberOfWeeds.Value > lastWeedCount.Value)
                {
                    __instance.numberOfWeeds.Value = Math.Clamp(lastWeedCount.Value + (Game1.random.NextDouble() < Config.GrowChance ? 1 : 0), 0, 4);
                }
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.reduceBy))]
        public static class Grass_reduceBy_Patch
        {
            public static void Prefix(Grass __instance)
            {
                if (!Config.ModEnabled)
                    return;
                lastWeedCount.Value = __instance.numberOfWeeds.Value;
            }
            public static void Postfix(Grass __instance, ref bool __result)
            {
                if (!Config.ModEnabled || !__result)
                    return;
                __instance.numberOfWeeds.Value = Math.Max(__instance.numberOfWeeds.Value, 0);
                __result = false;
            }
        }
        [HarmonyPatch(typeof(Grass), "shake")]
        public static class Grass_shake_Patch
        {
            public static void Prefix(Grass __instance, ref float shake, ref float rate)
            {
                if (!Config.ModEnabled || __instance.numberOfWeeds.Value == 4)
                    return;
                shake *= (__instance.numberOfWeeds.Value / 4f);
            }
        }
        [HarmonyPatch(typeof(Grass), "createDestroySprites")]
        public static class Grass_createDestroySprites_Patch
        {
            public static bool Prefix()
            {
                if (!Config.ModEnabled || lastWeedCount.Value > 0)
                    return true;
                lastWeedCount.Value = 4;
                return false;
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.doCollisionAction))]
        public static class Grass_doCollisionAction_Patch
        {
            public static void Postfix(Grass __instance, Character who)
            {
                if (!Config.ModEnabled || __instance.numberOfWeeds.Value == 4 || who is not Farmer f)
                    return;
                f.temporarySpeedBuff *= Math.Max(0, __instance.numberOfWeeds.Value) / 4f;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.OnTerrainFeatureAdded))]
        public static class GameLocation_OnTerrainFeatureAdded_Patch
        {
            public static void Postfix(GameLocation __instance, TerrainFeature feature, Vector2 location)
            {
                if (!Config.ModEnabled || feature is not Grass grass || grass.grassType.Value != 1)
                    return;
                OnAdded(grass, __instance, location);
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.OnTerrainFeatureRemoved))]
        public static class GameLocation_OnTerrainFeatureRemoved_Patch
        {
            public static void Postfix(GameLocation __instance, TerrainFeature feature)
            {
                if (!Config.ModEnabled || feature is not Grass grass || grass.grassType.Value != 1)
                    return;
                OnRemoved(grass, __instance);
            }

        }
    }
}