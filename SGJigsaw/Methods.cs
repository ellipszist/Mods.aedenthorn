using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using System;
using System.Runtime.CompilerServices;
using xTile;
using xTile.Tiles;

namespace SGJigsaw
{
	public partial class ModEntry : Mod
    {
        private void DrawMenuSlot(SpriteBatch b, Rectangle rectangle)
        {
            var text = SHelper.Translation.Get("menu-text");
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Color.White, 4f, false, -1f);
            b.Draw(SHelper.ModContent.Load<Texture2D>("assets/menu.png"), new Rectangle(rectangle.Location + new Point(8, 8), rectangle.Size - new Point(16,16)), Color.White);
            float rot = (float)Math.Sin(Game1.gameModeTicks / 100f);
            SpriteText.drawStringHorizontallyCenteredAt(b, text, Game1.viewport.Width / 2 + 64, rectangle.Y + 64, color: Color.LightGoldenrodYellow);
            //b.DrawString(Game1.dialogueFont, text, rectangle.Location.ToVector2() + new Vector2(14, 18), Color.Black, rot, new Vector2(10f, 10f), 4, SpriteEffects.None, 1);
            //b.DrawString(Game1.dialogueFont, text, rectangle.Location.ToVector2() + new Vector2(16, 16), Color.White);
        }

        private void LoadGame()
        {
            if(Config.ModEnabled)
                TitleMenu.subMenu = new JigsawGameMenu(null);
        }

    }
}
