using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AutoFarm
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public KeybindList MenuKey { get; set; } = new KeybindList(SButton.N);
        public SButton CreateKey { get; set; } = SButton.MouseLeft;
    }
}
