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
        public Object GetSprinklerAtMouse();
        public Object GetSprinklerAtTileCorner(GameLocation location, int x, int y);
        public bool IsSprinklerAtMouse();
        public bool IsSprinklerAtTileCorner(GameLocation location, Vector2 tile);
        public Object GetScarecrowAtMouse();
        public Object GetScarecrowAtTileCorner(GameLocation location, int x, int y);
        public bool IsScarecrowAtMouse();
        public bool IsScarecrowAtTileCorner(GameLocation location, Vector2 tile);
        public int GetSprinklerRadius(Object obj);
        public List<Vector2> GetSprinklerRange(Vector2 tile, int radius);
        public List<Vector2> GetScarecrowRange(Vector2 tile, int radius);
        public List<Vector2> GetSprinklerRange(GameLocation location, Vector2 tile);
        public List<Vector2> GetScarecrowRange(GameLocation location, Vector2 tile);

    }
    public class ImmersiveApi : IImmersiveApi
    {
        public Object GetSprinklerAtMouse()
        {
            return ModEntry.GetSprinklerAtMouse();
        }
        public Object GetSprinklerAtTileCorner(GameLocation l, int x, int y)
        {
            return ModEntry.GetSprinkler(l, x, y);
        }

        public bool IsSprinklerAtMouse()
        {
            var corner = ModEntry.GetMouseCornerTile();
            return ModEntry.HasData(Game1.currentLocation, ModEntry.sprinklerKey, corner.X, corner.Y);
        }
        public bool IsSprinklerAtTileCorner(GameLocation location, Vector2 tile)
        {
            return ModEntry.HasData(location, ModEntry.sprinklerKey, (int)tile.X, (int)tile.Y);
        }
        public Object GetScarecrowAtMouse()
        {
            return ModEntry.GetScarecrowAtMouse();
        }
        public Object GetScarecrowAtTileCorner(GameLocation l, int x, int y)
        {
            return ModEntry.GetScarecrow(l, x, y);
        }

        public bool IsScarecrowAtMouse()
        {
            var corner = ModEntry.GetMouseCornerTile();
            return ModEntry.HasData(Game1.currentLocation, ModEntry.scarecrowKey, corner.X, corner.Y);
        }
        public bool IsScarecrowAtTileCorner(GameLocation location, Vector2 tile)
        {
            return ModEntry.HasData(location, ModEntry.scarecrowKey, (int)tile.X, (int)tile.Y);
        }

        public int GetSprinklerRadius(Object obj)
        {
            return ModEntry.GetSprinklerRadius(obj);
        }

        public List<Vector2> GetSprinklerRange(Vector2 tile, int radius)
        {
            return ModEntry.GetSprinklerTiles(tile, radius);
        }

        public List<Vector2> GetSprinklerRange(GameLocation l, Vector2 tile)
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
                if(obj != null)
                {
                    tiles.AddRange(ModEntry.GetSprinklerTiles(p.ToVector2(), GetSprinklerRadius(obj)));
                }
            }
            return tiles.ToList();
        }

        public List<Vector2> GetScarecrowRange(Vector2 tile, int radius)
        {
            return ModEntry.GetScarecrowTiles(tile, radius);
        }

        public List<Vector2> GetScarecrowRange(GameLocation l, Vector2 tile)
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
                var obj = ModEntry.GetScarecrowCached(l, p.X, p.Y);
                if(obj != null)
                {
                    tiles.AddRange(ModEntry.GetScarecrowTiles(p.ToVector2(), obj.GetRadiusForScarecrow()));
                }
            }
            return tiles.ToList();
        }
    }
}