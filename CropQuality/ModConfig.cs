using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace CropQuality
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool ConstantQuality { get; set; } = true;
        public bool RandomQuality { get; set; } = true;
        public bool ResetConstantForRegrow { get; set; } = true;
        public float QualityModifier { get; set; } = 1f;
        public float GoldMaxChance { get; set; } = 0.75f;
        public float SilverMaxChance { get; set; } = 0.75f;
        public SButton ShowButton { get; set; } = SButton.None;
        public bool ToggleShow { get; set; } = false;
        public float Scale { get; set; } = 0.8f;
    }
}
