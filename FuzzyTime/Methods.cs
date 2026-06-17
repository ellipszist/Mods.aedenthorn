using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;
using System.Text;

namespace FuzzyTime
{
    public partial class ModEntry
    {
        public static void DrawTextWithShadow(SpriteBatch b, StringBuilder text, SpriteFont font, Vector2 position, Color color, float scale , float layerDepth , int horizontalShadowOffset , int verticalShadowOffset , float shadowIntensity , int numShadows, DayTimeMoneyBox box)
        {
            if (!Config.ModEnabled || text != AccessTools.FieldRefAccess<DayTimeMoneyBox, StringBuilder>(box, "_timeText"))
            {
                Utility.drawTextWithShadow(b, text, font, position, color, scale, layerDepth, horizontalShadowOffset, verticalShadowOffset, shadowIntensity, numShadows);
                return;
            }
            font = Game1.smallFont;
            var str = GetTimeText();
            Vector2 txtSize = font.MeasureString(str);

            var sourceRect = AccessTools.FieldRefAccess<DayTimeMoneyBox, Rectangle>(box, "sourceRect");

            Vector2 timePosition = new Vector2((float)sourceRect.X * 0.565f - txtSize.X / 2f + (float)((box.timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0), (float)sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.31f : 0.31f) - txtSize.Y / 2f + (float)((box.timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0));
            Utility.drawTextWithShadow(b, str, font, box.position + timePosition, color, scale, layerDepth, horizontalShadowOffset, verticalShadowOffset, shadowIntensity, numShadows);
        }

        private static string GetTimeText()
        {
            var keys = Config.TimeNames.Keys.ToList();
            keys.Sort();
            keys.Reverse();
            int key;
            int maxLength = 12;
            try
            {
                key = keys.First(k => k <= Game1.timeOfDay);
            }
            catch
            {
                key = keys.Last();
            }
            var str = Config.TimeNames[key];
            if (SHelper.Translation.ContainsKey(str))
            {
                str = SHelper.Translation.Get(str);
            }
            if(str.Length > maxLength)
            {
                var temp = str + "  ";
                float speed = 500;
                var tick = Game1.currentGameTime.TotalGameTime.TotalMilliseconds / speed;
                var offset = (int)(tick % temp.Length);
                temp = temp.Substring(offset, Math.Min(maxLength, temp.Length - offset));
                if (temp.Length < maxLength)
                {
                    temp += str.Substring(0, maxLength - temp.Length);
                }
                str = temp;
            }
            return str;
        }
    }
}