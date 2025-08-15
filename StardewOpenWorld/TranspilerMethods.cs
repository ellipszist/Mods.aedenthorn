using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {

        private static void DontRemoveMonster(NetCollection<NPC> npcs, Monster monster, GameLocation location)
        {
            if (!Config.ModEnabled || location != openWorldLocation || monster.Position.X < 0f || monster.Position.X > Config.OpenWorldSize * 64 || monster.Position.Y < 0f || monster.Position.Y > Config.OpenWorldSize * 64)
                npcs.Remove(monster);
        }
        public static int GetGlobalCharacterInt(int value, Character character)
        {
            if (!Config.ModEnabled || character.currentLocation?.Name.Contains(locName) != true)
                return value;
            int v = value % (openWorldChunkSize * 64) + ChunkDisplayOffset(value);
            return v;
        }
        public static float GetGlobalCharacterFloat(float value, Character character)
        {
            if (!Config.ModEnabled || character.currentLocation?.Name.Contains(locName) != true)
                return value;
            float v = value % (openWorldChunkSize * 64) + ChunkDisplayOffset(value);
            return v;
        }
        public static int GetGlobalTreeInt(int value, Tree tree)
        {
            if (!Config.ModEnabled || tree.Location?.Name.Contains(locName) != true)
                return value;
            int v = value % (openWorldChunkSize * 64) + ChunkDisplayOffset(value);
            return v;
        }


        public static float GetGlobalTreeFloat(float value, Tree tree)
        {
            if (!Config.ModEnabled || tree.Location?.Name.Contains(locName) != true)
                return value;
            float v = value % openWorldChunkSize + ChunkDisplayOffset(value);
            return v;
        }


        public static float GetChestDrawLayer(float value, Chest chest, int x, int y)
        {
            if (!Config.ModEnabled || chest.Location?.Name.Contains(locName) != true)
                return value;
            float draw_x = (float)x;
            float draw_y = (float)y;
            if (chest.localKickStartTile != null)
            {
                draw_x = Utility.Lerp(chest.localKickStartTile.Value.X, draw_x, chest.kickProgress);
                draw_y = Utility.Lerp(chest.localKickStartTile.Value.Y, draw_y, chest.kickProgress);
            }
            return Math.Max(0f, ((draw_y % openWorldChunkSize + 1f) * 64f + ChunkDisplayOffset(value) - 24f) / 10000f) + draw_x % openWorldChunkSize * 1E-05f;
        }
        public static float GetObjectDrawLayer(float value, Object obj, int x, int y)
        {
            if (!Config.ModEnabled || obj.Location?.Name.Contains(locName) != true)
                return value;
            return Math.Max(0f, (float)((y % openWorldChunkSize + 1) * 64 - 24) / 10000f) + (float)x % openWorldChunkSize * 1E-05f;
        }
        public static float GetObjectDrawLayer2(float value, Object obj, int x, int y)
        {
            if (!Config.ModEnabled || !obj.Location.Name.Contains(locName))
                return value;
            return Math.Max(0f, (float)((y % openWorldChunkSize + 1) * 64 + ChunkDisplayOffset(y) + 2) / 10000f) + (float)x % openWorldChunkSize / 1000000f;
        }
        public static float GetObjectDrawLayer3(float value, Object obj, int x, int y)
        {
            if (!Config.ModEnabled || !obj.Location.Name.Contains(locName))
                return value;
            return (float)((y % openWorldChunkSize + ChunkDisplayOffset(y) + 1) * 64) / 10000f + (obj.TileLocation.X % openWorldChunkSize) / 50000f; ;
        }
        public static Rectangle GetObjectBoundingBox(Rectangle value, Object obj, int x, int y)
        {
            if (!Config.ModEnabled || obj.Location?.Name.Contains(locName) != true)
                return value;
            value.Location = new(value.Location.X % openWorldChunkSize, value.Location.Y % openWorldChunkSize + ChunkDisplayOffset(y));
            return value;
        }

        public void ResetChunkTiles()
        {
            int size = Config.OpenWorldSize / openWorldChunkSize;
            cachedChunks = new();
        }

        public static Tile[,] GetChunkTiles(string layer, int cx, int cy)
        {
            if (cachedChunks.TryGetValue(new Point(cx, cy), out var chunk) && chunk.tiles.TryGetValue(layer, out var tiles))
            {
                return tiles;
            }
            return null;
        }
    }
}