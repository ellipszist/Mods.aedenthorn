using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;

namespace QuestHelper
{
	public partial class ModEntry : Mod
    {

        public static void DrawQuestMarker(SpriteBatch b, Vector2 position)
        {
            float yOffset2 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(32, yOffset2 - 96)), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(395, 497, 3, 8)), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset2 / 16f), SpriteEffects.None, 1f);
        }

        public static bool IsSlimeName(string s)
        {
            return s.Contains("Slime") || s.Contains("Jelly") || s.Contains("Sludge");
        }
    }
}
