using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CropQuality
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.plant))]
        public static class HoeDirt_plant_Patch
        {
            public static void Postfix(HoeDirt __instance, bool isFertilizer, bool __result)
            {
                if (!Config.ModEnabled || __instance.crop is not Crop crop || crop.modData.ContainsKey(plantedKey))
                    return;
                crop.modData[plantedKey] = Config.RandomQuality ? Game1.random.Next(int.MaxValue).ToString() : Game1.stats.DaysPlayed.ToString();

            }
        }
        [HarmonyPatch(typeof(Crop), nameof(Crop.harvest))]
        public static class Crop_harvest_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Crop.harvest");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.PropertySetter(typeof(Item), nameof(Item.Quality)))
                    {
                        SMonitor.Log("Intercepting forage quality");
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetQuality))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                    else if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi2 && mi2 == AccessTools.Method(typeof(MathHelper), nameof(MathHelper.Clamp), new System.Type[] { typeof(int), typeof(int), typeof(int) }) && codes[i + 1].opcode == OpCodes.Stloc_S)
                    {
                        SMonitor.Log("Intercepting crop quality");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetQuality))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Crop), nameof(Crop.draw))]
        public static class Crop_draw_Patch
        {
            private static bool skip = false;
            public static void Postfix(Crop __instance, SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
            {
                if (!Config.ModEnabled || __instance.Dirt == null || (!__instance.forageCrop.Value && (__instance.currentPhase.Value < __instance.phaseDays.Count - 1 || (__instance.fullyGrown.Value && __instance.dayOfCurrentPhase.Value > 0))) || (Config.ShowButton != SButton.None && ((!Config.ToggleShow && !SHelper.Input.IsDown(Config.ShowButton)) ||(Config.ToggleShow && !showing.Value))))
                    return;

                if (__instance.modData.TryGetValue(numberKey, out var str) && int.TryParse(str, out int num) && num > 0)
                {
                    if (__instance.drawPosition.X > 0)
                        return;
                    for(int i = 1; i <= num; i++)
                    {
                        Vector2 position = Game1.GlobalToLocal(Game1.viewport, __instance.drawPosition * -1) - new Vector2(24, 32);

                        int quality = GetQuality(__instance, i);
                        if(quality > 0)
                            DrawQuality(b, __instance, quality, position, toTint.A / 255f);

                    }
                }
                else if(__instance.drawPosition.X >= 0)
                {
                    Vector2 position = Game1.GlobalToLocal(Game1.viewport, __instance.drawPosition);
                    int quality = GetQuality(__instance);
                    if(quality > 0)
                    {
                        DrawQuality(b, __instance, quality, position, toTint.A / 255f);
                    }
                }

            }

            private static void DrawQuality(SpriteBatch b, Crop instance, int quality, Vector2 position, float alpha)
            {
                Rectangle qualityRect = ((quality < 4) ? new Rectangle(338 + (quality - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8));
                Texture2D qualitySheet = Game1.mouseCursors;
                b.Draw(qualitySheet, position + new Vector2(8,8), new Rectangle?(qualityRect), Color.White * alpha, 0f, Vector2.Zero, 4f * Config.Scale, SpriteEffects.None, instance.layerDepth + 11 / 10000f);
            }
        }
    }
}