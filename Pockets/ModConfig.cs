using System.Collections.Generic;

namespace Pockets
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public float Alpha { get; set; } = 0.25f;
        public Dictionary<string, PocketData> DefaultPockets { get; set; } = new()
        {
            {
                "DefaultShirt",
                new PocketData()
                {
                    ClothesType = StardewValley.Objects.Clothing.ClothesType.SHIRT,
                    StartX = 0,
                    StartY = 24,
                    Width = 64,
                    Height = 24,
                    PocketRows = 3,
                    PocketSlots = 9
                }
            },
            {
                "DefaultRightPants",
                new PocketData()
                {
                    ClothesType = StardewValley.Objects.Clothing.ClothesType.PANTS,
                    StartX = 0,
                    StartY = 48,
                    Width = 32,
                    Height = 24,
                    PocketRows = 3,
                    PocketSlots = 9
                }
            },
            {
                "DefaultLeftPants",
                new PocketData()
                {
                    ClothesType = StardewValley.Objects.Clothing.ClothesType.PANTS,
                    StartX = 32,
                    StartY = 48,
                    Width = 32,
                    Height = 24,
                    PocketRows = 3,
                    PocketSlots = 9
                }
            }
        };
    }
}
