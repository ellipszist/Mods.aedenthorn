using StardewModdingAPI;

namespace BuildingShift
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int ShiftAmount { get; set; } = 32;
        public SButton Reset { get; set; } = SButton.NumPad5;
        public SButton ShiftDown { get; set; } = SButton.Down;
        public SButton ShiftUp { get; set; } = SButton.Up;
        public SButton ShiftRight { get; set; } = SButton.Right;
        public SButton ShiftLeft { get; set; } = SButton.Left;
        public SButton ModKey { get; set; } = SButton.LeftShift;
    }
}
