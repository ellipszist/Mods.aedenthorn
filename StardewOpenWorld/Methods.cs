using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {
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

        private static async void PreloadWorldChunk(int cx, int cy)
        {
            int size = openWorldSize / openWorldChunkSize;
            if (cx < 0 || cy < 0 || cx >= size || cy >= size)
                return;
            if (cachedChunks is null)
            {
                int l = openWorldSize / openWorldChunkSize;
                cachedChunks = new();
            }
            var point = new Point(cx, cy);
            if (cachedChunks.ContainsKey(point))
            {
                return;
            }
            var chunk = new WorldChunk();
            cachedChunks[point] = chunk;
            biomeDict = SHelper.GameContent.Load<Dictionary<string, Biome>>(dictPath);
        //    await Task.Factory.StartNew(() => PreloadWorldChunkAsync(point, chunk));
        //}

        //private static void PreloadWorldChunkAsync(Point point, WorldChunk chunk)
        //{
            foreach (var layer in openWorldLocation.Map.Layers)
            {
                if (layer.Id == "Back")
                {
                    var backTiles = new Tile[openWorldChunkSize, openWorldChunkSize];
                    Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, point.X * point.Y + point.X);
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
            }
            var chunkBox = new Rectangle(point.X * openWorldChunkSize, point.Y * openWorldChunkSize, openWorldChunkSize, openWorldChunkSize);
            foreach(var b in biomeDict.Values)
            {
                var map = SHelper.GameContent.Load<Map>(b.MapPath);
                Rectangle mapBox = new(b.MapPosition, new(map.Layers[0].LayerWidth, map.Layers[0].LayerHeight));
                if (!chunkBox.Intersects(mapBox))
                    continue;
                foreach(var l in map.Layers)
                {
                    var nl = openWorldLocation.Map.GetLayer(l.Id);
                    if (nl == null)
                    {
                        nl = new Layer(l.Id, openWorldLocation.Map, new xTile.Dimensions.Size(openWorldChunkSize, openWorldChunkSize), l.TileSize);
                        openWorldLocation.Map.AddLayer(nl);
                        openWorldLocation.SortLayers();
                    }
                    if(!chunk.tiles.TryGetValue(l.Id, out var tiles))
                    {
                        tiles = new Tile[openWorldChunkSize, openWorldChunkSize];
                        chunk.tiles[l.Id] = tiles;
                    }
                    for(int x = 0; x < l.Tiles.Array.GetLength(0); x++)
                    {
                        for (int y = 0; y < l.Tiles.Array.GetLength(1); y++)
                        {
                            var rp = new Point(b.MapPosition.X + x, b.MapPosition.Y + y);
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

        public static void PlayerTileChanged()
        {
            int size = openWorldSize / openWorldChunkSize;
            List<Point> keep = new();
            foreach (var f in Game1.getAllFarmers())
            {
                if (f.currentLocation.Name.Contains(locName))
                {
                    var pc = GetPlayerChunk(f);
                    PreloadWorldChunk(pc.X, pc.Y);
                    keep.Add(new(pc.X, pc.Y));
                    foreach (var p in Utility.getSurroundingTileLocationsArray(pc.ToVector2()))
                    {
                        PreloadWorldChunk((int)p.X, (int)p.Y);
                        keep.Add(new((int)p.X, (int)p.Y));
                    }
                }
            }
            foreach(var p in cachedChunks.Keys.ToArray())
            {
                if (!keep.Contains(p))
                {
                    cachedChunks.Remove(p);
                }
            }
        }


        private static Point GetPlayerChunk(Farmer f)
        {
            return new Point(f.TilePoint.X / openWorldChunkSize, f.TilePoint.Y / openWorldChunkSize);
        }

        public static WorldChunk CreateChunk(int cx, int cy)
        {
            WorldChunk outchunk = new WorldChunk();
            List<WorldChunk> chunks = new List<WorldChunk>();
            foreach (var biome in biomes) 
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