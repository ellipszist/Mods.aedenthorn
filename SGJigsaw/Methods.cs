using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace SGJigsaw
{
	public partial class ModEntry : Mod
    {
        private void DrawMenuSlot(SpriteBatch b, Rectangle rectangle)
        {
            var text = SHelper.Translation.Get("menu-text");
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Color.White, 4f, false, -1f);
            b.DrawString(Game1.dialogueFont, text, rectangle.Location.ToVector2() + new Vector2(14, 18), Color.Black);
            b.DrawString(Game1.dialogueFont, text, rectangle.Location.ToVector2() + new Vector2(16, 16), Color.White);
        }

        private void LoadGame()
        {
            if(Config.ModEnabled)
                TitleMenu.subMenu = new JigsawGameMenu(null);
        }
    }
}
