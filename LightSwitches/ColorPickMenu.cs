using LightSwitches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace LightSwitches
{
    internal class ColorPickMenu : IClickableMenu
    {
        private Furniture f;
        private ColorPicker picker;
        private Color lastColor;
        private bool held;
        private IClickableMenu activeClickableMenu;
        public ColorPickMenu(Furniture _f, IClickableMenu oldMenu = null)
        {
            activeClickableMenu = oldMenu;
            f = _f;
            width = SliderBar.defaultWidth + 160;
            height = 144;
            if (!f.modData.TryGetValue(ModEntry.colorKey, out var str))
            {
                str = "#FFFFFF";
                f.modData[ModEntry.colorKey] = str;
            }
            lastColor = Utility.StringToColor(str) ?? Color.White;
            RecreatePicker();
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            RecreatePicker();
            base.gameWindowSizeChanged(oldBounds, newBounds);
        }

        private void RecreatePicker()
        {
            upperRightCloseButton = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width / 2 + width / 2, Game1.uiViewport.Height / 2 - 64, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
            picker = new ColorPicker("Color", Game1.uiViewport.Width / 2 + width / 2 - SliderBar.defaultWidth - 72, Game1.uiViewport.Height / 2 - 114);

            picker.setColor(Utility.StringToColor(f.modData[ModEntry.colorKey]) ?? Color.White);
        }

        public override void releaseLeftClick(int x, int y)
        {
            picker.releaseClick();
            held = false;
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if(upperRightCloseButton.containsPoint(x, y))
            {
                base.receiveLeftClick(x, y, playSound);
            }
            else
            {
                ChangeColor(picker.click(x, y));
                held = true;
            }
        }
        public override void leftClickHeld(int x, int y)
        {
            if (held)
            {
                ChangeColor(picker.clickHeld(x, y));
                held = true;
            }
            else
            {
                held = false;
            }
        }
        private void ChangeColor(Color color)
        {
            lastColor = color;
            f.modData[ModEntry.colorKey] = ModEntry.ColorToHexString(color);
            if(f.Location != null && f.modData.TryGetValue(ModEntry.onKey, out var on) && on == "on")
            {
                f.Location.modData[ModEntry.colorKey] = ModEntry.ColorToHexString(color);
            }
        }
        public override void draw(SpriteBatch b)
        {
            if(activeClickableMenu != null)
            {
                activeClickableMenu.draw(b);
            }
            upperRightCloseButton.bounds = new Rectangle(Game1.uiViewport.Width / 2 + width / 2, Game1.uiViewport.Height / 2 - 144, 48, 48);
            base.draw(b);
            drawTextureBox(b, Game1.uiViewport.Width / 2 - width / 2, Game1.uiViewport.Height / 2 - width / 2, width, height - 24, Color.White);
            
            b.Draw(Game1.staminaRect, new Rectangle(Game1.uiViewport.Width / 2 - width / 2 + 24, Game1.uiViewport.Height / 2 - width / 2 + 36, 48, 48), null, lastColor);
            picker.draw(b);
            drawMouse(b);
        }
        public override bool readyToClose()
        {
            if(activeClickableMenu != null)
            {
                Game1.activeClickableMenu = activeClickableMenu;
                Game1.playSound("bigDeSelect");
                return false;
            }
            return base.readyToClose();
        }
    }
}