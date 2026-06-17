
using Microsoft.Xna.Framework;

namespace GiantCropInteriors
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ShowMissingItemNames { get; set; } = true;
        public bool ShowMissingItemDetails { get; set; } = false;
        public bool ShowMissingIcons { get; set; } = true;
        public bool ShowMissingAchievementNames { get; set; } = true;
        public bool ShowMissingAchievementDetails { get; set; } = false;
        public bool Debug { get; set; } = false;
    }
}
