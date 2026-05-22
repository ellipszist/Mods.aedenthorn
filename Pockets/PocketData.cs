using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.Objects;

namespace Pockets
{
    public class PocketData
    {
        public Clothing.ClothesType ClothesType { get; set; }
        public SButton HotKey { get; set; } = SButton.None;
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int PocketSlots { get; set; }
        public int PocketRows { get; set; }
    }
}