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
        public static bool IsOpenTile(WorldChunk chunk, Vector2 t)
        {
            if (!IsVectorInMap(t))
                return false;
            Tile? tile = chunk.tiles["Back"][(int)t.X % openWorldChunkSize, (int)t.Y % openWorldChunkSize];
            return tile is not null && grassTiles.Contains(tile.TileIndex) && !openWorldLocation.terrainFeatures.ContainsKey(t) && !openWorldLocation.Objects.ContainsKey(t) && !openWorldLocation.overlayObjects.ContainsKey(t);
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

    }
}