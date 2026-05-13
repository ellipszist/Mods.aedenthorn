using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using Object = StardewValley.Object;

namespace LetterBlocks
{
	public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Object_draw_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || !TryGetBlockData(__instance.ItemId, out var data))
                    return;
                if (!__instance.modData.TryGetValue(letterKey, out var letterData))
                {
                    letterData = $"0,0,{data.DefaultColor}";
                    __instance.modData[letterKey] = letterData;
                }
                __instance.modData[letterKey] = letterData;
                var font = SHelper.GameContent.Load<SpriteFont>(data.FontPath) ?? Game1.dialogueFont;
                var split = letterData.Split(',');
                var letter = data.Letters[int.Parse(split[0])][int.Parse(split[1])].ToString();
                var size = font.MeasureString(letter) * data.FontScale;
                var pos = Game1.GlobalToLocal(new Vector2(x, y) * 64 + new Vector2(32 - size.X / 2, 32 - size.Y / 2));
                var layer = (__instance.GetBoundingBoxAt(x, y).Center.Y + 1) / 10000f;
                var color = DiscreteColorPicker.getColorFromSelection(int.Parse(split[2]));
                spriteBatch.DrawString(font, letter, pos, color, 0, Vector2.Zero, data.FontScale, SpriteEffects.None, layer);

            }
        }
    }
}
