using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace StardewGames
{
	public partial class ModEntry : Mod
    {

        public static void DrawPrairieKing(SpriteBatch b, Rectangle rectangle)
        {
            var text = SHelper.Translation.Get("prairie-text");
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Color.White, 4f, false, -1f);
            //b.Draw(SHelper.ModContent.Load<Texture2D>("assets/prairieMenu.png"), new Rectangle(rectangle.Location + new Point(8, 8), rectangle.Size - new Point(16, 16)), Color.White);
            SpriteText.drawStringHorizontallyCenteredAt(b, text, Game1.viewport.Width / 2 + 64, rectangle.Y + 64, color: Color.LightGoldenrodYellow);
        }

        public static void ClickPrairieKing(int x, int y)
        {
            var mpos = Game1.getMousePosition();
            Game1.currentSong?.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
            Game1.currentMinigame = new AbigailGame(null);
            currentMiniGame = CurrentMiniGame.PrairieKing;
        }
        public static void DrawJunimo(SpriteBatch b, Rectangle rectangle)
        {
            var text = SHelper.Translation.Get("junimo-text");
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Color.White, 4f, false, -1f);
            //b.Draw(SHelper.ModContent.Load<Texture2D>("assets/junimoMenu.png"), new Rectangle(rectangle.Location + new Point(8, 8), rectangle.Size - new Point(16, 16)), Color.White);
            SpriteText.drawStringHorizontallyCenteredAt(b, text, Game1.viewport.Width / 2 + 64, rectangle.Y + 64, color: Color.LightGoldenrodYellow);
        }

        public static void ClickJunimo(int x, int y)
        {
        }
    }
}
