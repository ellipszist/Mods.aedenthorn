using StardewValley;
using System.Collections.Generic;

namespace LetterBlocks
{
    public class BlockData
    {
        public string[] Letters { get; set; } = new string[]
            {
                "abcdefghijklmnopqrstuvwxyz",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                "1234567890"
            };
        public string FontPath { get; set; } = "Fonts\\SpriteFont1";
        public int DefaultColor {  get; set; }
        public float FontScale { get; set; } = 1f;
    }
}