using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using xTile.Dimensions;

namespace ImmersiveSprinklersAndScarecrows
{
    public interface IImmersiveApi
    {
        public Object GetObjectAtMouse();
        public Object GetObjectAtTileCorner(GameLocation location, int x, int y);
        public bool IsObjectAtMouse();
        public bool IsObjectAtTileCorner(GameLocation location, Vector2 tile);
        public int GetRadius(Object obj);
        public List<Vector2> GetRange(Vector2 tile, int radius);
        public List<Vector2> GetRange(GameLocation location, Vector2 tile);

    }
    public class ImmersiveApi : IImmersiveApi
    {
        public Object GetObjectAtMouse()
        {
            return ModEntry.GetSprinklerAtMouse();
        }
        public Object GetObjectAtTileCorner(GameLocation l, int x, int y)
        {
            return ModEntry.GetSprinkler(l, x, y);
        }

        public int GetRadius(Object obj)
        {
            return ModEntry.GetSprinklerRadius(obj);
        }

        public List<Vector2> GetRange(Vector2 tile, int radius)
        {
            return ModEntry.GetSprinklerTiles(tile, radius);
        }

        public List<Vector2> GetRange(GameLocation l, Vector2 tile)
        {
            HashSet<Vector2> tiles = new HashSet<Vector2>();
            var x = (int) tile.X;
            var y = (int) tile.Y;
            List<Point> points = new List<Point>()
            {
                new Point(x, y),
                new Point(x - 1, y),
                new Point(x - 1, y - 1),
                new Point(x, y - 1)
            };
            foreach(var p in points)
            {
                var obj = ModEntry.GetSprinklerCached(l, p.X, p.Y);
                if(obj!= null)
                {
                    tiles.AddRange(ModEntry.GetSprinklerTiles(p.ToVector2(), GetRadius(obj)));
                }
            }
            return tiles.ToList();
        }

        public bool IsObjectAtMouse()
        {
            var corner = ModEntry.GetMouseCornerTile();
            return ModEntry.SprinklerAt(Game1.currentLocation, corner.X, corner.Y) != null;
        }
        public bool IsObjectAtTileCorner(GameLocation location, Vector2 tile)
        {
            return ModEntry.SprinklerAt(location, (int)tile.X, (int)tile.Y) != null;
        }
    }
}