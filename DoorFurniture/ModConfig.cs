using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace DoorFurniture
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = true;
        public bool AutoOpen { get; set; } = true;
        public int AutoCloseDelay { get; set; } = 10;
        public int PreventCloseBuffer { get; set; } = 64;
        public SButton FlipButton { get; set; } = SButton.F;
    }
}
