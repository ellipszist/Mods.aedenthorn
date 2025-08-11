using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using xTile.Tiles;

namespace StardewOpenWorld
{
    public class WorldChunk
    {
        public Dictionary<Point, Object> objects = new();
        public Dictionary<Point,TerrainFeature> terrainFeatures = new();
        public Dictionary<string, Tile[,]> tiles = new();
        public int priority;
    }
}