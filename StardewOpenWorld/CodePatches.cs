using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;
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
        [HarmonyPatch(typeof(Monster), nameof(Monster.update))]
        public static class Monster_update_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Monster.update");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(NetCollection<NPC>), "Remove", new Type[] { typeof(NPC) }))
                    {
                        SMonitor.Log("Preventing monster removal for out of bounds");

                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DontRemoveMonster));
                        codes.RemoveAt(i + 1);
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_2));
                        break;
                    }
                }
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.draw), new Type[] { typeof(SpriteBatch), typeof(float) })]
        public static class Character_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Character.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetGlobalCharacterInt))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GreenSlime), nameof(GreenSlime.draw), new Type[] { typeof(SpriteBatch) })]
        public static class GreenSlime_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GreenSlime.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)) && codes[i - 1].opcode == OpCodes.Call && codes[i - 1].operand is MethodInfo && (MethodInfo)codes[i - 1].operand == AccessTools.PropertyGetter(typeof(Character), nameof(Character.StandingPixel)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetGlobalCharacterInt))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Tree), nameof(Tree.draw))]
        public static class Tree_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Tree.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.PropertyGetter(typeof(Rectangle), nameof(Rectangle.Bottom)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetGlobalTreeInt))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                    else if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.X)) && codes[i + 1].opcode == OpCodes.Ldc_R4 && codes[i + 2].opcode == OpCodes.Div)
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetGlobalTileFloat))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Object_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Object.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (CodesCompare(codes, i, new OpCode[] { OpCodes.Ldc_R4,OpCodes.Ldarg_3,OpCodes.Ldc_I4_1,OpCodes.Add,OpCodes.Ldc_I4_S,OpCodes.Mul,OpCodes.Ldc_I4_S, OpCodes.Sub,OpCodes.Conv_R4,OpCodes.Ldc_R4,OpCodes.Div,OpCodes.Call,OpCodes.Ldarg_2,OpCodes.Conv_R4,OpCodes.Ldc_R4,OpCodes.Mul,OpCodes.Add,OpCodes.Stloc_S })){
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetObjectDrawLayer))));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_3));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 21;                    
                    }
                    if (CodesCompare(codes, i, new OpCode[] { OpCodes.Ldc_R4,OpCodes.Ldarg_3,OpCodes.Ldc_I4_1,OpCodes.Add,OpCodes.Ldc_I4_S,OpCodes.Mul,OpCodes.Ldc_I4_2, OpCodes.Add,OpCodes.Conv_R4,OpCodes.Ldc_R4,OpCodes.Div,OpCodes.Call,OpCodes.Ldarg_2,OpCodes.Conv_R4,OpCodes.Ldc_R4,OpCodes.Div,OpCodes.Add,OpCodes.Stloc_S })){
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetObjectDrawLayer2))));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_3));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_0));
                        break;                   
                    }
                }
                return codes.AsEnumerable();
            }
        }
        
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.getDrawLayer))]
        public static class Farmer_getDrawLayer_Patch
        {
            public static bool Prefix(Farmer __instance, ref float __result)
            {
                if (!Config.ModEnabled || __instance.currentLocation != openWorldLocation)
                    return true;
                int rsp = __instance.StandingPixel.Y % (openWorldChunkSize * 64) + (Game1.viewport.Y / (openWorldChunkSize * 64) < __instance.StandingPixel.Y / (openWorldChunkSize * 64) ? openWorldChunkSize * 64 : 0);
                if (__instance.onBridge.Value)
                {
                    __result = rsp / 10000f + __instance.drawLayerDisambiguator + 0.0256f;
                }
                else if (__instance.IsSitting() && __instance.mapChairSitPosition.Value.X != -1f && __instance.mapChairSitPosition.Value.Y != -1f)
                {
                    Vector2 sit_position = __instance.mapChairSitPosition.Value;
                    __result = ((sit_position.Y % (openWorldChunkSize * 128) + 1f) * 64f + 3141) / 10000f;
                }
                else
                    __result = rsp / 10000f + __instance.drawLayerDisambiguator;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.hasTileAt), new Type[] { typeof(int),typeof(int),typeof(string),typeof(string) })]
        public static class GameLocation_hasTileAt_Patch1
        {
            public static bool Prefix(GameLocation __instance, int x, int y, string layer, string tilesheetId, ref bool __result)
            {
                if (!Config.ModEnabled || layer != "Back" || !__instance.Name.Contains(locName) || x < 0 || y < 0 || x >= openWorldSize || y >= openWorldSize)
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.hasTileAt), new Type[] { typeof(Location),typeof(string),typeof(string) })]
        public static class GameLocation_hasTileAt_Patch2
        {
            public static bool Prefix(GameLocation __instance, Location tile, string layer, string tilesheetId, ref bool __result)
            {
                if (!Config.ModEnabled || layer != "Back" || !__instance.Name.Contains(locName) || tile.X < 0 || tile.Y < 0 || tile.X >= openWorldSize || tile.Y >= openWorldSize)
                    return true;
                __result = true;
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
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        public static class GameLocation_isCollidingPosition_Patch
        {
            public static bool Prefix(GameLocation __instance, Rectangle position, Rectangle viewport, bool isFarmer, bool projectile, Character character, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.Name.Contains(locName))
                    return true;
                BoundingBoxGroup passableTiles = null;
                Farmer farmer = character as Farmer;
                Rectangle? currentBounds;
                if (farmer != null)
                {
                    isFarmer = true;
                    var bb = farmer.GetBoundingBox();
                    currentBounds = new Rectangle?(bb);
                    
                    passableTiles = farmer.TemporaryPassableTiles;
                }
                else
                {
                    farmer = null;
                    isFarmer = false;
                    currentBounds = null;
                }
                Vector2? currentTopRight = null;
                Vector2? currentTopLeft = null;
                Vector2? currentBottomRight = null;
                Vector2? currentBottomLeft = null;
                Vector2? currentBottomMid = null;
                Vector2? currentTopMid = null;
                if (currentBounds != null)
                {
                    currentTopRight = new Vector2?(new Vector2((float)((currentBounds.Value.Right - 1) / 64), (float)(currentBounds.Value.Top / 64)));
                    currentTopLeft = new Vector2?(new Vector2((float)(currentBounds.Value.Left / 64), (float)(currentBounds.Value.Top / 64)));
                    currentBottomRight = new Vector2?(new Vector2((float)((currentBounds.Value.Right - 1) / 64), (float)((currentBounds.Value.Bottom - 1) / 64)));
                    currentBottomLeft = new Vector2?(new Vector2((float)(currentBounds.Value.Left / 64), (float)((currentBounds.Value.Bottom - 1) / 64)));
                    currentBottomMid = new Vector2?(new Vector2((float)(currentBounds.Value.Center.X / 64), (float)((currentBounds.Value.Bottom - 1) / 64)));
                    currentTopMid = new Vector2?(new Vector2((float)(currentBounds.Value.Center.X / 64), (float)(currentBounds.Value.Top / 64)));
                }

                Vector2 nextTopRight = new Vector2((float)(position.Right / 64), (float)(position.Top / 64));
                Vector2 nextTopLeft = new Vector2((float)(position.Left / 64), (float)(position.Top / 64));
                Vector2 nextBottomRight = new Vector2((float)(position.Right / 64), (float)(position.Bottom / 64));
                Vector2 nextBottomLeft = new Vector2((float)(position.Left / 64), (float)(position.Bottom / 64));
                bool nextLargerThanTile = position.Width > 64;
                Vector2 nextBottomMid = new Vector2((float)(position.Center.X / 64), (float)(position.Bottom / 64));
                Vector2 nextTopMid = new Vector2((float)(position.Center.X / 64), (float)(position.Top / 64));

                Tile tmp;


                if ((bool)AccessTools.Method(typeof(GameLocation), "_TestCornersTiles").Invoke(__instance, new object[]{nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate (Vector2 tile)
                {
                    if (__instance.terrainFeatures.TryGetValue(tile, out var tf))
                    {
                        var asdf = tf.getBoundingBox();
                        var xxx= asdf.Intersects(position);
                        var yyy = tf.isPassable(character);
                        if(xxx && !yyy)
                            return true;
                    }
                    if (__instance.Objects.TryGetValue(tile, out var obj))
                    {
                        var asdf = obj.GetBoundingBox();
                        var xxx= asdf.Intersects(position);
                        var yyy = obj.isPassable();
                        if(xxx && !yyy)
                            return true;
                    }
                    int cx = (int)tile.X / openWorldChunkSize;
                    int cy = (int)tile.Y / openWorldChunkSize;
                    int tx = (int)tile.X % openWorldChunkSize;
                    int ty = (int)tile.Y % openWorldChunkSize;
                    var backTiles = GetChunkTiles("Back", cx, cy);
                    var buildingTiles = GetChunkTiles("Buildings", cx, cy);
                    if(buildingTiles is null)
                        return false;
                    tmp = buildingTiles[tx,ty];
                    if (tmp != null)
                    {
                        if (projectile && __instance is VolcanoDungeon)
                        {
                            Tile back_tile = backTiles[tx, ty];
                            if (back_tile != null)
                            {
                                if (back_tile.TileIndexProperties.ContainsKey("Water"))
                                {
                                    return false;
                                }
                                if (back_tile.Properties.ContainsKey("Water"))
                                {
                                    return false;
                                }
                            }
                        }
                        bool flag3;
                        if (!tmp.TileIndexProperties.ContainsKey("Shadow") && !tmp.TileIndexProperties.ContainsKey("Passable") && !tmp.Properties.ContainsKey("Passable") && (!projectile || (!tmp.TileIndexProperties.ContainsKey("ProjectilePassable") && !tmp.Properties.ContainsKey("ProjectilePassable"))))
                        {
                            if (!isFarmer)
                            {
                                if (!tmp.TileIndexProperties.ContainsKey("NPCPassable") && !tmp.Properties.ContainsKey("NPCPassable"))
                                {
                                    Character character3 = character;
                                    bool? flag2 = ((character3 != null) ? new bool?(character3.canPassThroughActionTiles()) : null);
                                    flag3 = flag2 != null && flag2.GetValueOrDefault() && tmp.Properties.ContainsKey("Action");
                                }
                                else
                                {
                                    flag3 = true;
                                }
                            }
                            else
                            {
                                flag3 = false;
                            }
                        }
                        else
                        {
                            flag3 = true;
                        }
                        if (!flag3)
                        {
                            return passableTiles == null || !passableTiles.Contains((int)tile.X, (int)tile.Y);
                        }
                    }
                    return false;
                } }))
                {
                    __result = true;
                    return false;
                }
                return true;
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

        [HarmonyPatch(typeof(FarmerRenderer), nameof(FarmerRenderer.drawHairAndAccesories))]
        public static class FarmerRenderer_drawHairAndAccesories_Patch2
        {
            public static void Prefix(FarmerRenderer __instance, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, ref float layerDepth)
            {
                if (!Config.ModEnabled)
                    return;
            }
            public static void Postfix(FarmerRenderer __instance, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
            {
                if (!Config.ModEnabled)
                    return;
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
                displayOffset -= new Location(Game1.viewport.Width - 11 * 64, Game1.viewport.Height + 18 * 64 - 32);
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
                                            if (__instance.Id.Equals("Front"))
                                            {
                                            }
                                            float drawn_sort = 0f;
                                            if (sort_offset >= 0f)
                                            {
                                                drawn_sort = (ry * (16 * pixelZoom) + 16 * pixelZoom + sort_offset) / 10000f;
                                            }
                                            displayDevice.DrawTile(tile, tileLocation, drawn_sort);


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