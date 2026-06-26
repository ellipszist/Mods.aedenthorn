using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace Tutorials
{
    public partial class ModEntry
    {
        public static void OpenTutorial(string key, List<string> cats = null)
        {
            if (!Context.IsWorldReady)
                return;
            foreach (var k in Config.OpenTutorialKey.Keybinds)
            {
                foreach (var b in k.Buttons)
                {
                    SHelper.Input.Suppress(b);
                }
            }
            Game1.playSound("bigSelect");
            Game1.activeClickableMenu = new CustomTutorialMenu(key, cats);
        }
    }
}