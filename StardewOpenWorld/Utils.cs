using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using xTile.Layers;
using xTile.Tiles;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {

        private static bool IsChunkInMap(int cx, int cy)
        {
            return (cx >= 0 && cy >= 0 && cx < Config.OpenWorldSize / openWorldChunkSize && cy < Config.OpenWorldSize / openWorldChunkSize);
        }
        private static bool IsChunkInMap(Point cp)
        {
            return (cp.X >= 0 && cp.Y >= 0 && cp.X < Config.OpenWorldSize / openWorldChunkSize && cp.Y < Config.OpenWorldSize / openWorldChunkSize);
        }
        private static bool IsVectorInMap(Vector2 v)
        {
            return (v.X >= 0 && v.Y >= 0 && v.X < Config.OpenWorldSize && v.Y < Config.OpenWorldSize);
        }
        private static int ChunkDisplayOffset(int value)
        {
            return (Game1.viewport.Y / (openWorldChunkSize * 64) < value / (openWorldChunkSize * 64) ? openWorldChunkSize * 64 : 0);
        }
        private static float ChunkDisplayOffset(float value)
        {
            return (Game1.viewport.Y / (openWorldChunkSize * 64) < (int)value / (openWorldChunkSize * 64) ? openWorldChunkSize * 64 : 0);
        }

        public static bool CodesCompare(List<CodeInstruction> codes, int i, OpCode[] opCodes)
        {
            for (int j = 0; j < opCodes.Length; j++)
            {
                if (codes.Count <= i + j)
                    return false;
                if (codes[i + j].opcode != opCodes[j])
                    return false;
            }
            return true;
        }
        public static bool IsOpenTile(Vector2 av)
        {
            if (!IsVectorInMap(av))
                return false;
            var cp = new Point((int)av.X / openWorldChunkSize, (int)av.Y / openWorldChunkSize);
            if (!cachedChunks.TryGetValue(cp, out var chunk))
            {
                chunk = CacheChunk(cp);
            }
            Tile? tile = chunk.tiles["Back"][(int)av.X % openWorldChunkSize, (int)av.Y % openWorldChunkSize];
            return tile is not null && grassTiles.Contains(tile.TileIndex) && !openWorldLocation.terrainFeatures.ContainsKey(av) && !openWorldLocation.Objects.ContainsKey(av) && !openWorldLocation.overlayObjects.ContainsKey(av);
        }

        private static Point GetPlayerChunk(Farmer f)
        {
            return new Point(f.TilePoint.X / openWorldChunkSize, f.TilePoint.Y / openWorldChunkSize);
        }
        public static Tile GetTile(Layer layer, int x, int y)
        {
            return null;
        }

        public static void SetTile(Layer layer, int x, int y, Tile value)
        {
        }
        public static Point[] GetSurroundingTileLocationsArray(Point tileLocation, bool include)
        {
            if (include)
            {
                return new Point[]
                {
                    tileLocation,
                    new Point(-1, 0) + tileLocation,
                    new Point(1, 0) + tileLocation,
                    new Point(0, 1) + tileLocation,
                    new Point(0, -1) + tileLocation,
                    new Point(-1, -1) + tileLocation,
                    new Point(1, -1) + tileLocation,
                    new Point(1, 1) + tileLocation,
                    new Point(-1, 1) + tileLocation
                };
            }
            return new Point[]
            {
                new Point(-1, 0) + tileLocation,
                new Point(1, 0) + tileLocation,
                new Point(0, 1) + tileLocation,
                new Point(0, -1) + tileLocation,
                new Point(-1, -1) + tileLocation,
                new Point(1, -1) + tileLocation,
                new Point(1, 1) + tileLocation,
                new Point(-1, 1) + tileLocation
            };
        }
        public static Point[] GetSurroundingPointArray(bool include)
        {
            if (include)
            {
                return new Point[]
                {
                    new Point(0, 0),
                    new Point(-1, 0),
                    new Point(1, 0),
                    new Point(0, 1),
                    new Point(0, -1),
                    new Point(-1, -1),
                    new Point(1, -1),
                    new Point(1, 1),
                    new Point(-1, 1)
                };
            }
            return new Point[]
            {
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, 1),
                new Point(0, -1),
                new Point(-1, -1),
                new Point(1, -1),
                new Point(1, 1),
                new Point(-1, 1)
            };
        }
        public static Point GetAbsolutePosition(Point cp, int x, int y)
        {
            return new Point(cp.X * openWorldChunkSize + x, cp.Y * openWorldChunkSize + y);
        }

        private static List<Point> GetBlob(Point c, Random r, int maxTiles)
        {

            List<Point> tiles = new List<Point>();
            int maxSize = (int)Math.Round(Math.Sqrt(maxTiles));
            int maxVariation = (int)Math.Round(maxSize / 2f);
            int height = maxSize - r.Next(maxVariation);
            int width = maxSize - r.Next(maxVariation);
            double ratio = (double)height / (double)width;
            int leftTop = (maxSize - height) / 2;
            int leftBot = maxSize - leftTop;
            int rightTop = (maxSize - height) / 2;
            int rightBot = maxSize - rightTop;
            for (int x = width / 2; x >= 0; x--)
            {

                double chance = Math.Pow((width / 2 - x) / (double)width, 2) * 4;
                while (leftTop < height / 2)
                {
                    if (r.NextDouble() < chance)
                    {
                        leftTop++;
                    }
                    else
                    {
                        break;
                    }
                }
                while (leftBot > height / 2)
                {
                    if (r.NextDouble() < chance)
                    {
                        leftBot--;
                    }
                    else
                    {
                        break;
                    }
                }
                for (int y = leftTop; y < leftBot; y++)
                {
                    tiles.Add(c + new Point(x, y));
                }
            }
            for (int x = width / 2 + 1; x < width; x++)
            {
                double chance = Math.Pow((x - width / 2) / (double)width, 2) * 4;
                while (rightTop < height / 2)
                {
                    if (r.NextDouble() < chance)
                    {
                        rightTop++;
                    }
                    else
                    {
                        break;
                    }
                }
                while (rightBot > height / 2)
                {
                    if (r.NextDouble() < chance)
                    {
                        rightBot--;
                    }
                    else
                    {
                        break;
                    }
                }
                if (rightBot <= rightTop)
                {
                    break;
                }
                for (int y = rightTop; y < rightBot; y++)
                {
                    tiles.Add(c + new Point(x, y));
                }

            }
            return tiles;


            /*
            Point begin = new(0, 0);
            Point end = new(0, 0);
            int idx = 0;
            List<Point> tiles = new List<Point>();
            while (idx < maxTiles)
            {
                foreach (var rect in landmarkRects)
                {
                    var bounds = new Rectangle(c + begin - new Point(1, 1), (c + end + new Point(1, 1)) - (c + begin - new Point(1, 1)));
                    if (rect.Intersects(bounds))
                        return null;
                }
                for (int x = begin.X; x <= end.X; x++)
                {
                    for (int y = begin.Y; y <= end.Y; y++)
                    {
                        Point v = c + new Point(x, y);
                        if (tiles.Contains(v))
                            continue;
                        if (x != begin.X && x != end.X && y != begin.Y && y != end.Y)
                            continue;
                        if (v == c)
                        {
                            tiles.Add(v);
                            idx++;
                            if (idx >= maxTiles)
                                goto cont;
                            goto next;
                        }

                        int surround = 0;
                        foreach (var s in Utility.getSurroundingTileLocationsArray(v.ToVector2()))
                        {
                            if (tiles.Contains(s.ToPoint()))
                            {
                                surround++;
                                break;
                            }
                        }
                        if (surround == 0)
                        {
                            //continue;
                        }
                        //var distance = ((v.X - c.X) + (v.Y - c.Y)) / 2f;
                        var distance = Vector2.Distance(v.ToVector2(), c.ToVector2());

                        double chance = 1 - distance / (maxTiles / 48);
                        if (r.NextDouble() < chance)
                        {
                            tiles.Add(v);
                            idx++;
                            if (idx >= maxTiles)
                                goto cont;
                        }
                    }
                }
                begin -= new Point(1, 1);
                end += new Point(1, 1);
                if (end.X > openWorldChunkSize / 2)
                    break;
                next:
                continue;
            }
        cont:
            // fill holes
            for (int x = begin.X; x <= end.X; x++)
            {
                for (int y = begin.Y; y <= end.Y; y++)
                {
                    Point v = c + new Point(x, y);
                    if (tiles.Contains(v))
                    {
                        bool fill = false;
                        for (int y2 = end.Y; y2 > y; y2--)
                        {
                            Point v2 = c + new Point(x, y2);
                            if (tiles.Contains(v2))
                            {
                                fill = true;
                            }
                            else if (fill)
                            {
                                tiles.Add(v2);
                            }
                        }
                        break;
                    }
                }
            }
            for (int y = begin.Y; y <= end.Y; y++)
            {
                for (int x = begin.X; x <= end.X; x++)
                {
                    Point v = c + new Point(x, y);
                    if (tiles.Contains(v))
                    {
                        bool fill = false;
                        for (int x2 = end.X; x2 > x; x2--)
                        {
                            Point v2 = c + new Point(x2, y);
                            if (tiles.Contains(v2))
                            {
                                fill = true;
                            }
                            else if (fill)
                            {
                                tiles.Add(v2);
                            }
                        }
                        break;
                    }
                }
            }
            */
        }
        private static List<Point> GetBlobPadding(List<Point> tiles, int amount, bool include)
        {
            List<Point> temp = new(tiles);
            List<Point> newTiles = new();
            for(int i = 0; i < amount; i++)
            {
                foreach (var t in temp)
                {
                    for(int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            var np = t + new Point(x, y);
                            if (!temp.Contains(np) && !newTiles.Contains(np))
                            {
                                newTiles.Add(np);
                            }
                        }
                    }
                    temp = new(tiles);
                    temp.AddRange(newTiles);
                }
            }
            if (include)
                newTiles.AddRange(tiles);
            return newTiles;
        }
    }
}