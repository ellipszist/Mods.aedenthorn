using Microsoft.Xna.Framework;
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
using xTile;
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
                animatedTiles[tlayer.Id] = new();
                for(int x = 0; x < tlayer.LayerWidth; x++)
                {
                    for (int y = 0; y < tlayer.LayerHeight; y++)
                    {
                        if (tlayer.Tiles[x, y] is AnimatedTile tile)
                        {
                            List<StaticTile> tiles = new List<StaticTile>();
                            foreach(var t in tile.TileFrames)
                            {
                                var ts = GetOpenWorldTileSheet(t.TileSheet);
                                tiles.Add(new StaticTile(layer, ts, BlendMode.Alpha, t.TileIndex));
                            }
                            animatedTiles[tlayer.Id][tile.TileIndex] = new AnimatedTile(layer, tiles.ToArray(), tile.FrameInterval);
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
            for(int i = loadedChunks.Count - 1; i >= 0; i--)
            {
                UnloadChunk(loadedChunks[i]);
            }
            loadedChunks = new();

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
                    monsterCenters[cp][rp] = GetRandomMonsterSpawn(ap.ToVector2(), r);
                }
                cachedChunks = new();
            }

        }
        public static void BuildWorldChunk(Point cp)
        {
            int size = Config.OpenWorldSize / openWorldChunkSize;
            if (cp.X < 0 || cp.Y < 0 || cp.X >= size || cp.Y >= size)
                return;
            WorldChunk chunk;
            if (!cachedChunks.TryGetValue(cp, out chunk))
            {
                chunk = CacheChunk(cp);
            }
            else if (chunk.initialized)
            {
                return;
            }
            //    await Task.Factory.StartNew(() => PreloadWorldChunkAsync(point, chunk));
            //}

            //private static void PreloadWorldChunkAsync(Point point, WorldChunk chunk)
            //{
            Stopwatch s = new();
            //s.Start();
            AddLandmarksToChunk(cp, chunk);
            //SMonitor.Log($"Landmarks in {s.ElapsedMilliseconds}");
            //s.Restart();
            AddLakesToChunk(cp, chunk);
            //SMonitor.Log($"grass in {s.ElapsedMilliseconds}");
            //s.Restart();
            AddGrassToChunk(cp, chunk);
            //SMonitor.Log($"grass in {s.ElapsedMilliseconds}");
            //s.Restart();
            AddBorderToChunk(cp, chunk);
            //SMonitor.Log($"border in {s.ElapsedMilliseconds}");
            //s.Restart();
            AddChestsToChunk(cp, chunk);
            //SMonitor.Log($"chests in {s.ElapsedMilliseconds}");
            //s.Restart();
            AddTreesToChunk(cp, chunk);
            //SMonitor.Log($"trees in {s.ElapsedMilliseconds}");
            //s.Restart();
            AddRocksToChunk(cp, chunk);
            //SMonitor.Log($"Rocks in {s.ElapsedMilliseconds}");
            //s.Restart();
            AddMonstersToChunk(cp, chunk);
            //SMonitor.Log($"Monsters {s.ElapsedMilliseconds}");
            s.Stop();
            SMonitor.Log($"Loaded chunk in {s.ElapsedMilliseconds}ms");
            cachedChunks[cp].initialized = true;
        }

        private static WorldChunk CacheChunk(Point cp)
        {
            var chunk = new WorldChunk();
            cachedChunks[cp] = chunk;
            chunk.tiles["Back"] = new Tile[openWorldChunkSize, openWorldChunkSize];
            chunk.tiles["Buildings"] = new Tile[openWorldChunkSize, openWorldChunkSize];
            chunk.tiles["Front"] = new Tile[openWorldChunkSize, openWorldChunkSize];
            return chunk;
        }

        public static void AddLandmarksToChunk(Point point, WorldChunk chunk)
        {
            landmarkDict = SHelper.GameContent.Load<Dictionary<string, Landmark>>(landmarkDictPath);

            var chunkBox = new Rectangle(point.X * openWorldChunkSize, point.Y * openWorldChunkSize, openWorldChunkSize, openWorldChunkSize);
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
                    if (!chunk.tiles.TryGetValue(l.Id, out var tiles))
                    {
                        tiles = new Tile[openWorldChunkSize, openWorldChunkSize];
                        chunk.tiles[l.Id] = tiles;
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

                                chunk.tiles[l.Id][rx, ry] = tile;
                            }
                        }
                    }
                    chunk.tiles[l.Id] = tiles;
                }
            }
        }


        private static void AddLakesToChunk(Point chunkPoint, WorldChunk chunk)
        {
            Random r = Utility.CreateRandom(RandomSeed, chunkPoint.X * chunkPoint.Y + chunkPoint.X, "lakes".GetHashCode());
            List<Rectangle> landmarks = new List<Rectangle>();
            foreach (var landmark in landmarkDict.Values)
            {
                var map = SHelper.GameContent.Load<Map>(landmark.MapPath);
                Rectangle mapBox = new(landmark.MapPosition, new(map.Layers[0].LayerWidth, map.Layers[0].LayerHeight));
                landmarks.Add(mapBox);
            }
            if (!lakeCenters.TryGetValue(chunkPoint, out var centers))
                return;
            foreach (var c in centers)
            {
                Point begin = new(0, 0);
                Point end = new(0, 0);
                int idx = 0;
                int maxTiles = r.Next(Config.MinLakeSize, Config.MaxLakeSize + 1);
                List<Point> tiles = new List<Point>();
                while (idx < maxTiles)
                {
                    foreach (var rect in landmarks)
                    {
                        var bounds = new Rectangle(c + begin - new Point(1, 1), (c + end + new Point(1, 1)) - (c + begin - new Point(1, 1)));
                        if (rect.Intersects(bounds))
                            goto next;
                    }
                    for (int x = begin.X; x <= end.X; x++)
                    {
                        for (int y = begin.Y; y <= end.Y; y++)
                        {
                            Point v = c + new Point(x, y);
                            if (tiles.Contains(v))
                                continue;
                            double chance = v == c ? 1 : 0;
                            if (tiles.Contains(v + new Point(-1, 0)))
                                chance += 0.334;
                            if (tiles.Contains(v + new Point(1, 0)))
                                chance += 0.334;
                            if (tiles.Contains(v + new Point(0, -1)))
                                chance += 0.334;
                            if (tiles.Contains(v + new Point(0, 1)))
                                chance += 0.334;
                            if(chance > 0 && chance < 1)
                            {
                                chance -= Vector2.Distance(v.ToVector2(), c.ToVector2()) / (openWorldChunkSize / 2);
                            }
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
                }
                
            cont:
                foreach (var v in tiles)
                {
                    AddWaterTileToChunk(chunkPoint, v, tiles);
                }

            next:
                continue;
            }
        }

        private static void AddWaterTileToChunk(Point chunkPoint, Point v, List<Point> tiles)
        {
            var back = openWorldLocation.Map.GetLayer("Back");
            var build = openWorldLocation.Map.GetLayer("Buildings");
            var mainSheet = openWorldLocation.Map.GetTileSheet("outdoors");
            AddTileToChunk(chunkPoint, "Back", v.X, v.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246));
            if (!tiles.Contains(v + new Point(-1, 0)))
            {
                if (tiles.Contains(v + new Point(-1, 1)))
                {
                    AddTileToChunk(chunkPoint, "Back", v.X - 1, v.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1266));
                    AddTileToChunk(chunkPoint, "Buildings", v.X - 1, v.Y - 1, animatedTiles["Buildings"][283]);
                }
                else if (tiles.Contains(v + new Point(-1, -1)))
                {
                    AddTileToChunk(chunkPoint, "Back", v.X - 1, v.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246));
                }
                else
                {
                    if (tiles.Contains(v + new Point(-1, 2)))
                    {
                        AddTileToChunk(chunkPoint, "Back", v.X - 1, v.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1241));
                        AddTileToChunk(chunkPoint, "Buildings", v.X - 1, v.Y, animatedTiles["Buildings"][283]);
                    }
                    else
                    {
                        AddTileToChunk(chunkPoint, "Back", v.X - 1, v.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246));
                        AddTileToChunk(chunkPoint, "Buildings", v.X - 1, v.Y, animatedTiles["Buildings"][208]);
                    }
                }
            }
            if (!tiles.Contains(v + new Point(1, 0)))
            {
                if (tiles.Contains(v + new Point(1, 1)))
                {
                    AddTileToChunk(chunkPoint, "Back", v.X + 1, v.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1267));
                    AddTileToChunk(chunkPoint, "Buildings", v.X + 1, v.Y - 1, animatedTiles["Buildings"][284]);
                }
                else if (tiles.Contains(v + new Point(1, -1)))
                {
                    AddTileToChunk(chunkPoint, "Back", v.X + 1, v.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246));
                }
                else
                {
                    if (tiles.Contains(v + new Point(1, 2)))
                    {
                        AddTileToChunk(chunkPoint, "Back", v.X + 1, v.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1242));
                        AddTileToChunk(chunkPoint, "Buildings", v.X + 1, v.Y, animatedTiles["Buildings"][284]);
                    }
                    else
                    {
                        AddTileToChunk(chunkPoint, "Back", v.X + 1, v.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246));
                        AddTileToChunk(chunkPoint, "Buildings", v.X + 1, v.Y, animatedTiles["Buildings"][210]);
                    }
                }
            }
        }

        private static void AddTileToChunk(Point chunkPoint, string layer, int x, int y, Tile tile)
        {
            Point offset = new();
            if (x < 0)
            {
                offset += new Point(-1, 0);
            }
            else if (x >= openWorldChunkSize)
            {
                offset += new Point(1, 0);
            }
            if (y < 0)
            {
                offset += new Point(0, -1);
            }
            else if (y >= openWorldChunkSize)
            {
                offset += new Point(0, 1);
            }
            chunkPoint += offset;

            if (!IsChunkInMap(chunkPoint))
                return;
            Point point = new Point(x - offset.X * openWorldChunkSize, y - offset.Y * openWorldChunkSize);
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

        private static void AddGrassToChunk(Point chunkPoint, WorldChunk chunk)
        {
            var layer = openWorldLocation.Map.GetLayer("Back");
            Random r = Utility.CreateRandom(RandomSeed, chunkPoint.X * chunkPoint.Y + chunkPoint.X, 42);
            var mainSheet = openWorldLocation.Map.GetTileSheet("outdoors");
            for (int y = 0; y < openWorldChunkSize; y++)
            {
                for (int x = 0; x < openWorldChunkSize; x++)
                {
                    if (chunk.tiles["Back"][x, y] != null)
                        continue;
                    int idx = 351;
                    var which = r.NextDouble();
                    if (which < 0.025f)
                    {
                        idx = 304;
                    }
                    else if (which < 0.05f)
                    {
                        idx = 305;
                    }
                    else if (which < 0.15f)
                    {
                        idx = 300;
                    }
                    chunk.tiles["Back"][x, y] = new StaticTile(layer, mainSheet, BlendMode.Alpha, idx);
                }
            }
        }

        private static void AddBorderToChunk(Point chunkPoint, WorldChunk chunk)
        {
            Random r = Utility.CreateRandom(RandomSeed);

            var grassTiles = new int[] { 150, 151, 152, 175, 175, 175, 175, 175, 175 };
            var rightTiles = new int[] { 316, 341, 366, 391, 416 };
            var leftTiles = new int[] { 319, 344, 369, 394, 419 };
            var buildLayer = openWorldLocation.Map.GetLayer("Buildings");
            var frontLayer = openWorldLocation.Map.GetLayer("Front");
            var ts = openWorldLocation.Map.GetTileSheet("outdoors");
            if (chunkPoint.X == 0)
            {
                for (int y = 0; y < openWorldChunkSize; y++)
                {
                    chunk.tiles["Buildings"][0, y] = new StaticTile(buildLayer, ts, BlendMode.Alpha, leftTiles[r.Next(leftTiles.Length)]);
                }
            }
            else if (chunkPoint.X == Config.OpenWorldSize / openWorldChunkSize - 1)
            {
                for (int y = 0; y < openWorldChunkSize; y++)
                {
                    chunk.tiles["Buildings"][openWorldChunkSize - 1, y] = new StaticTile(buildLayer, ts, BlendMode.Alpha, rightTiles[r.Next(rightTiles.Length)]);
                }
            }
            if (chunkPoint.Y == 0)
            {
                for (int x = 0; x < openWorldChunkSize; x++)
                {
                    chunk.tiles["Buildings"][x, 0] = new StaticTile(buildLayer, ts, BlendMode.Alpha, 217);
                }
            }
            else if (chunkPoint.Y == Config.OpenWorldSize / openWorldChunkSize - 1)
            {

                for (int x = 0; x < openWorldChunkSize; x++)
                {
                    if (chunkPoint.X == Config.OpenWorldSize / openWorldChunkSize / 2)
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


        private static void AddRocksToChunk(Point chunkPoint, WorldChunk chunk)
        {

            Random r = Utility.CreateRandom(RandomSeed, chunkPoint.X * chunkPoint.Y + chunkPoint.X, 42);
            if (!rockCenters.TryGetValue(chunkPoint, out var centers))
                return;
            MethodInfo litter = typeof(MineShaft).GetMethod("createLitterObject", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var c in centers)
            {
                Point begin = new(0, 0);
                Point end = new(0, 0);
                var level = (int)((Config.OpenWorldSize - c.Y - chunkPoint.Y * openWorldChunkSize) / Config.OpenWorldSize * Config.MaxOutcropLevel);
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
                            if (r.NextDouble() < Math.Pow(Config.TreeDensity, Vector2.Distance(v, c) / 3f))
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
                                    var av = v + chunkPoint.ToVector2() * openWorldChunkSize;
                                    if (!chunk.objects.ContainsKey(av))
                                        chunk.objects[av] = (Object)litter.Invoke(shaft, new object[] { 0.001, 5E-05, gemStoneChance, av });
                                }
                                else
                                {
                                    Point ocp = chunkPoint + offset;
                                    if (IsChunkInMap(ocp))
                                    {
                                        var av = v + ocp.ToVector2() * openWorldChunkSize;
                                        if (!cachedChunks.ContainsKey(ocp))
                                        {
                                           CacheChunk(ocp);
                                        }
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
                continue;
            }
        }
        public static void AddChestsToChunk(Point chunkPoint, WorldChunk chunk)
        {
            Random r = Utility.CreateRandom(RandomSeed, 942);
            int freeTiles = Enumerable.Range(0, openWorldChunkSize * openWorldChunkSize).Count(i => IsOpenTile(chunk, new Vector2(i % openWorldChunkSize + chunkPoint.X * openWorldChunkSize, i / openWorldChunkSize + chunkPoint.Y * openWorldChunkSize)));
            float chestCount = freeTiles / (float)(Config.TilesPerChestMin + ((Config.TilesPerChestMax - Config.TilesPerChestMin) * r.NextDouble() * chunkPoint.Y * openWorldChunkSize / Config.OpenWorldSize )); 
            int spawnedChestCount = Math.Min(freeTiles, (int)Math.Floor(chestCount));
            int i = 0;
            int attempt = 0;
            while (i < spawnedChestCount && attempt < spawnedChestCount * 10)
            {
                Vector2 freeTile = new(r.Next(openWorldChunkSize), r.Next(openWorldChunkSize));
                var av = ToGlobalTile(chunkPoint, freeTile);
                if (IsOpenTile(chunk, av))
                {
                    double fraction = Math.Pow(r.NextDouble(), 1 / Config.RarityChance);
                    int level = (int)Math.Ceiling(fraction * chunkPoint.Y * openWorldChunkSize / Config.OpenWorldSize);
                    Chest chest = advancedLootFrameworkApi.MakeChest(treasuresList, Config.ItemListChances, Config.MaxItems, Config.MinItemValue, Config.MaxItemValue, level, Config.IncreaseRate, Config.ItemsBaseMaxValue, freeTile);
                    chest.CanBeGrabbed = false;
                    chest.playerChoiceColor.Value = MakeTint(fraction);
                    chest.modData.Add(modKey, "T");
                    chest.modData.Add(modCoinKey, advancedLootFrameworkApi.GetChestCoins(level, Config.IncreaseRate, Config.CoinBaseMin, Config.CoinBaseMax).ToString());
                    chunk.overlayObjects[av] = chest;
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

        private static void AddTreesToChunk(Point chunkPoint, WorldChunk chunk)
        {

            Random r = Utility.CreateRandom(RandomSeed, chunkPoint.X * chunkPoint.Y + chunkPoint.X, 42);
            if (!treeCenters.TryGetValue(chunkPoint, out var centers))
                return;
            foreach (var c in centers)
            {

                Point begin = new(0, 0);
                Point end = new(0, 0);
                int idx = 0;
                int trees = r.Next(Config.MinTreesPerForest, Config.MaxTreesPerForest + 1);
                while (idx < trees)
                {
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
                                    var av = v + chunkPoint.ToVector2() * openWorldChunkSize;
                                    if (!chunk.terrainFeatures.ContainsKey(av))
                                        chunk.terrainFeatures[av] = new Tree(GetRandomTree(av, r, c.Value), r.NextDouble() < 0.2 ? 4 : 5);
                                }
                                else
                                {
                                    Point ocp = chunkPoint + offset;
                                    if (IsChunkInMap(ocp))
                                    {
                                        var av = v + ocp.ToVector2() * openWorldChunkSize;
                                        if (!cachedChunks.ContainsKey(ocp))
                                        {
                                            CacheChunk(ocp);
                                        }
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
                    if (end.X > openWorldChunkSize / 10)
                        break;
                }
            next:
                continue;
            }
        }

        private static string GetRandomTree(Vector2 p, Random r, string which = null)
        {
            float distance = Math.Abs(p.Y - Config.OpenWorldSize / 2) / (Config.OpenWorldSize / 2);
            Dictionary<string, double> chances = new();
            chances.Add("1", Math.Max(0, 1 - distance));
            chances.Add("3", Math.Pow(distance, 2));
            chances.Add("6", Math.Max(0, 1 - Math.Pow(distance, 2) * 2));
            chances.Add("7", Math.Max(0, 1 - Math.Pow(distance, 2) * 2) / 2);
            chances.Add("8", Math.Pow(distance, 2) / 2);
            chances.Add("9", Math.Max(0, 1 - Math.Pow(distance, 2) * 2));
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
            File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "monsters.json"), JsonConvert.SerializeObject(dict, Formatting.Indented));
            return dict;
        }

        private static string GetRandomMonsterSpawn(Vector2 p, Random r, string which = null)
        {
            var monsterDict = SHelper.GameContent.Load<Dictionary<string, MonsterSpawnInfo>>(monsterDictPath);

            Dictionary<string, double> chances = new();
            double total = 0;
            foreach (var m in monsterDict)
            {
                total += m.Value.Chance;
            }
            double roll = r.NextDouble();
            double count = 0;
            foreach (var m in monsterDict)
            {
                count += m.Value.Chance / total;
                if (roll < count)
                {
                    return m.Key;
                }
            }
            return null;
        }
        private static void AddMonstersToChunk(Point chunkPoint, WorldChunk chunk)
        {
            var monsterDict = SHelper.GameContent.Load<Dictionary<string, MonsterSpawnInfo>>(monsterDictPath);

            Random r = Utility.CreateRandom(RandomSeed, chunkPoint.X * chunkPoint.Y + chunkPoint.X, 142);

            if (!monsterCenters.TryGetValue(chunkPoint, out var mcs))
                return;
            foreach (var mc in mcs)
            {
                var mi = monsterDict[mc.Value];
                if (r.NextDouble() > mi.Chance)
                    continue;
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
                                    var av = v + chunkPoint.ToVector2() * openWorldChunkSize;
                                    chunk.monsters[av] = monsters[idx];
                                }
                                else
                                {
                                    int num = Config.OpenWorldSize / openWorldChunkSize;
                                    Point ocp = chunkPoint + offset;
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
                    TryLoadChunk(pc);
                    keep.Add(new(pc.X, pc.Y));
                    foreach (var v in Utility.getSurroundingTileLocationsArray(pc.ToVector2()))
                    {
                        var p = v.ToPoint();
                        TryLoadChunk(p);
                        keep.Add(p);
                    }
                }
            }
            foreach (var p in cachedChunks.Keys.ToArray())
            {
                if (!keep.Contains(p))
                {
                    UnloadChunk(p);
                }
            }
        }

        private static void UnloadChunk(Point p)
        {
            if(!cachedChunks.TryGetValue(p, out var chunk))
            {
                TryLoadChunk(p);
                chunk = cachedChunks[p];
                if (chunk == null)
                    return;
            }
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
                BuildWorldChunk(pc);
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
                if (IsOpenTile(chunk, t.Key))
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
                if (IsOpenTile(chunk, t.Key))
                {
                    openWorldLocation.objects[t.Key] = t.Value;
                }
            }
            foreach (var t in chunk.overlayObjects)
            {
                if (IsOpenTile(chunk, t.Key))
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
                if (IsOpenTile(chunk, t.Key))
                {
                    Monster m = null;
                    switch (t.Value.Type)
                    {
                        case "GreenSlime":
                            m = new GreenSlime(t.Key * 64, t.Value.Level);
                            m.modData[modKey] = $"{pc.X},{pc.Y}";
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
                            m.modData[modKey] = $"{pc.X},{pc.Y}";
                            break;
                        case "Dino":
                            m = new DinoMonster(t.Key * 64);
                            m.modData[modKey] = $"{pc.X},{pc.Y}";
                            break;
                    }
                    if (m is not null)
                    {
                        openWorldLocation.characters.Add(m);
                    }
                }
            }
        }

        public static void UpdateTreasuresList()
        {
            treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(Config.ItemListChances.Where(p => p.Value > 0).ToDictionary(s => s.Key, s => s.Value).Keys.ToArray(), Config.MinItemValue, Config.MaxItemValue);
        }
    }
}