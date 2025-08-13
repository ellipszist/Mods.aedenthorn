using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            if (!Config.ModEnabled || location != openWorldLocation || monster.Position.X < 0f || monster.Position.X > openWorldSize * 64 || monster.Position.Y < 0f || monster.Position.Y > openWorldSize * 64)
                npcs.Remove(monster);
        }
        public static bool CodesCompare(List<CodeInstruction> codes, int i, OpCode[] opCodes)
        {
            for(int j = 0; j < opCodes.Length; j++)
            {
                if (codes.Count <= i + j)
                    return false;
                if (codes[i + j].opcode != opCodes[j])
                    return false;
            }
            return true;
        }
        public static int GetGlobalCharacterInt(int value, Character character)
        {
            if (!Config.ModEnabled || character.currentLocation?.Name.Contains(locName) != true)
                return value;
            int v = value % (openWorldChunkSize * 64) + (Game1.viewport.Y / (openWorldChunkSize * 64) < value / (openWorldChunkSize * 64) ? openWorldChunkSize * 64 : 0);
            return v;
        }
        public static float GetGlobalCharacterFloat(float value, Character character)
        {
            if (!Config.ModEnabled || character.currentLocation?.Name.Contains(locName) != true)
                return value;
            float v = value % (openWorldChunkSize * 64) + (Game1.viewport.Y / (openWorldChunkSize * 64) < value / (openWorldChunkSize * 64) ? openWorldChunkSize * 64 : 0);
            return v;
        }
        public static int GetGlobalTreeInt(int value, Tree tree)
        {
            if (!Config.ModEnabled || tree.Location?.Name.Contains(locName) != true)
                return value;
            int v = value % (openWorldChunkSize * 64) + (Game1.viewport.Y / (openWorldChunkSize * 64) < value / (openWorldChunkSize * 64) ? openWorldChunkSize * 64 : 0);
            return v;
        }
        public static float GetGlobalTileFloat(float value, Tree tree)
        {
            if (!Config.ModEnabled || tree.Location?.Name.Contains(locName) != true)
                return value;
            float v = value % openWorldChunkSize + (Game1.viewport.Y / (openWorldChunkSize * 64) < (value * 64) / (openWorldChunkSize * 64) ? openWorldChunkSize : 0);
            return v;
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
            return Math.Max(0f, (float)((y % openWorldChunkSize + 1) * 64 + 2) / 10000f) + (float)x % openWorldChunkSize / 1000000f;
        }

        public void ResetChunkTiles()
        {
            int size = openWorldSize / openWorldChunkSize;
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

        public static void BuildWorldChunk(Point cp)
        {
            int size = openWorldSize / openWorldChunkSize;
            if (cp.X < 0 || cp.Y < 0 || cp.X >= size || cp.Y >= size)
                return;
            WorldChunk chunk;
            if (!cachedChunks.TryGetValue(cp, out chunk))
            {
                chunk = new();
                cachedChunks[cp] = chunk;
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
            s.Start();
            AddGrassToChunk(cp, chunk);
            //SMonitor.Log($"grass in {s.ElapsedMilliseconds}");
            s.Restart();
            AddTreesToChunk(cp, chunk);
            //SMonitor.Log($"trees in {s.ElapsedMilliseconds}");
            s.Restart();
            AddBiomesToChunk(cp, chunk);
            //SMonitor.Log($"Biomes in {s.ElapsedMilliseconds}");
            s.Restart();
            AddMonstersToChunk(cp, chunk);
            //SMonitor.Log($"Monsters {s.ElapsedMilliseconds}");
            s.Stop();
            cachedChunks[cp].initialized = true;
        }


        private static void AddGrassToChunk(Point point, WorldChunk chunk)
        {
            var layer = openWorldLocation.Map.GetLayer("Back");
            var backTiles = new Tile[openWorldChunkSize, openWorldChunkSize];
            Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, point.X * point.Y + point.X, 42);
            var mainSheet = openWorldLocation.Map.GetTileSheet("outdoors");
            for (int y = 0; y < openWorldChunkSize; y++)
            {
                for (int x = 0; x < openWorldChunkSize; x++)
                {
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
                    backTiles[x, y] = new StaticTile(layer, mainSheet, BlendMode.Alpha, idx);
                }
            }
            chunk.tiles[layer.Id] = backTiles;
        }

        public static void AddBiomesToChunk(Point point, WorldChunk chunk)
        {
            biomeDict = SHelper.GameContent.Load<Dictionary<string, Biome>>(biomeDictPath);

            var chunkBox = new Rectangle(point.X * openWorldChunkSize, point.Y * openWorldChunkSize, openWorldChunkSize, openWorldChunkSize);
            foreach (var b in biomeDict.Values)
            {
                var map = SHelper.GameContent.Load<Map>(b.MapPath);
                Rectangle mapBox = new(b.MapPosition, new(map.Layers[0].LayerWidth, map.Layers[0].LayerHeight));
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
                            var rp = new Point(b.MapPosition.X + x, b.MapPosition.Y + y);
                            openWorldLocation.terrainFeatures.Remove(rp.ToVector2());
                            if (l.Tiles[x, y] != null && chunkBox.Contains(rp))
                            {
                                var rx = b.MapPosition.X + x - chunkBox.X;
                                var ry = b.MapPosition.Y + y - chunkBox.Y;
                                var ot = l.Tiles[x, y];
                                var ots = ot.TileSheet;
                                var nts = openWorldLocation.Map.TileSheets.FirstOrDefault(s => s.ImageSource == ots.ImageSource);
                                if (nts == null)
                                {
                                    nts = new TileSheet(ots.Id, openWorldLocation.Map, ots.ImageSource, ots.SheetSize, l.TileSize);
                                    openWorldLocation.Map.AddTileSheet(nts);
                                }
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

        private static void AddTreesToChunk(Point chunkPoint, WorldChunk chunk)
        {
            Stopwatch s = new();
            s.Start();

            Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, chunkPoint.X * chunkPoint.Y + chunkPoint.X, 42);
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
                                    int num = openWorldSize / openWorldChunkSize;
                                    Point ocp = chunkPoint + offset;
                                    if(ocp.X >= 0 && ocp.X < num && ocp.Y >= 0 && ocp.Y < num)
                                    {
                                        var av = v + ocp.ToVector2() * openWorldChunkSize;
                                        if (!cachedChunks.ContainsKey(ocp))
                                            cachedChunks[ocp] = new();
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
            float distance = Math.Abs(p.Y - openWorldSize / 2) / (openWorldSize / 2);
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
                if(roll < count)
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
            Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, chunkPoint.X * chunkPoint.Y + chunkPoint.X, 142);

            if (!monsterCenters.TryGetValue(chunkPoint, out var mcs))
                return;
            foreach(var mc in mcs)
            {
                var mi = monsterDict[mc.Value];
                if (r.NextDouble() > mi.Chance)
                    continue;
                List<MonsterSpawn> monsters = new();
                foreach(var m in mi.Monsters)
                {
                    if (r.NextDouble() > m.Chance)
                        continue;
                    int amount = r.Next(m.Min, m.Max + 1);
                    if (amount == 0)
                        continue;
                    for(int i = 0; i < amount; i++)
                    {
                        monsters.Add(new() { Type = m.Type, Level = r.Next(m.MinLevel, m.MaxLevel + 1) });
                    }
                }
                if (!monsters.Any())
                    return;
                Point begin = new(0, 0);
                Point end = new(0, 0);
                int idx = 0;
                while(idx < monsters.Count)
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
                                if(offset.X == 0 && offset.Y == 0)
                                {
                                    var av = v + chunkPoint.ToVector2() * openWorldChunkSize;
                                    chunk.monsters[av] = monsters[idx];
                                }
                                else
                                {
                                    int num = openWorldSize / openWorldChunkSize;
                                    Point ocp = chunkPoint + offset;
                                    if (ocp.X >= 0 && ocp.X < num && ocp.Y >= 0 && ocp.Y < num)
                                    {
                                        var av = v + ocp.ToVector2() * openWorldChunkSize;
                                        if (!cachedChunks.ContainsKey(ocp))
                                            cachedChunks[ocp] = new();
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
            int size = openWorldSize / openWorldChunkSize;
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
            foreach(var p in cachedChunks.Keys.ToArray())
            {
                if (!keep.Contains(p))
                {
                    loadedChunks.Remove(p);

                    for(int i = openWorldLocation.characters.Count - 1; i >= 0; i--)
                    {
                        Character c = openWorldLocation.characters[i];
                        if (c.modData.TryGetValue(modKey, out var ps))
                        {
                            string[] sps = ps.Split(',');
                            if(new Point(int.Parse(sps[0]), int.Parse(sps[1])) == p)
                            {
                                openWorldLocation.characters.RemoveAt(i);
                            }
                        }
                    }
                    for (int y = 0; y < openWorldChunkSize; y++)
                    {
                        for (int x = 0; x < openWorldChunkSize; x++)
                        {
                            int ax = x + p.X * openWorldChunkSize;
                            int ay = y + p.Y * openWorldChunkSize;
                            openWorldLocation.terrainFeatures.Remove(new(ax, ay));
                            openWorldLocation.Objects.Remove(new(ax, ay));

                        }
                    }
                }
            }
        }

        private static void TryLoadChunk(Point pc)
        {
            if (!loadedChunks.Contains(pc))
            {
                loadedChunks.Add(pc);
                BuildWorldChunk(pc);
                AddTreesFromChunk(pc);
                AddMonstersFromChunk(pc);
            }
        }

        private static void AddTreesFromChunk(Point pc)
        {
            var chunk = cachedChunks[pc];
            var grass = new List<int>() { 351, 304, 305, 300 };
            foreach (var t in chunk.terrainFeatures)
            {
                if (IsOpenTile(chunk, t.Key))
                {
                    openWorldLocation.terrainFeatures[t.Key] = t.Value;
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
                            if(mineArea < 121 && mineArea > 39)
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
                    if(m is not null)
                    {
                        openWorldLocation.characters.Add(m);
                    }
                }
            }
        }
        public static bool IsOpenTile(WorldChunk chunk, Vector2 t)
        {
            Tile? tile = chunk.tiles["Back"][(int)t.X % openWorldChunkSize, (int)t.Y % openWorldChunkSize];
            return tile is not null && grassTiles.Contains(tile.TileIndex) && !openWorldLocation.terrainFeatures.ContainsKey(t) && !openWorldLocation.Objects.ContainsKey(t);
        }

        private static Point GetPlayerChunk(Farmer f)
        {
            return new Point(f.TilePoint.X / openWorldChunkSize, f.TilePoint.Y / openWorldChunkSize);
        }

        public static WorldChunk CreateChunk(int cx, int cy)
        {
            WorldChunk outchunk = new WorldChunk();
            List<WorldChunk> chunks = new List<WorldChunk>();
            foreach (var biome in biomeCodeDict) 
            {
                var chunk = biome.Value.Invoke(Game1.uniqueIDForThisGame, cx, cy);
                if(chunk != null)
                    chunks.Add(chunk);
            }
            if (!chunks.Any())
                return null;
            chunks.Sort(delegate (WorldChunk a, WorldChunk b) { return a.priority.CompareTo(b.priority); });
            foreach(var chunk in chunks)
            {
                foreach (var kvp in chunk.objects)
                    outchunk.objects[kvp.Key] = kvp.Value;
                foreach (var kvp in chunk.terrainFeatures)
                    outchunk.terrainFeatures[kvp.Key] = kvp.Value;
                foreach (var kvp in chunk.tiles)
                {
                    for(int x = 0; x < kvp.Value.GetLength(0); x++)
                    {
                        for (int y = 0; y < kvp.Value.GetLength(1); y++)
                        {
                            if (kvp.Value[x,y] != null)
                            {
                                outchunk.tiles[kvp.Key][x, y] = kvp.Value[x, y];
                            }
                        }
                    }
                }
            }
            return outchunk;
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