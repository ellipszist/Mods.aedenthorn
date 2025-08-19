using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Character), nameof(Character.draw), new Type[] { typeof(SpriteBatch), typeof(float) })]
        public static class Character_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Character.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.IntToLocalY))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GreenSlime), nameof(GreenSlime.draw), new Type[] { typeof(SpriteBatch) })]
        public static class GreenSlime_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GreenSlime.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)) && codes[i - 1].opcode == OpCodes.Call && codes[i - 1].operand is MethodInfo && (MethodInfo)codes[i - 1].operand == AccessTools.PropertyGetter(typeof(Character), nameof(Character.StandingPixel)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.IntToLocalY))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(BigSlime), nameof(BigSlime.draw), new Type[] { typeof(SpriteBatch) })]
        public static class BigSlime_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling BigSlime.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)) && codes[i - 1].opcode == OpCodes.Call && codes[i - 1].operand is MethodInfo && (MethodInfo)codes[i - 1].operand == AccessTools.PropertyGetter(typeof(Character), nameof(Character.StandingPixel)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.IntToLocalY))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(DinoMonster), nameof(DinoMonster.draw), new Type[] { typeof(SpriteBatch) })]
        public static class DinoMonster_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling DinoMonster.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)) && codes[i - 1].opcode == OpCodes.Call && codes[i - 1].operand is MethodInfo && (MethodInfo)codes[i - 1].operand == AccessTools.PropertyGetter(typeof(Character), nameof(Character.StandingPixel)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.IntToLocalY))));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Grass), nameof(Grass.draw))]
        public static class Grass_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Grass.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.X)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.FloatToLocalX))));
                        i++;
                    }
                    else if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.Y)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.FloatToLocalY))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Tree), nameof(Tree.draw))]
        public static class Tree_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Tree.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.PropertyGetter(typeof(Rectangle), nameof(Rectangle.Bottom)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.IntToLocalY))));
                        i++;
                    }
                    else if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.X)) && codes[i + 1].opcode == OpCodes.Ldc_R4 && codes[i + 2].opcode == OpCodes.Div)
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.FloatToLocalXTile))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Chest_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Chest.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (CodesCompare(codes, i, new OpCode[] { OpCodes.Ldc_R4, OpCodes.Ldloc_1, OpCodes.Ldc_R4, OpCodes.Add, OpCodes.Ldc_R4, OpCodes.Mul, OpCodes.Ldc_R4, OpCodes.Sub, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Call, OpCodes.Ldloc_0, OpCodes.Ldc_R4, OpCodes.Mul, OpCodes.Add, OpCodes.Stloc_2 }))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");

                        codes.Insert(i + 15, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetChestDrawLayer))));
                        codes.Insert(i + 15, new CodeInstruction(OpCodes.Ldarg_3));
                        codes.Insert(i + 15, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 15, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Object_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Object.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (CodesCompare(codes, i, new OpCode[] { OpCodes.Ldc_R4, OpCodes.Ldarg_3, OpCodes.Ldc_I4_1, OpCodes.Add, OpCodes.Ldc_I4_S, OpCodes.Mul, OpCodes.Ldc_I4_S, OpCodes.Sub, OpCodes.Conv_R4, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Call, OpCodes.Ldarg_2, OpCodes.Conv_R4, OpCodes.Ldc_R4, OpCodes.Mul, OpCodes.Add, OpCodes.Stloc_S }))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");

                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetObjectDrawLayer))));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_3));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 21;
                    }
                    else if (CodesCompare(codes, i, new OpCode[] { OpCodes.Ldc_R4, OpCodes.Ldarg_3, OpCodes.Ldc_I4_1, OpCodes.Add, OpCodes.Ldc_I4_S, OpCodes.Mul, OpCodes.Ldc_I4_2, OpCodes.Add, OpCodes.Conv_R4, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Call, OpCodes.Ldarg_2, OpCodes.Conv_R4, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Add, OpCodes.Stloc_S }))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");

                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetObjectDrawLayer2))));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_3));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 21;
                    }
                    else if (CodesCompare(codes, i, new OpCode[] { OpCodes.Ldarg_3, OpCodes.Ldc_I4_1, OpCodes.Add, OpCodes.Ldc_I4_S, OpCodes.Mul, OpCodes.Conv_R4, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Ldarg_0, OpCodes.Ldfld, OpCodes.Callvirt, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Add, OpCodes.Stloc_S }))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");

                        codes.Insert(i + 14, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetObjectDrawLayer3))));
                        codes.Insert(i + 14, new CodeInstruction(OpCodes.Ldarg_3));
                        codes.Insert(i + 14, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 14, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 15;
                    }
                    else if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Object), nameof(Object.GetBoundingBoxAt)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");

                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetObjectBoundingBox))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_3));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 4;
                    }
                }
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.getDrawLayer))]
        public static class Farmer_getDrawLayer_Patch
        {
            public static bool Prefix(Farmer __instance, ref float __result)
            {
                if (!Config.ModEnabled || __instance.currentLocation != openWorldLocation)
                    return true;
                int rsp = __instance.StandingPixel.Y % (openWorldChunkSize * 64) + (Game1.viewport.Y / (openWorldChunkSize * 64) < __instance.StandingPixel.Y / (openWorldChunkSize * 64) ? openWorldChunkSize * 64 : 0);
                if (__instance.onBridge.Value)
                {
                    __result = rsp / 10000f + __instance.drawLayerDisambiguator + 0.0256f;
                }
                else if (__instance.IsSitting() && __instance.mapChairSitPosition.Value.X != -1f && __instance.mapChairSitPosition.Value.Y != -1f)
                {
                    Vector2 sit_position = __instance.mapChairSitPosition.Value;
                    __result = ((sit_position.Y % (openWorldChunkSize * 128) + 1f) * 64f + 3141) / 10000f;
                }
                else
                    __result = rsp / 10000f + __instance.drawLayerDisambiguator;
                return false;
            }
        }
    }
}