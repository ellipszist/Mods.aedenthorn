using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using xTile;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {
        //[HarmonyPatch(typeof(Game1), nameof(Game1.loadForNewGame))]
        public static class Game1_loadForNewGame_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;

                openWorldLocation = new GameLocation(mapPath, locName) { IsOutdoors = true, IsFarm = true, IsGreenhouse = false };
                SMonitor.Log("Created new game location");

                Game1.locations.Add(openWorldLocation);
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.loadMap))]
        public static class GameLocation_loadMap_Patch
        {
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.ModEnabled || !__instance.Name.Contains(locName))
                    return;
                AccessTools.FieldRefAccess<Map, Size>(__instance.Map, "m_displaySize") = new Size(100000, 100000);
                AccessTools.FieldRefAccess<Map, string>(__instance.Map, "m_id") = locName;
            }
        }
        [HarmonyPatch(typeof(Map), "UpdateDisplaySize")]
        public static class Map_UpdateDisplaySize_Patch
        {
            public static bool Prefix(string ___m_id, ref Size ___m_displaySize)
            {
                if (!Config.ModEnabled || !___m_id.Contains(locName))
                    return true;
                ___m_displaySize = new Size(100000, 100000);
                return false;
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.isOutdoorMapSmallerThanViewport))]
        public static class Game1_isOutdoorMapSmallerThanViewport_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                return !Config.ModEnabled || Game1.currentLocation is null || !Game1.currentLocation.Name.Contains(locName);
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.UpdateViewPort))]
        public static class Game1_UpdateViewPort_Patch
        {
            public static void Prefix(ref bool overrideFreeze)
            {
                if (!Config.ModEnabled || !Game1.currentLocation.Name.Contains(locName))
                    return;
                overrideFreeze = true;
                Game1.forceSnapOnNextViewportUpdate = true;
                Game1.currentLocation.forceViewportPlayerFollow = true;
            }
        }
        [HarmonyPatch(typeof(Layer), nameof(Layer.Tiles))]
        [HarmonyPatch(MethodType.Getter)]
        public static class Layer_Tiles_Getter_Patch
        {            
            public static void Postfix(Layer __instance, TileArray __result)
            {
                if (!Config.ModEnabled || !__instance.Map.Id.Contains(locName))
                    return;
                __result = new MyTileArray(__instance, __result);
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isTileOnMap), new Type[] { typeof(Vector2) })]
        public static class GameLocation_isTileOnMap_Patch1
        {
            public static bool Prefix(GameLocation __instance, Vector2 position, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.Name.Contains(locName))
                    return true;
                __result = position.X >= 0f && position.X < openWorldSize && position.Y >= 0f && position.Y < openWorldSize;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isTileOnMap), new Type[] { typeof(int), typeof(int) })]
        public static class GameLocation_isTileOnMap_Patch2
        {
            public static bool Prefix(GameLocation __instance, int x, int y, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.Name.Contains(locName))
                    return true;
                __result = x >= 0f && x < openWorldSize && y >= 0f && y < openWorldSize;
                return false;
            }
        }
        [HarmonyPatch(typeof(Layer), nameof(Layer.Draw))]
        public static class Layer_Draw_Patch
        {
            public static bool Prefix(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, int pixelZoom, float sort_offset)
            {
                if (!Config.ModEnabled || !Game1.currentLocation.Name.Contains(locName))
                    return true;
                Layer.zoom = pixelZoom;

                int tileWidth = pixelZoom * 16;
                int tileHeight = pixelZoom * 16;
                Location tileInternalOffset = new Location(Wrap(mapViewport.X, tileWidth), Wrap(mapViewport.Y, tileHeight));
                Location tileLocation = displayOffset - tileInternalOffset;

                Point loc = Game1.player.TilePoint;
                Tile[,] tiles = new Tile[openWorldChunkSize, openWorldChunkSize];
                Point playerChunk = new(loc.X / openWorldChunkSize, loc.Y / openWorldChunkSize);
                Rectangle playerBox = new(loc.X - openWorldChunkSize / 2, loc.Y - openWorldChunkSize / 2, openWorldChunkSize, openWorldChunkSize);
                List<Tile[,]> surroundingChunkTiles = new();


                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        var cx = playerChunk.X + x;
                        var cy = playerChunk.Y + y;
                        Tile[,] chunkTiles = null;
                        if (cx >= 0 && cy >= 0)
                        {
                            chunkTiles = GetChunkTiles(__instance.Id, cx, cy);
                        }
                        surroundingChunkTiles.Add(chunkTiles);
                    }
                }
                for (int ry = 0; ry < openWorldChunkSize; ry++)
                {
                    tileLocation.X = displayOffset.X - tileInternalOffset.X;
                    if (tileInternalOffset.X < 32)
                    {
                        tileLocation.X -= tileWidth;
                    }
                    for (int rx = 0; rx < openWorldChunkSize; rx++)
                    {
                        int ax = playerBox.X + rx;
                        int ay = playerBox.Y + ry;

                        int idx = 0;
                        for (int y = -1; y < 2; y++)
                        {
                            for (int x = -1; x < 2; x++)
                            {
                                var cx = playerChunk.X + x;
                                var cy = playerChunk.Y + y;
                                if (surroundingChunkTiles[idx] is not null)
                                {
                                    Rectangle chunkBox = new(openWorldChunkSize * cx, openWorldChunkSize * cy, openWorldChunkSize, openWorldChunkSize);
                                    if (chunkBox.Contains(new Point(ax, ay)))
                                    {
                                        var tx = ax - openWorldChunkSize * cx;
                                        var ty = ay - openWorldChunkSize * cy;
                                        Tile tile = surroundingChunkTiles[idx][tx, ty];
                                        if(tile is not null)
                                        {
                                            float drawn_sort = 0f;
                                            if (sort_offset >= 0f)
                                            {
                                                drawn_sort = ((float)(ry * (16 * pixelZoom) + 16 * pixelZoom) + sort_offset) / 10000f;
                                            }
                                            if (!__instance.Id.Equals("Front"))
                                            {
                                                displayDevice.DrawTile(tile, tileLocation, drawn_sort);
                                            }
                                        }
                                        goto next;
                                    }
                                }
                                idx++;
                            }
                        }
                    next:
                        tileLocation.X += tileWidth;
                    }
                    tileLocation.Y += tileHeight;
                }
                return false;
            }


            private static int Wrap(int value, int span)
            {
                value %= span;
                return value;
            }
        }
    }
}