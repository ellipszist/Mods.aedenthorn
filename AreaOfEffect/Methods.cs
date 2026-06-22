using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace AreaOfEffect
{
    public partial class ModEntry
    {
        public static void OpenTutorial(string key)
        {
            if (!Context.IsWorldReady || !TutorialDict.TryGetValue(key, out var data))
                return;
            Game1.activeClickableMenu = new CustomTutorialMenu(data);
        }
    }
}