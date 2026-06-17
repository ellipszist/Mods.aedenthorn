
using Microsoft.Xna.Framework;

namespace ShowMissingCollectionEntries
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ShowMissingDetails { get; set; } = true;
        public bool ShowMissingIcons { get; set; } = true;
        public bool ShowMissingAchievementDetails { get; set; } = true;
        public bool Debug { get; set; } = false;
    }
}
