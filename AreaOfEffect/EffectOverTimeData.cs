using Microsoft.Xna.Framework;
using StardewValley;

namespace AreaOfEffect
{
    public class EffectOverTimeData
    {
        public SpellEffect Effect { get; set; }
        public GameLocation Location { get; set; }
        public Vector2 Tile { get; set; }
        public Farmer Who { get; set; }
        public int Milliseconds { get; set; }

    }
}