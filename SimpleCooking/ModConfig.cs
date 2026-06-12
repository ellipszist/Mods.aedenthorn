
using Microsoft.Xna.Framework;

namespace SimpleCooking
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public float GrilledEdibilityMult { get; set; } = 2;
        public float GrilledPriceMult { get; set; } = 2;
        public int VegetableCookTime { get; set; } = 10;
        public int FishCookTime { get; set; } = 30;
        public float VegetableBurn { get; set; } = 3;
        public float FishBurn { get; set; } = 2;
        public Color GrilledColor { get; set; } = Color.DarkGoldenrod;
        public bool Debug { get; set; } = false;
    }
}
