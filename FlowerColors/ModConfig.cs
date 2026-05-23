using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FlowerColors
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool CombineStacks { get; set; } = true;
        public bool Debug { get; set; } = true;
        public KeybindList CopyButton { get; set; } = new (new Keybind(SButton.LeftControl, SButton.C));
        public KeybindList PasteButton { get; set; } = new (new Keybind(SButton.LeftControl, SButton.V));
        public KeybindList PickButton { get; set; } = new (new Keybind(SButton.LeftControl, SButton.X));
        public SButton PrismaticModKey { get; set; } = SButton.LeftShift;
    }
}
