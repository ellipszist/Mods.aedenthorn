using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
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

    }
}