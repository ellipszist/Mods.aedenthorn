using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public static bool Stable_GetDefaultHorseTile_Prefix(Stable __instance, ref Point __result)
        {
            if (!Config.ModEnabled)
                return true;
            foreach (var kvp in MountDict)
            {
                var bd = __instance.GetData();
                if (bd.Name == kvp.Value.Stable)
                {
                    __result = new Point(__instance.tileX.Value + kvp.Value.SpawnOffset.X, __instance.tileY.Value + kvp.Value.SpawnOffset.Y);
                    return false;
                }
            }
            return true;
        }

        public static void Horse_Postfix(Horse __instance, Guid horseId, int xTile, int yTile)
        {
            if (!Config.ModEnabled || !MountDict.Any())
                return;
            var stable = __instance.TryFindStable();
            var bd = stable.GetData();
            foreach (var kvp in MountDict)
            {
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
            if (!MountDict.TryGetValue(key, out var data))
                return true;
            SetSprite(__instance, data);
            return false;
        }
        public static void Horse_GetBoundingBox_Postfix(Horse __instance, ref Rectangle __result)
        {
            if (!Config.ModEnabled || !__instance.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data))
                return;
           __result.Inflate(data.HorizontalSizeDiff, data.VerticalSizeDiff);
        }
        public static void Horse_SyncPositionToRider_Postfix(Horse __instance, bool ___roomForHorseAtDismountTile)
        {
            if (!Config.ModEnabled || __instance.rider is null || __instance.dismounting.Value || !__instance.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data))
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
        public static void Horse_draw_Prefix(Horse __instance, int ___munchingCarrotTimer, ref int __state)
        {
            if (!Config.ModEnabled || ___munchingCarrotTimer <= 0|| (__instance.Sprite.SpriteWidth == 32 && __instance.Sprite.SpriteHeight == 32))
                return;
            __state = ___munchingCarrotTimer;
            ___munchingCarrotTimer = 0;
        }
        public static int toggle;
        public static void Horse_draw_Postfix(Horse __instance, SpriteBatch b, ref int ___munchingCarrotTimer, int __state)
        {
            if (__state <= 0 || ++toggle < 10)
                return;
            toggle %= 20;
            float xScale = __instance.Sprite.SpriteWidth / 32f;
            float yScale = __instance.Sprite.SpriteHeight / 32f;
            switch (__instance.FacingDirection)
            {
                case 1:
                    b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(80f * xScale, -56f * yScale + 16 * (yScale - 1)), new Rectangle?(new Rectangle((int)Math.Round(179 * xScale) + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * (int)Math.Round(16 * xScale), (int)Math.Round(97 * yScale), (int)Math.Round(16 * xScale), (int)Math.Round(14 * yScale))), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (__instance.Position.Y + 64f) / 10000f + 1E-07f);
                    return;
                case 2:
                    b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(24f * xScale, -24f * yScale + 16 * (yScale - 1)), new Rectangle?(new Rectangle((int)Math.Round(170 * xScale) + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * (int)Math.Round(16 * xScale), (int)Math.Round(112 * yScale), (int)Math.Round(16 * xScale), (int)Math.Round(16 * yScale))), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (__instance.Position.Y + 64f) / 10000f + 1E-07f);
                    return;
                case 3:
                    b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(-16f * xScale, -56f * yScale + 16 * (yScale - 1)), new Rectangle?(new Rectangle((int)Math.Round(179 * xScale) + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * (int)Math.Round(16 * xScale), (int)Math.Round(97 * yScale), (int)Math.Round(16 * xScale), (int)Math.Round(14 * yScale))), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, (__instance.Position.Y + 64f) / 10000f + 1E-07f);
                    break;
                default:
                    return;
            }
            ___munchingCarrotTimer = __state;
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
            if (!Config.ModEnabled || !__instance.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data) || data.AllowHats)
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
        public static IEnumerable<CodeInstruction> Farmer_getMovementSpeed_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Farmer.getMovementSpeed");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i > 3 && codes[i].opcode == OpCodes.Ldc_R4 && codes[i - 4].opcode == OpCodes.Ldfld && ((FieldInfo)codes[i - 4].operand).Name == "ateCarrotToday")
                {
                    SMonitor.Log($"Adding method to change carrot speed bonus");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetSpeedBonus))));
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
