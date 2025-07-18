using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
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
        public static IEnumerable<CodeInstruction> Farmer_updateMovementAnimation_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Farmer.updateMovementAnimation");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if(codes[i].opcode == OpCodes.Ldc_I4_2 && codes[i + 1].opcode == OpCodes.Call)
                {
                    SMonitor.Log($"Adding method to change mount speed");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetSpeed))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    break;
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
    }
}
