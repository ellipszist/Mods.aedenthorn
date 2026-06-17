using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace GiantCropInteriors
{
    public partial class ModEntry
    {
        public static bool AchievementAchieved(bool value)
        {
            if (!Config.ModEnabled || !Config.ShowMissingAchievementNames)
                return value;
            return true;
        }
        public static Color ModifyColor(Color color, CollectionsPage page)
        {
            if (!Config.ModEnabled || !Config.ShowMissingIcons || page.currentTab == 5)
                return color;
            if(page.currentTab == 4)
            {
                return new Color(100, 50, 50, 255);
            }
            return Color.White;
        }
    }
}