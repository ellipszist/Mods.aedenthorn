using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
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
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {
        

        public static void CreateAnimatedTiles()
        {
            animatedTiles.Clear();
            foreach (var tlayer in Game1.getLocationFromName("Town").Map.Layers)
            {
                var layer = openWorldLocation.Map.GetLayer(tlayer.Id);
                if (layer is null)
                    continue;
                for(int x = 0; x < tlayer.LayerWidth; x++)
                {
                    for (int y = 0; y < tlayer.LayerHeight; y++)
                    {
                        if (tlayer.Tiles[x, y] is AnimatedTile tile)
                        {
                            string sheetID = openWorldLocation.Map.TileSheets.FirstOrDefault(s => s.ImageSource == tile.TileSheet.ImageSource)?.Id;
                            if (sheetID == null)
                                continue;
                            if (!animatedTiles.ContainsKey(sheetID))
                            {
                                animatedTiles[sheetID] = new();
                            }
                            List<StaticTile> tiles = new List<StaticTile>();
                            foreach(var t in tile.TileFrames)
                            {
                                var ts = GetOpenWorldTileSheet(t.TileSheet);
                                tiles.Add(new StaticTile(layer, ts, BlendMode.Alpha, t.TileIndex));
                            }
                            animatedTiles[sheetID][tile.TileIndex] = new AnimatedTile(layer, tiles.ToArray(), tile.FrameInterval);
                        }
                    }
                }
            }
        }

        public static TileSheet GetOpenWorldTileSheet(TileSheet tileSheet)
        {
            var nts = openWorldLocation.Map.TileSheets.FirstOrDefault(s => s.ImageSource == tileSheet.ImageSource);
            if (nts == null)
            {
                nts = new TileSheet(tileSheet.Id, openWorldLocation.Map, tileSheet.ImageSource, tileSheet.SheetSize, new xTile.Dimensions.Size(64, 64));
                openWorldLocation.Map.AddTileSheet(nts);
            }
            return nts;
        }

        private void ReloadOpenWorld(bool force)
        {
            for (int i = loadedChunks.Count - 1; i >= 0; i--)
            {
                UnloadChunk(loadedChunks[i]);
            }
            loadedChunks.Clear();
            landmarkRects.Clear();
            landmarkDict = SHelper.GameContent.Load<Dictionary<string, Landmark>>(landmarkDictPath);
            foreach (var landmark in landmarkDict.Values)
            {
                var map = SHelper.GameContent.Load<Map>(landmark.MapPath);
                Rectangle mapBox = new(landmark.MapPosition, new(map.Layers[0].LayerWidth, map.Layers[0].LayerHeight));
                landmarkRects.Add(mapBox);
            }

            if (force)
            {
                treeCenters = new();
                rockCenters = new();
                lakeCenters = new();
                monsterCenters = new();
                
                RandomSeed = Utility.CreateRandomSeed(Game1.uniqueIDForThisGame / 100UL, Config.NewMapDaily ? Game1.stats.DaysPlayed * 10U + 1U : 0.0);

                Random r = Utility.CreateRandom(RandomSeed, 242);
                for (int i = 0; i < r.Next(Config.OpenWorldSize / Config.TilesPerForestMax * Config.OpenWorldSize, Config.OpenWorldSize / Config.TilesPerForestMin * Config.OpenWorldSize + 1); i++)
                {
                    Point ap = new(
                            r.Next(10, Config.OpenWorldSize - 10),
                            r.Next(10, Config.OpenWorldSize - 10)
                        );
                    Point cp = new(ap.X / openWorldChunkSize, ap.Y / openWorldChunkSize);
                    Vector2 rp = new(ap.X % openWorldChunkSize, ap.Y % openWorldChunkSize);
                    if (!treeCenters.ContainsKey(cp))
                    {
                        treeCenters[cp] = new();
                    }
                    treeCenters[cp][rp] = GetRandomTree(ap.ToVector2(), r);
                }
                for (int i = 0; i < r.Next(Config.OpenWorldSize / Config.TilesPerLakeMax * Config.OpenWorldSize, Config.OpenWorldSize / Config.TilesPerLakeMin * Config.OpenWorldSize + 1); i++)
                {
                    Point ap = new(
                            r.Next(10, Config.OpenWorldSize - 10),
                            r.Next(10, Config.OpenWorldSize - 10)
                        );
                    Point cp = new(ap.X / openWorldChunkSize, ap.Y / openWorldChunkSize);
                    Point rp = new(ap.X % openWorldChunkSize, ap.Y % openWorldChunkSize);
                    if (!lakeCenters.ContainsKey(cp))
                    {
                        lakeCenters[cp] = new();
                    }
                    if (!lakeCenters[cp].Exists(c => Vector2.Distance(c.ToVector2(), rp.ToVector2()) < openWorldChunkSize))
                        lakeCenters[cp].Add(rp);
                }
                for (int i = 0; i < r.Next(Config.OpenWorldSize / Config.TilesPerOutcropMax * Config.OpenWorldSize, Config.OpenWorldSize / Config.TilesPerOutcropMin * Config.OpenWorldSize + 1); i++)
                {
                    Point ap = new(
                            r.Next(10, Config.OpenWorldSize - 10),
                            r.Next(10, Config.OpenWorldSize - 10)
                        );
                    Point cp = new(ap.X / openWorldChunkSize, ap.Y / openWorldChunkSize);
                    Vector2 rp = new(ap.X % openWorldChunkSize, ap.Y % openWorldChunkSize);
                    if (!rockCenters.ContainsKey(cp))
                    {
                        rockCenters[cp] = new();
                    }
                    rockCenters[cp].Add(rp);
                }
                for (int i = 0; i < r.Next(Config.OpenWorldSize / Config.TilesPerGrassMax * Config.OpenWorldSize, Config.OpenWorldSize / Config.TilesPerGrassMin * Config.OpenWorldSize + 1); i++)
                {
                    Point ap = new(
                            r.Next(10, Config.OpenWorldSize - 10),
                            r.Next(10, Config.OpenWorldSize - 10)
                        );
                    Point cp = new(ap.X / openWorldChunkSize, ap.Y / openWorldChunkSize);
                    Vector2 rp = new(ap.X % openWorldChunkSize, ap.Y % openWorldChunkSize);
                    if (!grassCenters.ContainsKey(cp))
                    {
                        grassCenters[cp] = new();
                    }
                    grassCenters[cp].Add(rp);
                }
                for (int i = 0; i < r.Next(Config.OpenWorldSize / Config.TilesPerMonsterMax * Config.OpenWorldSize, Config.OpenWorldSize / Config.TilesPerMonsterMin * Config.OpenWorldSize + 1); i++)
                {
                    Point ap = new(
                            r.Next(10, Config.OpenWorldSize - 10),
                            r.Next(10, Config.OpenWorldSize - 10)
                        );
                    Point cp = new(ap.X / openWorldChunkSize, ap.Y / openWorldChunkSize);
                    Vector2 rp = new(ap.X % openWorldChunkSize, ap.Y % openWorldChunkSize);
                    if (!monsterCenters.ContainsKey(cp))
                    {
                        monsterCenters[cp] = new();
                    }
                    var ms = GetRandomMonsterSpawn(ap.ToVector2(), r);
                    if(ms != null)
                    {
                        monsterCenters[cp][rp] = ms;
                    }
                }
                cachedChunks = new();
            }
        }
        public static void BuildWorldChunks(List<Point> chunks)
        {
            for (int i = chunks.Count - 1; i >= 0; i--)
            {
                if (!IsChunkInMap(chunks[i]))
                    chunks.RemoveAt(i);
            }

            for (int i = chunks.Count - 1; i >= 0; i--)
            {
                var cp = chunks[i];
                WorldChunk chunk;
                if (!cachedChunks.TryGetValue(cp, out chunk))
                {
                    CacheChunk(cp);
                }
                else if (chunk.initialized)
                {
                    chunks.RemoveAt(i);
                }
            }

            foreach (var cp in chunks)
            {
                AddChestsToChunk(cp);
            }
            foreach (var cp in chunks)
            {
                AddTreesToChunk(cp);
            }
            foreach (var cp in chunks)
            {
                AddRocksToChunk(cp);
            }
            foreach (var cp in chunks)
            {
                AddGrassToChunk(cp);
            }
            foreach (var cp in chunks)
            {
                AddMonstersToChunk(cp);
            }
            foreach (var cp in chunks)
            {
                cachedChunks[cp].initialized = true;
            }
        }

        private static WorldChunk CacheChunk(Point cp)
        {
            var chunk = new WorldChunk();
            cachedChunks[cp] = chunk;
            chunk.tiles["Back"] = new Tile[openWorldChunkSize, openWorldChunkSize];
            chunk.tiles["Buildings"] = new Tile[openWorldChunkSize, openWorldChunkSize];
            chunk.tiles["Front"] = new Tile[openWorldChunkSize, openWorldChunkSize];

            AddLandmarksToChunk(cp);
            AddLakesToChunk(cp);
            AddGrassTilesToChunk(cp);
            AddBorderToChunk(cp);

            return chunk;
        }

        public static void AddLandmarksToChunk(Point cp)
        {
            var chunkBox = new Rectangle(cp.X * openWorldChunkSize, cp.Y * openWorldChunkSize, openWorldChunkSize, openWorldChunkSize);
            foreach (var landmark in landmarkDict.Values)
            {
                var map = SHelper.GameContent.Load<Map>(landmark.MapPath);
                Rectangle mapBox = new(landmark.MapPosition, new(map.Layers[0].LayerWidth, map.Layers[0].LayerHeight));
                if (!chunkBox.Intersects(mapBox))
                    continue;
                foreach (var l in map.Layers)
                {
                    var nl = openWorldLocation.Map.GetLayer(l.Id);
                    if (nl == null)
                    {
                        nl = new Layer(l.Id, openWorldLocation.Map, new xTile.Dimensions.Size(openWorldChunkSize, openWorldChunkSize), l.TileSize);
                        openWorldLocation.Map.AddLayer(nl);
                        openWorldLocation.SortLayers();
                    }
                    if (!cachedChunks[cp].tiles.TryGetValue(l.Id, out var tiles))
                    {
                        tiles = new Tile[openWorldChunkSize, openWorldChunkSize];
                        cachedChunks[cp].tiles[l.Id] = tiles;
                    }
                    for (int x = 0; x < l.Tiles.Array.GetLength(0); x++)
                    {
                        for (int y = 0; y < l.Tiles.Array.GetLength(1); y++)
                        {
                            var rp = new Point(landmark.MapPosition.X + x, landmark.MapPosition.Y + y);
                            openWorldLocation.terrainFeatures.Remove(rp.ToVector2());
                            if (l.Tiles[x, y] != null && chunkBox.Contains(rp))
                            {
                                var rx = landmark.MapPosition.X + x - chunkBox.X;
                                var ry = landmark.MapPosition.Y + y - chunkBox.Y;
                                var ot = l.Tiles[x, y];
                                var ots = ot.TileSheet;
                                var nts = GetOpenWorldTileSheet(ots);
                                Tile tile;
                                if (ot is AnimatedTile)
                                {
                                    List<StaticTile> frames = new();
                                    foreach (var t in (ot as AnimatedTile).TileFrames)
                                    {
                                        var nts2 = openWorldLocation.Map.TileSheets.FirstOrDefault(s => s.ImageSource == t.TileSheet.ImageSource);
                                        if (nts2 == null)
                                        {
                                            nts2 = new TileSheet(ots.Id, openWorldLocation.Map, ots.ImageSource, ots.SheetSize, l.TileSize);
                                            openWorldLocation.Map.AddTileSheet(nts);
                                        }
                                        frames.Add(new StaticTile(nl, nts2, t.BlendMode, t.TileIndex));
                                    }
                                    tile = new AnimatedTile(nl, frames.ToArray(), (ot as AnimatedTile).FrameInterval);
                                }
                                else
                                {
                                    tile = new StaticTile(nl, nts, ot.BlendMode, ot.TileIndex);
                                }

                                cachedChunks[cp].tiles[l.Id][rx, ry] = tile;
                            }
                        }
                    }
                    cachedChunks[cp].tiles[l.Id] = tiles;
                }
            }
        }


        private static void AddLakesToChunk(Point chunkPoint)
        {
            Random r = Utility.CreateRandom(RandomSeed, chunkPoint.X * chunkPoint.Y + chunkPoint.X, "lakes".GetHashCode());
            if (!lakeCenters.TryGetValue(chunkPoint, out var centers))
                return;
            var back = openWorldLocation.Map.GetLayer("Back");
            var mainSheet = openWorldLocation.Map.GetTileSheet("outdoors");
            foreach (var c in centers)
            {
                int maxTiles = r.Next(Config.MinLakeSize * 2, Config.MaxLakeSize * 2 + 1);
                List<Point> tiles = GetBlob(c, r, maxTiles);
                if(tiles != null)
                {
                    var padding = GetBlobPadding(tiles, 5, true);
                    foreach (var p in padding)
                    {
                        AddBlobTileToChunk(BorderTiles.meadow, GetRandomMeadowTile(r, back, mainSheet), back, mainSheet, chunkPoint, p, padding, r);
                    }
                    padding = GetBlobPadding(tiles, 3, true);
                    foreach (var p in padding)
                    {
                        AddBlobTileToChunk(BorderTiles.dirt, GetRandomDirtTile(r, back, mainSheet), back, mainSheet, chunkPoint, p, padding, r);
                    }
                    foreach (var p in tiles)
                    {
                        AddWaterTileToChunk(chunkPoint, p, tiles, r);
                    }
                }
            }
        }
        

        private static void AddWaterTileToChunk(Point chunkPoint, Point p, List<Point> tiles, Random r)
        {
            var back = openWorldLocation.Map.GetLayer("Back");
            var build = openWorldLocation.Map.GetLayer("Buildings");
            var mainSheet = openWorldLocation.Map.GetTileSheet("outdoors");
            AddTileToChunk(chunkPoint, "Back", p.X, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
            //return;
            if (!tiles.Contains(p + new Point(-1, 0)))
            {

                if (tiles.Contains(p + new Point(-1, 1)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1266), true);
                    if (!tiles.Contains(p + new Point(0, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1268), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y - 1, animatedTiles[mainSheet.Id][283], true);
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1243), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y - 2, animatedTiles[mainSheet.Id][183], true);
                    }
                }
                else if (tiles.Contains(p + new Point(-1, 2)))
                {
                    AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y, animatedTiles[mainSheet.Id][283], true);
                    if (tiles.Contains(p + new Point(0, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1241), true);
                    }
                    else
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1268), true);
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1243), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y - 1, animatedTiles[mainSheet.Id][183], true);
                    }

                }
                else
                {
                    if(tiles.Contains(p + new Point(-1, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Back", p.X - 2, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246) , true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X - 2, p.Y, animatedTiles[mainSheet.Id][258], true);
                    }
                    else if(tiles.Contains(p + new Point(-1, -2)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y, animatedTiles[mainSheet.Id][237], true);
                        if(!tiles.Contains(p + new Point(0, 1)))
                        {
                            AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y + 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y + 1, animatedTiles[mainSheet.Id][258], true);
                        }
                    }
                    else 
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y, animatedTiles[mainSheet.Id][r.Next() > 0.5 ? 208 : 233], true);

                        if (!tiles.Contains(p + new Point(0, -1)))
                        {
                            AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1268), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y - 1, animatedTiles[mainSheet.Id][r.Next() > 0.5 ? 208 : 233], true);
                            AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1243), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y - 2, animatedTiles[mainSheet.Id][183], true);
                        }
                        if (!tiles.Contains(p + new Point(0, 1)))
                        {
                            AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y + 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y + 1, animatedTiles[mainSheet.Id][258], true);
                        }
                    }
                }
            }
            if (!tiles.Contains(p + new Point(1, 0)))
            {
                if (tiles.Contains(p + new Point(1, 1)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1267), true);
                    if (!tiles.Contains(p + new Point(0, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1270), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y - 1, animatedTiles[mainSheet.Id][284], true);
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1245), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y - 2, animatedTiles[mainSheet.Id][185], true);
                    }
                }
                else if (tiles.Contains(p + new Point(1, 2)))
                {
                    AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y, animatedTiles[mainSheet.Id][284], true);
                    if (tiles.Contains(p + new Point(0, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1242), true);
                    }
                    else
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1270), true);
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1245), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y - 1, animatedTiles[mainSheet.Id][185], true);
                    }   

                }
                else
                {
                    if (tiles.Contains(p + new Point(1, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Back", p.X + 2, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 2, p.Y, animatedTiles[mainSheet.Id][260], true);
                    }
                    else if (tiles.Contains(p + new Point(1, -2)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y, animatedTiles[mainSheet.Id][238], true);
                    }
                    else
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y, animatedTiles[mainSheet.Id][r.Next() > 0.5 ? 210 : 235], true);

                        if (!tiles.Contains(p + new Point(0, -1)))
                        {
                            AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1270), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y - 1, animatedTiles[mainSheet.Id][r.Next() > 0.5 ? 210 : 235], true);
                            AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1245), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y - 2, animatedTiles[mainSheet.Id][185], true);
                        }
                        if (!tiles.Contains(p + new Point(0, 1)))
                        {
                            AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y + 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y + 1, animatedTiles[mainSheet.Id][260], true);
                        }
                    }
                }
            }
            if (!tiles.Contains(p + new Point(0, -1)))
            {
                if(!tiles.Contains(p + new Point(1, -1)) && !tiles.Contains(p + new Point(-1, -1)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1269), true);
                    AddTileToChunk(chunkPoint, "Back", p.X, p.Y - 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1244), true);
                    AddTileToChunk(chunkPoint, "Buildings", p.X, p.Y - 2, animatedTiles[mainSheet.Id][184], true);
                }
            }
            if (!tiles.Contains(p + new Point(0, 1)))
            {
                AddTileToChunk(chunkPoint, "Back", p.X, p.Y + 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                if (tiles.Contains(p + new Point(1, 1)))
                {
                    if(!tiles.Contains(p + new Point(1, 2)))
                    { 
                        AddTileToChunk(chunkPoint, "Back", p.X, p.Y + 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X, p.Y + 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 212), true);


                    }
                }
                else if(tiles.Contains(p + new Point(-1, 1)))
                {
                    if (!tiles.Contains(p + new Point(-1, 2)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X, p.Y + 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X, p.Y + 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 213), true);
                        if(tiles.Contains(p + new Point(-2, 2)))
                        {
                            AddTileToChunk(chunkPoint, "Buildings", p.X, p.Y + 3, new StaticTile(back, mainSheet, BlendMode.Alpha, 178), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X, p.Y + 4, new StaticTile(back, mainSheet, BlendMode.Alpha, 253), true);
                        }
                    }

                }
                else
                {
                    AddTileToChunk(chunkPoint, "Buildings", p.X, p.Y + 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 259), true);
                }
            }
        }
        private static void AddBlobTileToChunk(BorderTiles border, Tile tile, Layer layer, TileSheet sheet, Point chunkPoint, Point v, List<Point> tiles, Random r)
        {


            AddTileToChunk(chunkPoint, "Back", v.X, v.Y, tile);
            if (!tiles.Contains(v + new Point(-1, 0)))
            {

                if (tiles.Contains(v + new Point(-1, 1)))
                {
                    AddTileToChunk(chunkPoint, "Back", v.X - 1, v.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.TL));
                    if (!tiles.Contains(v + new Point(-2, 1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", v.X - 2, v.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.TLC));
                    }
                }
                else if (tiles.Contains(v + new Point(-1, -1)))
                {
                    AddTileToChunk(chunkPoint, "Back", v.X - 1, v.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.BL));
                    if (!tiles.Contains(v + new Point(-2, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", v.X - 2, v.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.BLC));
                    }
                }
                else
                {
                    AddTileToChunk(chunkPoint, "Back", v.X - 1, v.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.L));
                    if (!tiles.Contains(v + new Point(0, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", v.X - 1, v.Y - 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.TLC));
                    }
                    if (!tiles.Contains(v + new Point(0, 1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", v.X - 1, v.Y + 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.BLC));
                    }
                }
            }
            if (!tiles.Contains(v + new Point(1, 0)))
            {

                if (tiles.Contains(v + new Point(1, 1)))
                {
                    AddTileToChunk(chunkPoint, "Back", v.X + 1, v.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.TR));
                    if (!tiles.Contains(v + new Point(2, 1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", v.X + 2, v.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.TRC));
                    }
                }
                else if (tiles.Contains(v + new Point(1, -1)))
                {
                    AddTileToChunk(chunkPoint, "Back", v.X + 1, v.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.BR));
                    if (!tiles.Contains(v + new Point(2, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", v.X + 2, v.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.BRC));
                    }
                }
                else
                {
                    AddTileToChunk(chunkPoint, "Back", v.X + 1, v.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.R));
                    if (!tiles.Contains(v + new Point(0, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", v.X + 1, v.Y - 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.TRC));
                    }
                    if (!tiles.Contains(v + new Point(0, 1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", v.X + 1, v.Y + 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.BRC));
                    }
                }
            }
            if (!tiles.Contains(v + new Point(0, -1)) && !tiles.Contains(v + new Point(-1, -1)) && !tiles.Contains(v + new Point(1, -1)))
            {

                AddTileToChunk(chunkPoint, "Back", v.X, v.Y - 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.T));
            }
            if (!tiles.Contains(v + new Point(0, 1)) && !tiles.Contains(v + new Point(-1, 1)) && !tiles.Contains(v + new Point(1, 1)))
            {
                AddTileToChunk(chunkPoint, "Back", v.X, v.Y + 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.B));
            }
        }

        private static Tile GetRandomMeadowTile(Random r, Layer back, TileSheet mainSheet)
        {
            var which = r.NextDouble();

            if (which < 0.01f)
            {
                return animatedTiles[mainSheet.Id][150];
            }
            else if (which < 0.02f)
            {
                return animatedTiles[mainSheet.Id][151];
            }
            else if (which < 0.03f)
            {
                return animatedTiles[mainSheet.Id][152];
            }
            else if (which < 0.04f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 400);
            }
            else if (which < 0.05f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 401);
            }
            else if (which < 0.05f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 254);
            }
            else if (which < 0.06f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 255);
            }
            else
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 175);
            }
        }
        private static Tile GetRandomDirtTile(Random r, Layer back, TileSheet mainSheet)
        {
            var which = r.NextDouble();

            if (which < 0.01f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 181);
            }
            else if (which < 0.02f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 488);
            }
            else if (which < 0.03f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 207);
            }
            else if (which < 0.04f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 614);
            }
            else if (which < 0.05f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 206);
            }
            else if (which < 0.05f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 560);
            }
            else if (which < 0.06f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 564);
            }
            else if (which < 0.07f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 513);
            }
            else if (which < 0.08f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 463);
            }
            else if (which < 0.09f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 463);
            }
            else if (which < 0.10f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 610);
            }
            else if (which < 0.11f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 589);
            }
            else
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 227);
            }
        }

        private static void AddTileToChunk(Point chunkPoint, string layer, int rx, int ry, Tile tile, bool water = false)
        {
            if (water)
            {
                tile.Properties["Water"] = "T";
            }
            Point offset = new();
            if (rx < 0)
            {
                offset += new Point(-1, 0);
            }
            else if (rx >= openWorldChunkSize)
            {
                offset += new Point(1, 0);
            }
            if (ry < 0)
            {
                offset += new Point(0, -1);
            }
            else if (ry >= openWorldChunkSize)
            {
                offset += new Point(0, 1);
            }
            chunkPoint += offset;

            if (!IsChunkInMap(chunkPoint))
                return;
            Point point = new Point(rx - offset.X * openWorldChunkSize, ry - offset.Y * openWorldChunkSize);
            if (!cachedChunks.TryGetValue(chunkPoint, out var chunk))
            {
                chunk = CacheChunk(chunkPoint);
            }
            if (!chunk.tiles.ContainsKey(layer))
            {
                chunk.tiles[layer] = new Tile[openWorldChunkSize, openWorldChunkSize];
            }
            chunk.tiles[layer][point.X, point.Y] = tile;
        }

        private static void AddGrassTilesToChunk(Point cp)
        {
            var layer = openWorldLocation.Map.GetLayer("Back");
            Random r = Utility.CreateRandom(RandomSeed, cp.X * cp.Y + cp.X, 42);
            var mainSheet = openWorldLocation.Map.GetTileSheet("outdoors");
            for (int y = 0; y < openWorldChunkSize; y++)
            {
                for (int x = 0; x < openWorldChunkSize; x++)
                {
                    if (cachedChunks[cp].tiles["Back"][x, y] != null)
                        continue;
                    int idx = 351;
                    var which = r.NextDouble();
                    if (which < 0.01f)
                    {
                        idx = 304;
                    }
                    else if (which < 0.02f)
                    {
                        idx = 305;
                    }
                    else if (which < 0.03f)
                    {
                        idx = 300;
                    }
                    AddTileToChunk(cp, "Back", x, y, new StaticTile(layer, mainSheet, BlendMode.Alpha, idx));
                    
                }
            }
        }

        private static void AddBorderToChunk(Point cp)
        {
            Random r = Utility.CreateRandom(RandomSeed);

            var grassTiles = new int[] { 150, 151, 152, 175, 175, 175, 175, 175, 175 };
            var rightTiles = new int[] { 316, 341, 366, 391, 416 };
            var leftTiles = new int[] { 319, 344, 369, 394, 419 };
            var buildLayer = openWorldLocation.Map.GetLayer("Buildings");
            var frontLayer = openWorldLocation.Map.GetLayer("Front");
            var ts = openWorldLocation.Map.GetTileSheet("outdoors");
            var chunk = cachedChunks[cp];
            if (cp.X == 0)
            {
                for (int y = 0; y < openWorldChunkSize; y++)
                {
                    chunk.tiles["Buildings"][0, y] = new StaticTile(buildLayer, ts, BlendMode.Alpha, leftTiles[r.Next(leftTiles.Length)]);
                }
            }
            else if (cp.X == Config.OpenWorldSize / openWorldChunkSize - 1)
            {
                for (int y = 0; y < openWorldChunkSize; y++)
                {
                    chunk.tiles["Buildings"][openWorldChunkSize - 1, y] = new StaticTile(buildLayer, ts, BlendMode.Alpha, rightTiles[r.Next(rightTiles.Length)]);
                }
            }
            if (cp.Y == 0)
            {
                for (int x = 0; x < openWorldChunkSize; x++)
                {
                    chunk.tiles["Buildings"][x, 0] = new StaticTile(buildLayer, ts, BlendMode.Alpha, 217);
                }
            }
            else if (cp.Y == Config.OpenWorldSize / openWorldChunkSize - 1)
            {

                for (int x = 0; x < openWorldChunkSize; x++)
                {
                    if (cp.X == Config.OpenWorldSize / openWorldChunkSize / 2)
                    {
                        var off = x - openWorldChunkSize / 2;
                        if (off == 0 || off == 1)
                        {
                            continue;
                        }
                        else if (off == -1)
                        {
                            chunk.tiles["Front"][x, openWorldChunkSize - 2] = new StaticTile(frontLayer, ts, BlendMode.Alpha, 438);
                            chunk.tiles["Buildings"][x, openWorldChunkSize - 1] = new StaticTile(buildLayer, ts, BlendMode.Alpha, 217);
                            chunk.tiles["Front"][x, openWorldChunkSize - 1] = new StaticTile(frontLayer, ts, BlendMode.Alpha, leftTiles[r.Next(leftTiles.Length)]);
                        }
                        else if (off == 2)
                        {
                            chunk.tiles["Front"][x, openWorldChunkSize - 2] = new StaticTile(frontLayer, ts, BlendMode.Alpha, 439);
                            chunk.tiles["Buildings"][x, openWorldChunkSize - 1] = new StaticTile(buildLayer, ts, BlendMode.Alpha, 217);
                            chunk.tiles["Front"][x, openWorldChunkSize - 1] = new StaticTile(frontLayer, ts, BlendMode.Alpha, rightTiles[r.Next(rightTiles.Length)]);
                        }
                        else
                        {
                            chunk.tiles["Front"][x, openWorldChunkSize - 2] = new StaticTile(buildLayer, ts, BlendMode.Alpha, r.Next(413, 415));
                            chunk.tiles["Buildings"][x, openWorldChunkSize - 1] = new StaticTile(buildLayer, ts, BlendMode.Alpha, 217);
                            chunk.tiles["Front"][x, openWorldChunkSize - 1] = new StaticTile(frontLayer, ts, BlendMode.Alpha, grassTiles[r.Next(grassTiles.Length)]);
                        }
                    }
                    else
                    {
                        chunk.tiles["Front"][x, openWorldChunkSize - 2] = new StaticTile(buildLayer, ts, BlendMode.Alpha, r.Next(413, 415));
                        chunk.tiles["Buildings"][x, openWorldChunkSize - 1] = new StaticTile(buildLayer, ts, BlendMode.Alpha, 217);
                        chunk.tiles["Front"][x, openWorldChunkSize - 1] = new StaticTile(frontLayer, ts, BlendMode.Alpha, grassTiles[r.Next(grassTiles.Length)]);
                    }
                }
            }
        }


        private static void AddRocksToChunk(Point cp)
        {

            Random r = Utility.CreateRandom(RandomSeed, cp.X * cp.Y + cp.X, 42);
            if (!rockCenters.TryGetValue(cp, out var centers))
                return;

            var back = openWorldLocation.Map.GetLayer("Back");
            var mainSheet = openWorldLocation.Map.GetTileSheet("outdoors");

            MethodInfo litter = typeof(MineShaft).GetMethod("createLitterObject", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var c in centers)
            {
                foreach(var oc in centers)
                {
                    if (Vector2.Distance(oc, c) < openWorldChunkSize / 4)
                        goto cont;
                }
                foreach(var lc in lakeCenters.Keys)
                {
                    if (Vector2.Distance(lc.ToVector2(), c) < openWorldChunkSize / 4)
                        goto cont;
                }
                Point begin = new(0, 0);
                Point end = new(0, 0);
                var level = (int)((Config.OpenWorldSize - c.Y - cp.Y * openWorldChunkSize) / Config.OpenWorldSize * Config.MaxOutcropLevel);
                MineShaft shaft = new(level);
                double gemStoneChance = 0.0015 + 0.0015 * shaft.mineLevel / 10;
                int idx = 0;
                int rocks = r.Next(Config.MinRocksPerOutcrop, Config.MaxRocksPerOutcrop + 1);
                while (idx < rocks)
                {
                    for (int x = begin.X; x <= end.X; x++)
                    {
                        for (int y = begin.Y; y <= end.Y; y++)
                        {
                            if (x != begin.X && x != end.X && y != begin.Y && y != end.Y)
                                continue;
                            Vector2 v = c + new Vector2(x, y);
                            if (r.NextDouble() < Math.Pow(Config.RockDensity, Vector2.Distance(v, c) / 3f))
                            {
                                Point offset = new();
                                if (v.X < 0)
                                {
                                    offset += new Point(-1, 0);
                                }
                                else if (v.X >= openWorldChunkSize)
                                {
                                    offset += new Point(1, 0);
                                }
                                if (v.Y < 0)
                                {
                                    offset += new Point(0, -1);
                                }
                                else if (v.Y >= openWorldChunkSize)
                                {
                                    offset += new Point(0, 1);
                                }
                                if (offset.X == 0 && offset.Y == 0)
                                {
                                    var av = v + cp.ToVector2() * openWorldChunkSize;

                                    if (!IsOpenTile(av))
                                        continue;
                                    if (!cachedChunks[cp].objects.ContainsKey(av))
                                        cachedChunks[cp].objects[av] = (Object)litter.Invoke(shaft, new object[] { 0.001, 5E-05, gemStoneChance, av });
                                }
                                else
                                {
                                    Point ocp = cp + offset;
                                    if (IsChunkInMap(ocp))
                                    {
                                        var av = v + ocp.ToVector2() * openWorldChunkSize;
                                        if (!cachedChunks.ContainsKey(ocp))
                                        {
                                           CacheChunk(ocp);
                                        }
                                        var rect = new Rectangle(5106, 9886, 20, 20);
                                        if (rect.Contains(av.ToPoint()))
                                        {
                                            var asdf = true;
                                        }

                                        if (!IsOpenTile(av))
                                            continue;
                                        if (!cachedChunks[ocp].objects.ContainsKey(av))
                                            cachedChunks[ocp].objects[av] = (Object)litter.Invoke(shaft, new object[] { 0.001, 5E-05, gemStoneChance, av });
                                    }
                                }
                                idx++;
                                if (idx >= rocks)
                                    goto next;
                            }
                        }
                    }
                    begin -= new Point(1, 1);
                    end += new Point(1, 1);
                    if (end.X > openWorldChunkSize / 10)
                        break;
                }
            next:
                List<Point> tiles = GetBlob(c.ToPoint(), r, (int)Math.Pow(end.X - begin.X + 4, 2));
                if (tiles != null)
                {
                    var padding = GetBlobPadding(tiles, 2, true);
                    foreach (var ap in padding)
                    {
                        AddBlobTileToChunk(BorderTiles.meadow, GetRandomMeadowTile(r, back, mainSheet), back, mainSheet, cp, ap, padding, r);
                    }
                    foreach (var ap in tiles)
                    {
                        AddBlobTileToChunk(BorderTiles.dirt, GetRandomDirtTile(r, back, mainSheet), back, mainSheet, cp, ap, tiles, r);
                    }
                }
            cont:
                continue;
            }
        }
        private static void AddGrassToChunk(Point cp)
        {

            Random r = Utility.CreateRandom(RandomSeed, cp.X * cp.Y + cp.X, 42);
            if (!grassCenters.TryGetValue(cp, out var centers))
                return;
            foreach (var c in centers)
            {
                Point begin = new(0, 0);
                Point end = new(0, 0);
                int idx = 0;
                int grass = r.Next(Config.MinGrassPerField, Config.MaxGrassPerField + 1);
                while (idx < grass)
                {
                    for (int x = begin.X; x <= end.X; x++)
                    {
                        for (int y = begin.Y; y <= end.Y; y++)
                        {
                            if (x != begin.X && x != end.X && y != begin.Y && y != end.Y)
                                continue;
                            Vector2 v = c + new Vector2(x, y);
                            if (r.NextDouble() < Math.Pow(Config.GrassDensity, Vector2.Distance(v, c) / 3f))
                            {
                                Point offset = new();
                                if (v.X < 0)
                                {
                                    offset += new Point(-1, 0);
                                }
                                else if (v.X >= openWorldChunkSize)
                                {
                                    offset += new Point(1, 0);
                                }
                                if (v.Y < 0)
                                {
                                    offset += new Point(0, -1);
                                }
                                else if (v.Y >= openWorldChunkSize)
                                {
                                    offset += new Point(0, 1);
                                }
                                if (offset.X == 0 && offset.Y == 0)
                                {
                                    var av = v + cp.ToVector2() * openWorldChunkSize;

                                    if (!IsOpenTile(av))
                                        continue;
                                    if (!cachedChunks[cp].terrainFeatures.ContainsKey(av))
                                        cachedChunks[cp].terrainFeatures[av] = new Grass(1, 3);
                                }
                                else
                                {
                                    Point ocp = cp + offset;
                                    if (IsChunkInMap(ocp))
                                    {
                                        var av = v + ocp.ToVector2() * openWorldChunkSize;
                                        if (!cachedChunks.ContainsKey(ocp))
                                        {
                                           CacheChunk(ocp);
                                        }
                                        var rect = new Rectangle(5106, 9886, 20, 20);
                                        if (rect.Contains(av.ToPoint()))
                                        {
                                            var asdf = true;
                                        }

                                        if (!IsOpenTile(av))
                                            continue;
                                        if (!cachedChunks[ocp].terrainFeatures.ContainsKey(av))
                                            cachedChunks[ocp].terrainFeatures[av] = new Grass(1, 3);
                                    }
                                }
                                idx++;
                                if (idx >= grass)
                                    goto next;
                            }
                        }
                    }
                    begin -= new Point(1, 1);
                    end += new Point(1, 1);
                    if (end.X > openWorldChunkSize / 10)
                        break;
                }
            next:
                continue;
            }
        }

        public static void AddChestsToChunk(Point cp)
        {
            if (advancedLootFrameworkApi is null)
                return;
            Random r = Utility.CreateRandom(RandomSeed, 942);
            int freeTiles = Enumerable.Range(0, openWorldChunkSize * openWorldChunkSize).Count(i => IsOpenTile(new Vector2(i % openWorldChunkSize + cp.X * openWorldChunkSize, i / openWorldChunkSize + cp.Y * openWorldChunkSize)));
            float chestCount = freeTiles / (float)(Config.TilesPerChestMin + ((Config.TilesPerChestMax - Config.TilesPerChestMin) * r.NextDouble() * cp.Y * openWorldChunkSize / Config.OpenWorldSize )); 
            int spawnedChestCount = Math.Min(freeTiles, (int)Math.Floor(chestCount));
            int i = 0;
            int attempt = 0;
            while (i < spawnedChestCount && attempt < spawnedChestCount * 10)
            {
                Vector2 freeTile = new(r.Next(openWorldChunkSize), r.Next(openWorldChunkSize));
                var av = ToGlobalTile(cp, freeTile);
                if (IsOpenTile(av))
                {
                    double fraction = Math.Pow(r.NextDouble(), 1 / Config.RarityChance);
                    int level = (int)Math.Ceiling(fraction * cp.Y * openWorldChunkSize / Config.OpenWorldSize);
                    Chest chest = advancedLootFrameworkApi.MakeChest(treasuresList, Config.ItemListChances, Config.MaxItems, Config.MinItemValue, Config.MaxItemValue, level, Config.IncreaseRate, Config.ItemsBaseMaxValue, freeTile);
                    chest.CanBeGrabbed = false;
                    chest.playerChoiceColor.Value = MakeTint(fraction);
                    chest.modData.Add(modKey, "T");
                    chest.modData.Add(modCoinKey, advancedLootFrameworkApi.GetChestCoins(level, Config.IncreaseRate, Config.CoinBaseMin, Config.CoinBaseMax).ToString());
                    cachedChunks[cp].overlayObjects[av] = chest;
                    i++;
                }
                attempt++;
            }
        }

        public static Color MakeTint(double fraction)
        {
            return tintColors[(int)Math.Floor(fraction * tintColors.Length)];
        }

        public static Vector2 ToGlobalTile(Point p, Vector2 tile)
        {
            return tile + new Vector2(p.X * openWorldChunkSize, p.Y * openWorldChunkSize);
        }

        private static void AddTreesToChunk(Point cp)
        {

            Random r = Utility.CreateRandom(RandomSeed, cp.X * cp.Y + cp.X, 42);
            if (!treeCenters.TryGetValue(cp, out var centers))
                return;
            foreach (var c in centers)
            {
                Point begin = new(0, 0);
                Point end = new(0, 0);
                int idx = 0;
                int trees = r.Next(Config.MinTreesPerForest, Config.MaxTreesPerForest + 1);
                while (idx < trees)
                {
                    foreach (var rect in landmarkRects)
                    {
                        var bounds = new Rectangle(c.Key.ToPoint() + begin - new Point(1, 1), (c.Key.ToPoint() + end + new Point(1, 1)) - (c.Key.ToPoint() + begin - new Point(1, 1)));
                        if (rect.Intersects(bounds))
                            goto next;
                    }
                    for (int x = begin.X; x <= end.X; x++)
                    {
                        for (int y = begin.Y; y <= end.Y; y++)
                        {
                            if (x != begin.X && x != end.X && y != begin.Y && y != end.Y)
                                continue;
                            Vector2 v = c.Key + new Vector2(x, y);
                            if (r.NextDouble() < Math.Pow(Config.TreeDensity, Vector2.Distance(v, c.Key) / 3f))
                            {
                                Point offset = new();
                                if (v.X < 0)
                                {
                                    offset += new Point(-1, 0);
                                }
                                else if (v.X >= openWorldChunkSize)
                                {
                                    offset += new Point(1, 0);
                                }
                                if (v.Y < 0)
                                {
                                    offset += new Point(0, -1);
                                }
                                else if (v.Y >= openWorldChunkSize)
                                {
                                    offset += new Point(0, 1);
                                }
                                if (offset.X == 0 && offset.Y == 0)
                                {
                                    var av = v + cp.ToVector2() * openWorldChunkSize;
                                    if (!IsOpenTile(av))
                                        continue;
                                    if (!cachedChunks[cp].terrainFeatures.ContainsKey(av))
                                        cachedChunks[cp].terrainFeatures[av] = new Tree(GetRandomTree(av, r, c.Value), r.NextDouble() < 0.2 ? 4 : 5);
                                }
                                else
                                {
                                    Point ocp = cp + offset;
                                    if (IsChunkInMap(ocp))
                                    {
                                        var av = v + ocp.ToVector2() * openWorldChunkSize;
                                        if (!cachedChunks.ContainsKey(ocp))
                                        {
                                            CacheChunk(ocp);
                                        }
                                        if (!IsOpenTile(av))
                                            continue;
                                        if (!cachedChunks[ocp].terrainFeatures.ContainsKey(av))
                                            cachedChunks[ocp].terrainFeatures[av] = new Tree(GetRandomTree(av, r, c.Value), r.NextDouble() < 0.2 ? 4 : 5);
                                    }
                                }
                                idx++;
                                if (idx >= trees)
                                    goto next;
                            }
                        }
                    }
                    begin -= new Point(1, 1);
                    end += new Point(1, 1);
                    if (end.X > openWorldChunkSize / 5)
                        break;
                }
            next:
                continue;
            }
        }

        private static string GetRandomTree(Vector2 p, Random r, string which = null)
        {
            float distance = (Config.OpenWorldSize - p.Y) / Config.OpenWorldSize;
            Dictionary<string, double> chances = new()
            {
                { "1", Math.Max(0, 1 - distance) },
                { "3", Math.Pow(distance, 2) },
                { "6", Math.Max(0, 1 - Math.Pow(distance, 2) * 2) },
                { "7", Math.Max(0, 1 - Math.Pow(distance, 2) * 2) / 2 },
                { "8", Math.Pow(distance, 2) / 2 },
                { "9", Math.Max(0, 1 - Math.Pow(distance, 2) * 2) }
            };
            double total = 0;
            foreach (var key in chances.Keys.ToArray())
            {
                if (which == key)
                    chances[key] *= 8;
                total += chances[key];
            }
            double roll = r.NextDouble();
            double count = 0;
            foreach (var kvp in chances)
            {
                count += kvp.Value / total;
                if (roll < count)
                {
                    return kvp.Key;
                }
            }
            return "1";
        }
        private Dictionary<string, MonsterSpawnInfo> GetMonsterDict()
        {
            if (File.Exists(Path.Combine(SHelper.DirectoryPath, "monsters.json")))
            {
                return JsonConvert.DeserializeObject<Dictionary<string, MonsterSpawnInfo>>(File.ReadAllText(Path.Combine(SHelper.DirectoryPath, "monsters.json")));
            }
            var dict = new Dictionary<string, MonsterSpawnInfo>();
            dict["Slimes"] = new MonsterSpawnInfo()
            {
                Chance = 1,
                Monsters = new List<MonsterInfo>()
                    {
                        new MonsterInfo()
                        {
                            Type = "GreenSlime",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 3,
                            Max = 10
                        },
                        new MonsterInfo()
                        {
                            Type = "BigSlime",
                            Chance = 0.5,
                            MinLevel = 1,
                            MaxLevel = 150,
                            Min = 1,
                            Max = 4
                        }
                    }
            };
            dict["Dinos"] = new MonsterSpawnInfo()
            {
                Chance = 0.5,
                Difficulty = 0.3f,
                Monsters = new List<MonsterInfo>()
                    {
                        new MonsterInfo()
                        {
                            Type = "Dino",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 3,
                            Max = 10
                        }
                    }
            };
            dict["Serpents"] = new MonsterSpawnInfo()
            {
                Chance = 0.3,
                Difficulty = 0.7f,
                Monsters = new List<MonsterInfo>()
                    {
                        new MonsterInfo()
                        {
                            Type = "Serpent",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 3,
                            Max = 10
                        }
                    }
            };
            dict["RockCrabs"] = new MonsterSpawnInfo()
            {
                Chance = 0.7,
                Difficulty = 0.1f,
                Monsters = new List<MonsterInfo>()
                    {
                        new MonsterInfo()
                        {
                            Type = "RockCrab",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 1,
                            Max = 1
                        }
                    }
            };
            dict["RockGolem"] = new MonsterSpawnInfo()
            {
                Chance = 0.7,
                Difficulty = 0.4f,
                Monsters = new List<MonsterInfo>()
                    {
                        new MonsterInfo()
                        {
                            Type = "RockCrab",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 1,
                            Max = 3
                        }
                    }
            };
            dict["ShadowTribe"] = new MonsterSpawnInfo()
            {
                Chance = 0.7,
                Difficulty = 0.6f,
                Monsters = new List<MonsterInfo>()
                    {
                        new MonsterInfo()
                        {
                            Type = "ShadowGirl",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 1,
                            Max = 3
                        },
                        new MonsterInfo()
                        {
                            Type = "ShadowGuy",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 1,
                            Max = 3
                        },
                        new MonsterInfo()
                        {
                            Type = "ShadowBrute",
                            Chance = 0.5,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 1,
                            Max = 2
                        },
                        new MonsterInfo()
                        {
                            Type = "ShadowShaman",
                            Chance = 0.5,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 1,
                            Max = 1
                        }
                    }
            };
            File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "monsters.json"), JsonConvert.SerializeObject(dict, Formatting.Indented));
            return dict;
        }

        private static string GetRandomMonsterSpawn(Vector2 ap, Random r, string which = null)
        {
            var monsterDict = SHelper.GameContent.Load<Dictionary<string, MonsterSpawnInfo>>(monsterDictPath);

            Dictionary<string, double> chances = new();
            double total = 0;
            foreach (var m in monsterDict.Values)
            {
                var diffMod = m.Difficulty * (ap.Y / Config.OpenWorldSize);
                var chance = Math.Max(0, m.Chance - diffMod);
                total += chance;
            }
            double roll = r.NextDouble();
            double count = 0;
            foreach (var m in monsterDict)
            {
                var diffMod = m.Value.Difficulty * (ap.Y / Config.OpenWorldSize);
                var chance = Math.Max(0, m.Value.Chance - diffMod);
                count += chance / total;
                if (roll < count)
                {
                    return m.Key;
                }
            }
            return null;
        }
        private static void AddMonstersToChunk(Point cp)
        {
            var monsterDict = SHelper.GameContent.Load<Dictionary<string, MonsterSpawnInfo>>(monsterDictPath);

            Random r = Utility.CreateRandom(RandomSeed, cp.X * cp.Y + cp.X, 142);

            if (!monsterCenters.TryGetValue(cp, out var mcs))
                return;
            foreach (var mc in mcs)
            {
                var mi = monsterDict[mc.Value];

                List<MonsterSpawn> monsters = new();
                foreach (var m in mi.Monsters)
                {
                    if (r.NextDouble() > m.Chance)
                        continue;
                    int amount = r.Next(m.Min, m.Max + 1);
                    if (amount == 0)
                        continue;
                    for (int i = 0; i < amount; i++)
                    {
                        monsters.Add(new() { Type = m.Type, Level = r.Next(m.MinLevel, m.MaxLevel + 1) });
                    }
                }
                if (!monsters.Any())
                    return;
                Point begin = new(0, 0);
                Point end = new(0, 0);
                int idx = 0;
                while (idx < monsters.Count)
                {
                    for (int x = begin.X; x <= end.X; x++)
                    {
                        for (int y = begin.Y; y <= end.Y; y++)
                        {
                            if (x != begin.X && x != end.X && y != begin.Y && y != end.Y)
                                continue;
                            Vector2 v = mc.Key + new Vector2(x, y);
                            if (r.NextDouble() < Math.Pow(Config.MonsterDensity, Vector2.Distance(v, mc.Key) / 3f))
                            {
                                Point offset = new();
                                if (v.X < 0)
                                {
                                    offset += new Point(-1, 0);
                                }
                                else if (v.X >= openWorldChunkSize)
                                {
                                    offset += new Point(1, 0);
                                }
                                if (v.Y < 0)
                                {
                                    offset += new Point(0, -1);
                                }
                                else if (v.Y >= openWorldChunkSize)
                                {
                                    offset += new Point(0, 1);
                                }
                                if (offset.X == 0 && offset.Y == 0)
                                {
                                    var av = v + cp.ToVector2() * openWorldChunkSize;
                                    cachedChunks[cp].monsters[av] = monsters[idx];
                                }
                                else
                                {
                                    int num = Config.OpenWorldSize / openWorldChunkSize;
                                    Point ocp = cp + offset;
                                    if (ocp.X >= 0 && ocp.X < num && ocp.Y >= 0 && ocp.Y < num)
                                    {
                                        var av = v + ocp.ToVector2() * openWorldChunkSize;
                                        if (!cachedChunks.ContainsKey(ocp))
                                        {
                                            CacheChunk(ocp);
                                        }
                                        cachedChunks[ocp].monsters[av] = monsters[idx];
                                    }
                                }
                                idx++;
                                if (idx >= monsters.Count)
                                    goto next;
                            }
                        }
                    }
                    begin -= new Point(1, 1);
                    end += new Point(1, 1);
                    if (end.X > openWorldChunkSize / 10)
                        break;
                }
            next:
                continue;
            }
        }

        public static void PlayerTileChanged()
        {
            int size = Config.OpenWorldSize / openWorldChunkSize;
            List<Point> keep = new();
            foreach (var f in Game1.getAllFarmers())
            {
                if (f.currentLocation.Name.Contains(locName))
                {
                    var pc = GetPlayerChunk(f);
                    if(!keep.Contains(pc))
                        keep.Add(pc);
                    foreach (var v in Utility.getSurroundingTileLocationsArray(pc.ToVector2()))
                    {
                        var p = v.ToPoint();
                        if (!keep.Contains(p))
                            keep.Add(p);
                    }
                }
            }
            for(int i = loadedChunks.Count - 1; i >= 0; i--)
            {
                var cp = loadedChunks[i];
                if (!keep.Contains(cp))
                {
                    UnloadChunk(cp);
                }
            }
            BuildWorldChunks(keep.ToList());
            foreach (var cp in keep)
            {
                TryLoadChunk(cp);
            }
        }

        private static void UnloadChunk(Point p)
        {
            for (int i = openWorldLocation.characters.Count - 1; i >= 0; i--)
            {
                Character c = openWorldLocation.characters[i];
                if (c.modData.TryGetValue(modKey, out var ps))
                {
                    string[] sps = ps.Split(',');
                    if (new Point(int.Parse(sps[0]), int.Parse(sps[1])) == p)
                    {
                        openWorldLocation.characters.RemoveAt(i);
                    }
                }
            }
            if (!cachedChunks.TryGetValue(p, out var chunk))
            {
                return;
            }
            foreach(var obj in chunk.objects)
            {
                openWorldLocation.objects.Remove(obj.Key);
            }
            foreach(var obj in chunk.overlayObjects)
            {
                openWorldLocation.overlayObjects.Remove(obj.Key);
            }
            foreach(var tf in chunk.terrainFeatures)
            {
                openWorldLocation.terrainFeatures.Remove(tf.Key);
            }
            loadedChunks.Remove(p);
        }

        private static void TryLoadChunk(Point pc)
        {
            if (!IsChunkInMap(pc))
                return;
            if (!loadedChunks.Contains(pc))
            {
                AddTreesFromChunk(pc);
                AddObjectsFromChunk(pc);
                AddMonstersFromChunk(pc);
                loadedChunks.Add(pc);
            }
        }

        private static void AddTreesFromChunk(Point pc)
        {
            var chunk = cachedChunks[pc];
            foreach (var t in chunk.terrainFeatures)
            {
                if (IsOpenTile(t.Key))
                {
                    openWorldLocation.terrainFeatures[t.Key] = t.Value;
                }
            }
        }
        private static void AddObjectsFromChunk(Point pc)
        {
            var chunk = cachedChunks[pc];
            foreach (var t in chunk.objects)
            {
                if (IsOpenTile(t.Key))
                {
                    openWorldLocation.objects[t.Key] = t.Value;
                }
            }
            foreach (var t in chunk.overlayObjects)
            {
                if (IsOpenTile(t.Key))
                {
                    openWorldLocation.overlayObjects[t.Key] = t.Value;
                }
            }
        }

        private static void AddMonstersFromChunk(Point pc)
        {
            var chunk = cachedChunks[pc];
            foreach (var t in chunk.monsters)
            {
                if (IsOpenTile(t.Key))
                {
                    Monster m = null;
                    switch (t.Value.Type)
                    {
                        case "GreenSlime":
                            m = new GreenSlime(t.Key * 64, t.Value.Level);
                            break;
                        case "BigSlime":
                            int mineArea = t.Value.Level;
                            if (mineArea < 121 && mineArea > 39)
                            {
                                mineArea = mineArea / 40 * 40;
                            }
                            else if (mineArea > 25)
                            {
                                mineArea = 40;
                            }
                            m = new BigSlime(t.Key * 64, mineArea);
                            break;
                        case "Dino":
                            m = new DinoMonster(t.Key * 64);
                            break;
                        case "BlueSquid":
                            m = new BlueSquid(t.Key * 64);
                            break;
                        case "RockCrab":
                            m = new RockCrab(t.Key * 64);
                            break;
                        case "RockGolem":
                            m = new RockGolem(t.Key * 64, t.Value.Level);
                            break;
                        case "Serpent":
                            m = new Serpent(t.Key * 64);
                            break;
                        case "ShadowBrute":
                            m = new ShadowBrute(t.Key * 64);
                            break;
                        case "ShadowShaman":
                            m = new ShadowShaman(t.Key * 64);
                            break;
                        case "ShadowGuy":
                            m = new ShadowBrute(t.Key * 64);
                            break;
                        case "ShadowGirl":
                            m = new ShadowGirl(t.Key * 64);
                            break;
                    }
                    if (m is not null)
                    {
                        m.modData[modKey] = $"{pc.X},{pc.Y}";
                        openWorldLocation.characters.Add(m);
                    }
                }
            }
        }

        public static void UpdateTreasuresList()
        {
            treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(Config.ItemListChances.Where(p => p.Value > 0).ToDictionary(s => s.Key, s => s.Value).Keys.ToArray(), Config.MinItemValue, Config.MaxItemValue);
        }


        public static void DrawMap(RenderedActiveMenuEventArgs e)
        {
            if (!showingMap)
                return;
            int mapSize = openWorldChunkSize;
            int tileSize = 16;
            mapSize = Math.Min(Math.Min(Game1.viewport.Height - 32, Game1.viewport.Width - 32) / 16, mapSize);

            Point playerChunk = GetPlayerChunk(Game1.player);
            Rectangle playerBox = new Rectangle(Game1.player.TilePoint.X - mapSize / 2, Game1.player.TilePoint.Y - mapSize / 2, mapSize, mapSize);

            if (renderTarget is null)
            {
                renderTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, mapSize * tileSize, mapSize * tileSize);
                Game1.graphics.GraphicsDevice.SetRenderTarget(renderTarget);

                var renderBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);
                renderBatch.Begin();

                foreach (var offset in GetSurroundingPointArray(true))
                {
                    var cp = playerChunk + offset;
                    Vector2 startPos = Vector2.Zero;
                    foreach (var l in openWorldLocation.Map.Layers)
                    {
                        var tiles = GetChunkTiles(l.Id, cp.X, cp.Y);
                        for (int x = 0; x < openWorldChunkSize; x++)
                        {
                            for (int y = 0; y < openWorldChunkSize; y++)
                            {
                                var ap = GetAbsolutePosition(cp, x, y);
                                if (!playerBox.Contains(ap))
                                    continue;
                                var rp = ap - playerBox.Location;
                                var pos = startPos + rp.ToVector2() * tileSize;
                                if (tiles == null || tiles[x, y] == null)
                                {
                                    if (l.Id == "Back")
                                    {
                                        renderBatch.Draw(Game1.staminaRect, new Rectangle(pos.ToPoint(), new Point(tileSize, tileSize)), null, Color.Black, 0, Vector2.Zero, SpriteEffects.None, 1);
                                    }
                                }
                                else
                                {
                                    var tile = tiles[x, y];
                                    Texture2D texture2D;
                                    var m_tileSheetTextures = AccessTools.FieldRefAccess<XnaDisplayDevice, Dictionary<TileSheet, Texture2D>>(Game1.mapDisplayDevice as XnaDisplayDevice, "m_tileSheetTextures");
                                    if (!m_tileSheetTextures.TryGetValue(tile.TileSheet, out texture2D))
                                    {
                                        Game1.mapDisplayDevice.LoadTileSheet(tile.TileSheet);
                                    }
                                    texture2D = m_tileSheetTextures[tile.TileSheet];
                                    if (!texture2D.IsDisposed)
                                    {
                                        renderBatch.Draw(texture2D, new Rectangle(pos.ToPoint(), new Point(tileSize, tileSize)), tile.TileSheet.GetTileImageBounds(tile.TileIndex).ToXna(), Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
                                    }
                                }
                                if (l.Id == "Back")
                                {
                                    if(openWorldLocation.Objects.TryGetValue(ap.ToVector2(), out var obj))
                                    {
                                        ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId);
                                        renderBatch.Draw(itemData.GetTexture(), new Rectangle(pos.ToPoint(), new Point(tileSize, tileSize)), itemData.GetSourceRect(0, obj.ParentSheetIndex), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1);
                                    }
                                    else if(openWorldLocation.terrainFeatures.TryGetValue(ap.ToVector2(), out var tf) && tf is Tree tree)
                                    {
                                        renderBatch.Draw(tree.texture.Value, new Rectangle(pos.ToPoint() - new Point(0, tileSize), new Point(tileSize, tileSize * 2)), Tree.treeTopSourceRect, Color.White, 0f, new Vector2(24f, 96f), tree.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);
                                    }
                                }

                            }
                        }
                    }
                }
                renderBatch.End();
                Game1.graphics.GraphicsDevice.SetRenderTarget(null);
                upperRightCloseButton = new ClickableTextureComponent(new Rectangle(Math.Max(0, Game1.viewport.Width / 2 - mapSize / 2 * tileSize - 32) + Math.Min(Game1.viewport.Width -8, mapSize * tileSize - 16), Math.Max(16, Game1.viewport.Height / 2 - mapSize / 2 * tileSize), 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
            }
            Game1.drawDialogueBox(Math.Max(0, Game1.viewport.Width / 2 - mapSize / 2 * tileSize - 32), Math.Max(-82,Game1.viewport.Height / 2 - mapSize / 2 * tileSize - 96), Math.Min(Game1.viewport.Width, mapSize * tileSize + 64), Math.Min(Game1.viewport.Height + 98, mapSize * tileSize + 128), false, true);

            mapRect = new Rectangle(Game1.viewport.Width / 2 - mapSize / 2 * tileSize, Game1.viewport.Height / 2 - mapSize / 2 * tileSize, mapSize * tileSize, mapSize * tileSize);

            e.SpriteBatch.Draw(renderTarget, new Vector2(Game1.viewport.Width / 2 - mapSize / 2 * tileSize, Game1.viewport.Height / 2 - mapSize / 2 * tileSize), Color.White);
            foreach (Farmer player in Game1.getOnlineFarmers())
            {
                if (!player.currentLocation.Name.Equals(locName))
                    continue;
                float alpha = player == Game1.player ? 1 : 0.75f;
                if (!playerBox.Contains(player.Tile))
                    continue;
                Vector2 pos = Game1.player.Position - player.Position + new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height / 2 - 16);
                player.FarmerRenderer.drawMiniPortrat(e.SpriteBatch, pos, 0.00011f, 2f, 2, player, alpha);
            }
            if(upperRightCloseButton is not null)
                upperRightCloseButton.draw(e.SpriteBatch);
            Game1.activeClickableMenu.drawMouse(e.SpriteBatch);


        }
    }
}