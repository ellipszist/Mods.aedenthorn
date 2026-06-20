using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using System;

namespace ShowMissingCollectionEntries
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
                return Config.MissingRecipeTint;
            }
            return Color.White;
        }

        private void ReloadCollections()
        {
            if (!Config.ModEnabled || Game1.activeClickableMenu is not GameMenu gm)
                return;
            for(int i = 0; i < gm.pages.Count; i++)
            {
                if (gm.pages[i] is CollectionsPage cp)
                {
                    gm.pages[i] = new CollectionsPage(cp.xPositionOnScreen, cp.yPositionOnScreen, cp.width, cp.height);
                    if(i == gm.currentTab)
                    {
                        gm.pages[i].populateClickableComponentList();
                        gm.AddTabsToClickableComponents(gm.pages[i]);
                    }
                }
            }
        }

    }
}