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
        public string FontPath { get; set; }
        public int DefaultColor {  get; set; }
        public float FontScale { get; set; } = 1f;
        public bool SpriteText { get; set; }
    }
}