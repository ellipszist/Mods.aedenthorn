using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace CustomSplashScreen
{
	public partial class ModEntry : Mod
    {
        public static Color ChangeColor(Color color, SpriteBatch b)
        {
            if(!Config.ModEnabled)
                return color;
            if(splashBackground != null)
            {
                var menu = (Game1.activeClickableMenu as TitleMenu);
                float opacity = (menu.fadeFromWhiteTimer / 4000f);
                b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * opacity); 
                float xdiff = Game1.viewport.Width / (float)splashBackground.Width;
                float ydiff = Game1.viewport.Height / (float)splashBackground.Height;
                if (xdiff > ydiff) 
                {
                    int x = (int)(Game1.viewport.Width - splashBackground.Width * ydiff) / 2;
                    b.Draw(splashBackground, new Rectangle(x, 0, (int)(splashBackground.Width * ydiff), Game1.viewport.Height), Color.White * opacity); 
                }
                else
                {
                    int y = (int)(Game1.viewport.Height - splashBackground.Height * xdiff) / 2;
                    b.Draw(splashBackground, new Rectangle(0, y, Game1.viewport.Width, (int)(splashBackground.Height * xdiff)), Color.White * opacity); 

                }
                return Color.Transparent;
            }
            if(Config.BackgroundColor.A == 0)
            {
                return Utility.GetPrismaticColor(0, 2);
            }
            return Config.BackgroundColor;
        }
        public static double ChangeAltSurpriseChance(double chance)
        {
            if(!Config.ModEnabled)
                return chance;
            return Config.AltSurpriseChance;
        }
    }
}
