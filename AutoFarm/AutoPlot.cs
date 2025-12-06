using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AutoFarm
{
    public class AutoPlot
    {
        public HashSet<Vector2> tiles = new HashSet<Vector2>();
        public bool harvest;
        public bool till;
        public bool removeDead;
        public bool buySeed;
        public bool buyFertilizer;
        public string[] fertilizer = new string[4];
        public string[] seed = new string[4];
        public Vector2 GetKeyTile()
        {
            Vector2 tile = new Vector2(float.MaxValue, float.MaxValue);
            foreach(Vector2 t in tiles)
            {
                if (t.X < tile.X || (t.X == tile.X && t.Y < tile.Y))
                    tile = t;
            }
            return tile;
        }
    }
}