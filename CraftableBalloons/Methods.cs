using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;

namespace CraftableBalloons
{
    public partial class ModEntry : Mod
    {
        public static Color MakeColor(string v)
        {
            return v.StartsWith("#") && v.Length == 7 ? new Color(Convert.ToByte(v.Substring(1, 2), 16), Convert.ToByte(v.Substring(3, 2), 16), Convert.ToByte(v.Substring(5, 2), 16)) : Color.White;
        }
        public static string MakeColorString(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static void DrawBalloon(SpriteBatch b, Color color, Vector2 position, int facing, Point vel, int standing)
        {

            int up = 0;
            int right = 0;
            int max = 10;
            int div = 4;
            int height = 12;
            int segments = 1;
            if (vel != Point.Zero)
            {
                up = MathHelper.Clamp(vel.Y / div, -max, max) / 2;

                right = MathHelper.Clamp(vel.X / div, -max, max);
                height -= Math.Abs(up);
                segments = Math.Min(Math.Abs(right) + 1, height);
            }
            else
            {
                right += (int)Math.Round(Math.Sin(Game1.ticks / 50f));
            }

            var tex = SHelper.GameContent.Load<Texture2D>(balloonPath);
            Vector2 offset = new Vector2(-18 - segments * 4 * Math.Sign(right), -128 + (12 - height) * 4);
            float drawOffset = (standing + (up <= 0 ? 1 : 0)) / 10000f;
            switch (facing)
            {
                case 1:
                    if (up >= 0)
                        drawOffset = (standing) / 10000f;
                    offset += new Vector2(0, -8);
                    break;
                case 3:
                    if(up >= 0)
                        drawOffset = (standing) / 10000f;
                    offset += new Vector2(32, -8);
                    break;
                case 2:
                    if (up >= 0)
                    {
                        drawOffset = (standing) / 10000f;
                    }
                    offset += new Vector2(32, -8);
                    break;
            }
            Vector2 pos = Game1.GlobalToLocal(position + offset);
            b.Draw(tex, pos, new Rectangle(0, 16, 16, 16), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, drawOffset);
            b.Draw(tex, pos, new Rectangle(16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, drawOffset + 1/10000f);
            b.Draw(tex, pos + new Vector2(0, 64), new Rectangle(16, 16, 16, 1), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, drawOffset);

            int segmentHeight = (int)Math.Max(height / (float)segments, 1);
            for(int i = 0; i < segments; i++)
            {
                b.DrawString(Game1.dialogueFont, $"{segments}, {segmentHeight}, {height}", Vector2.Zero, Color.White);
                b.Draw(tex, pos + new Vector2(i * 4 * Math.Sign(right), 68 + segmentHeight * i * 4), new Rectangle(16, 17, 16, segmentHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, drawOffset);
            }
        }
    }
}
