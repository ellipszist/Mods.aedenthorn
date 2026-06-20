
using Microsoft.Xna.Framework;

namespace ShowMissingCollectionEntries
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ShowMissingIcons { get; set; } = true;
        public bool ShowMissingItemNames { get; set; } = true;
        public bool ShowMissingItemDetails { get; set; } = false;
        public bool ShowMissingAchievementNames { get; set; } = true;
        public bool ShowMissingAchievementDetails { get; set; } = false;
        public Color MissingRecipeTint { get; set; } = new Color(100, 50, 50, 255);
        public bool Debug { get; set; } = false;
    }
}
