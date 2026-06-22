using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Schema;

namespace AreaOfEffect
{
    internal class CustomTutorialMenu : IClickableMenu
    {
        private AOEToolData data;
        private int currentPage;
        private List<ClickableTextureComponent> pageDots = new();
        private ClickableTextureComponent texture;
        private ClickableTextureComponent leftArrow;
        private ClickableTextureComponent rightArrow;
        public CustomTutorialMenu(AOEToolData data) : base(Game1.uiViewport.Width / 2 - (600 + borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + borderWidth * 2) / 2 - 192, 600 + borderWidth * 2, 600 + borderWidth * 2 + 192, true)
        {
            data.Frames.Add(new()
            {
                Texture = "LooseSprites/Movies",
                SourceRect = new Rectangle(112, 768, 90, 61),
                Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
            });
            data.Frames.Add(new()
            {
                Texture = "LooseSprites/Movies",
                SourceRect = new Rectangle(208, 768, 90, 61),
                Text = "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat."
            });
            data.Frames.Add(new()
            {
                Texture = "LooseSprites/Movies",
                SourceRect = new Rectangle(304, 768, 90, 61),
                Text = "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur."
            });
            data.Frames.Add(new()
            {
                Texture = "LooseSprites/Movies",
                SourceRect = new Rectangle(400, 768, 90, 61),
                Text = "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
            });
            this.data = data;
            Recalibrate();
        }
        private void Recalibrate()
        {
            int size = 800;
            xPositionOnScreen = Game1.uiViewport.Width / 2 - (size + borderWidth * 2) / 2;
            yPositionOnScreen = Game1.uiViewport.Height / 2 - (size + borderWidth * 2) / 2;
            width = size + borderWidth * 2;
            height = size + borderWidth * 2;

            upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 36, yPositionOnScreen + 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false)
            {
                myID = 9175502
            };

            int space = 48;
            pageDots.Clear();
            for (int i = 0; i < data.Frames.Count; i++)
            {
                int pos = xPositionOnScreen + width / 2 - data.Frames.Count * space / 2 + i * space;
                pageDots.Add(new(new(pos, yPositionOnScreen + height - 80, 28, 36), Game1.mouseCursors, new(129 + (i == currentPage ? 8 : 0), 338, 7, 9), 4f));
            }
            rightArrow = new(new(xPositionOnScreen + width, yPositionOnScreen + height / 2, 44, 40), Game1.mouseCursors, new(12, 204, 44, 40), 1f);
            leftArrow = new(new(xPositionOnScreen - 44, yPositionOnScreen + height / 2, 44, 40), Game1.mouseCursors, new(8, 268, 44, 40), 1f);

            var d = data.Frames[currentPage];
            if (d.Texture is string tstr)
            {
                var tex = ModEntry.SHelper.GameContent.Load<Texture2D>(tstr);
                Rectangle source = d.SourceRect ?? new(0, 0, tex.Width, tex.Height);
                float x = xPositionOnScreen;
                float y = yPositionOnScreen + 100;
                float w = width - borderWidth / 2;
                float h = height / 2 - 64;
                float xr = source.Width * d.Scale / w; // 200 / 600
                float yr = source.Height * d.Scale / h; // 80 / 300
                float scale = d.Scale;
                if (xr > yr)
                {
                    scale /= xr;
                    y += h / 2;
                    y -= (source.Height * scale) / 2;
                }
                else
                {
                    scale /= yr;
                    x += w / 2;
                    x -= (source.Width * scale) / 2;
                }
                texture = new(new Rectangle((int)x, (int)y, (int)w, (int)h), tex, data.Frames[currentPage].SourceRect ?? new(), scale);
            }
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            Recalibrate();
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if(upperRightCloseButton.containsPoint(x, y))
            {
                exitThisMenu();
            }
            for(int i = 0; i < pageDots.Count; i++)
            {
                if (pageDots[i].containsPoint(x, y))
                {
                    Game1.playSound("shwip");
                    currentPage = i;
                    Recalibrate();
                }
            }
        }
        public override void receiveKeyPress(Keys key)
        {
            if(Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
            {
                currentPage--;
                if (currentPage < 0)
                    currentPage = data.Frames.Count - 1;
                Game1.playSound("shwip");
                Recalibrate();
                return;
            }
            if(Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
            {
                currentPage++;
                if (currentPage >= data.Frames.Count)
                    currentPage = 0;
                Game1.playSound("shwip");
                Recalibrate();
                return;
            }
            base.receiveKeyPress(key);
        }
        public override void performHoverAction(int x, int y)
        {
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, true, -1, -1, -1);
            upperRightCloseButton.draw(b);
            foreach(var cc in pageDots)
            {
                cc.draw(b);
            }
            texture?.draw(b);
            var yPosition = yPositionOnScreen + height / 2 + 12;
            b.Draw(Game1.menuTexture, new Vector2((float)this.xPositionOnScreen, (float)yPosition), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 4, -1, -1)), Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + 64, yPosition, this.width - 128, 64), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 6, -1, -1)), Color.White);
            b.Draw(Game1.menuTexture, new Vector2((float)(this.xPositionOnScreen + this.width - 64), (float)yPosition), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 7, -1, -1)), Color.White);
            if (data.Frames[currentPage].Text is string str)
            {
                var size = Game1.dialogueFont.MeasureString(str);
                SpriteText.drawString(b, str, xPositionOnScreen + spaceToClearSideBorder * 4, yPosition + 64, 999999, width - spaceToClearSideBorder * 8, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
            }
            leftArrow.draw(b);
            rightArrow.draw(b);
            drawMouse(b, false, -1);
        }
    }
}