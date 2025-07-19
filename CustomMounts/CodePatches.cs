using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomMounts
{
    public partial class ModEntry
    {
        public static bool NPC_behaviorOnFarmerLocationEntry_Prefix(NPC __instance)
        {
            if (!Config.ModEnabled || __instance.Sprite == null|| !__instance.modData.TryGetValue(modKey, out var key))
                return true;
            __instance.Sprite.currentFrame = 0;
            return false;
        }

        public static void Horse_Postfix(Horse __instance, Guid horseId, int xTile, int yTile)
        {
            if (!Config.ModEnabled || !mountDict.Any())
                return;
            var stable = __instance.TryFindStable();
            foreach(var kvp in mountDict)
            {
                var bd = stable.GetData();
                if (bd.Name == kvp.Value.Stable)
                {
                    __instance.modData[modKey] = kvp.Key;
                    __instance.Name = kvp.Value.Name;
                    __instance.displayName = kvp.Value.Name;
                    SetSprite(__instance, kvp.Value);
                }
            }
        }
        public static bool Horse_ChooseAppearance_Prefix(Horse __instance)
        {
            if (!Config.ModEnabled || !__instance.modData.TryGetValue(modKey, out var key))
                return true;
            if (__instance.Sprite != null && __instance.spriteOverridden)
                return true;
            if(mountDict is null)
            {
                mountDict = SHelper.GameContent.Load<Dictionary<string, MountData>>(dictPath);
            }
            if (!mountDict.TryGetValue(key, out var data))
                return true;
            SetSprite(__instance, data);
            return false;
        }
        public static void Horse_GetBoundingBox_Postfix(Horse __instance, ref Rectangle __result)
        {
            if (!Config.ModEnabled || !__instance.modData.TryGetValue(modKey, out var key) || !mountDict.TryGetValue(key, out var data))
                return;
           __result.Inflate(data.HorizontalSizeDiff, data.VerticalSizeDiff);
        }
        public static void Horse_SyncPositionToRider_Postfix(Horse __instance, bool ___roomForHorseAtDismountTile)
        {
            if (!Config.ModEnabled || __instance.rider is null || __instance.dismounting.Value || !__instance.modData.TryGetValue(modKey, out var key) || !mountDict.TryGetValue(key, out var data))
                return;
            __instance.Position += new Vector2(-data.HorizontalSizeDiff * 2, data.VerticalSizeDiff);
            if(__instance.Position.X < 0)
            {
                __instance.rider.Position -= new Vector2(__instance.Position.X, 0);
                __instance.Position -= new Vector2(__instance.Position.X, 0);
            }
        }
        public static IEnumerable<CodeInstruction> Horse_PerformDefaultHorseFootstep_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Horse.PerformDefaultHorseFootstep");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr)
                {
                    string which = (string)codes[i].operand;
                    switch (which)
                    {
                        case "thudStep":
                        case "woodyStep":
                        case "stoneStep":
                            SMonitor.Log($"Adding method to change {which}");
                            codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetStepSound))));
                            codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                            i += 2;
                            break;
                        default:
                            break;
                    }
                }
            }

            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> Horse_draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Horse.draw");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if(codes[i].opcode == OpCodes.Callvirt && (MethodInfo) codes[i].operand == AccessTools.Method(typeof(Hat), nameof(Hat.draw)))
                {
                    SMonitor.Log($"Overriding hat draw method");
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawHat));
                    codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    i++;
                }
            }

            return codes.AsEnumerable();
        }
        public static bool Horse_checkAction_Prefix(Horse __instance, Farmer who)
        {
            if (!Config.ModEnabled || !__instance.modData.TryGetValue(modKey, out var key) || !mountDict.TryGetValue(key, out var data) || data.AllowHats)
                return true;
            if (who != null && who.canMove || __instance.rider == null && who.mount == null && !who.FarmerSprite.PauseForSingleAnimation && __instance.currentLocation == who.currentLocation)
            {
                Stable stable = __instance.TryFindStable();
                if (stable != null && __instance.getOwner() == Game1.player && who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] is Hat)
                {
                    SMonitor.Log($"Preventing hat wearing");

                    return false;
                }
            }
            return true;
        }
        public static IEnumerable<CodeInstruction> Horse_checkAction_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Horse.checkAction");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if(i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldc_I4_0 && codes[i + 1].opcode == OpCodes.Conv_I8 && codes[i + 2].opcode == OpCodes.Callvirt)
                {
                    SMonitor.Log($"Preventing ownership erasure");
                    codes[i + 2].opcode = OpCodes.Call;
                    codes[i + 2].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.PreventOwnershipErasure));
                    break;
                }
            }

            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> Horse_checkAction2_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Horse.checkAction2");

            var codes = new List<CodeInstruction>(instructions);
            int which = -1;
            for (int i = 0; i < codes.Count; i++)
            {
                if (which < 0 && codes[i].opcode == OpCodes.Ldfld && ((FieldInfo)codes[i].operand).Name.EndsWith("__this"))
                    which = i;
                if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "(O)Carrot")
                {
                    SMonitor.Log($"Overriding carrot item");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetCarrotItem))));
                    codes.Insert(i + 1, codes[which].Clone());
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    i += 2;
                }
                else if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "eat")
                {
                    SMonitor.Log($"Overriding eat sound");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetEatSound))));
                    codes.Insert(i + 1, codes[which].Clone());
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    i += 2;
                }
            }

            return codes.AsEnumerable();
        }
        
        public static IEnumerable<CodeInstruction> Object_performUseAction_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Object.performUseAction");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if(codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "(O)911")
                {
                    SMonitor.Log($"Overriding flute item");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetFluteItem))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    i += 2;
                }
                else if(codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "horse_flute")
                {
                    SMonitor.Log($"Overriding flute sound");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetFluteSound))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    i += 2;
                }
            }
            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> Farmer_updateMovementAnimation_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Farmer.updateMovementAnimation");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_2 && codes[i + 1].opcode == OpCodes.Call)
                {
                    SMonitor.Log($"Adding method to change mount speed");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetSpeed))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    break;
                }
            }

            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> FarmerTeam_OnRequestHorseWarp_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling FarmerTeam_OnRequestHorseWarp");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 4].opcode == OpCodes.Call && ((MethodInfo)codes[i + 4].operand).Name.Contains("ForEachBuilding"))
                {
                    SMonitor.Log($"Reworking horse finding code");
                    codes.RemoveRange(i, 5);
                    codes[i - 2] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetHorseForFlute)));
                    codes.Insert(i  - 2, new CodeInstruction(OpCodes.Ldarg_1));
                    break;
                }
            }

            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> Game1_UpdateHorseOwnership_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Game1_UpdateHorseOwnership");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldc_I4_0 && codes[i + 1].opcode == OpCodes.Conv_I8 && codes[i + 2].opcode == OpCodes.Callvirt)
                {
                    SMonitor.Log($"Preventing ownership erasure");
                    codes[i + 2].opcode = OpCodes.Call;
                    codes[i + 2].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.PreventOwnershipErasure));
                    break;
                }
            }

            return codes.AsEnumerable();
        }

    }
}
