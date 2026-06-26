
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;

namespace Tutorials
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public KeybindList OpenTutorialKey { get; set; } = new(new Keybind( StardewModdingAPI.SButton.LeftControl, StardewModdingAPI.SButton.F1));
        public bool Debug { get; set; } = false;
    }
}
