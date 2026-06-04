
using StardewModdingAPI;

namespace LawnGrass
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public float GrowChance { get; set; } = 0.5f;
        public SButton ModKey { get; set; } = SButton.LeftControl;
    }
}
