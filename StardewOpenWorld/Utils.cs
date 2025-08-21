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

            if (chunk.tiles["Buildings"][(int)av.X % openWorldChunkSize, (int)av.Y % openWorldChunkSize] != null)
                return false;

            var back = chunk.tiles["Back"][(int)av.X % openWorldChunkSize, (int)av.Y % openWorldChunkSize];
            if (back != null && back.Properties.ContainsKey("Water"))
                return false;

            return !openWorldLocation.terrainFeatures.ContainsKey(av) && !openWorldLocation.Objects.ContainsKey(av) && !openWorldLocation.overlayObjects.ContainsKey(av);
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
        public static Point GetLocalPosition(Point cp, int x, int y, out Point ocp)
        {
            var point = new Point(x - cp.X * openWorldChunkSize,  y - cp.Y * openWorldChunkSize);
            var offset = GetPointOffset(point);
            ocp = cp + offset;
            point -= new Point(offset.X * openWorldChunkSize, offset.Y * openWorldChunkSize);
            return point;
        }
        public static Point GetLocalPosition(Point p)
        {
            return new Point(p.X % openWorldChunkSize,  p.Y % openWorldChunkSize);
        }

        private static Point GetPointOffset(Point p)
        {

            Point offset = new();
            if (p.X < 0)
            {
                offset += new Point(-1, 0);
            }
            else if (p.X >= openWorldChunkSize)
            {
                offset += new Point(1, 0);
            }
            if (p.Y < 0)
            {
                offset += new Point(0, -1);
            }
            else if (p.Y >= openWorldChunkSize)
            {
                offset += new Point(0, 1);
            }
            return offset;
        }
        private static List<Point> GetBlob(Point c, Random r, int maxTiles)
        {

            int maxSize = (int)Math.Round(Math.Sqrt(maxTiles));
            int maxVariation = (int)Math.Round(maxSize / 2f);
            int height = maxSize - r.Next(maxVariation);
            int width = maxSize - r.Next(maxVariation);

            List<Point> tiles = MakeBlob(c, r, height, width, maxSize);
            return tiles;

        }

        private static List<Point> MakeBlob(Point c, Random r, int height, int width, int maxSize)
        {
            List<Point> tiles = new List<Point>();

            int leftTop = (maxSize - height) / 2;
            int rightTop = leftTop;
            int leftBot = maxSize - leftTop;
            int rightBot = leftBot;
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
                    tiles.Add(c + new Point(x - width / 2, y - height / 2));
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
                    tiles.Add(c + new Point(x - width / 2, y - height / 2));
                }

            }
            return tiles;
        }

        private static List<Point> MakeBlobFromTiles(Random rand, List<Point> tiles)
        {
            int l = int.MaxValue;
            int t = int.MaxValue;
            int r = -1;
            int b = -1;
            foreach(var tile in tiles)
            {
                if(tile.X < l)
                {
                    l = tile.X;
                }
                if(tile.X > r)
                {
                    r = tile.X;
                }
                if (tile.Y < t)
                {
                    t = tile.Y;
                }
                if (tile.Y > b)
                {
                    b = tile.Y;
                }
            }
            var c = new Point(l + (r - l) / 2, t + (b - t) / 2);
            return MakeBlob(c, rand, b - t + 2, r - l + 2, r - l + 2);
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