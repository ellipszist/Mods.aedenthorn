using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace LauncherDrawer
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public KeybindList DrawerKey { get; set; } = new(SButton.NumPad5);
    }
}
