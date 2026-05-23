using StardewModdingAPI;

namespace FlowerColorPicker
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool CombineStacks { get; set; } = true;
        public bool Debug { get; set; } = true;
        public SButton PrismaticModKey { get; set; } = SButton.LeftShift;
    }
}
