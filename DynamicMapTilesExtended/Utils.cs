﻿using DMT.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewValley;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace DMT
{
    public static class Utils
    {
        private static bool Enabled => Context.Config.Enabled;

        public static bool CheckTrigger(DynamicTileProperty property, IEnumerable<string> triggers)
        {
            if (property.Trigger == null)
                return false;
            foreach (var trigger in triggers)
            {
                if (!trigger.Equals(property.Trigger, StringComparison.OrdinalIgnoreCase) && !trigger.StartsWith(property.Trigger + "(", StringComparison.OrdinalIgnoreCase))
                    continue;
                return true;
            }
            return false;
        }

        public static Point GetNextTile(int dir)
        {
            return dir switch
            {
                0 => new(0, -1),
                1 => new(1, 0),
                2 => new(0, 1),
                3 => new(-1, 0),
                _ => new(0) //This will never be hit, but I don't like my IDE complaining with colorfull squiqly lines
            };
        }

        public static bool PushTiles(GameLocation l, List<(Point p, Tile t)> tiles, int dir, Farmer f)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                Point destination = tiles[i].p + GetNextTile(dir);
                if (!l.isTileOnMap(tiles[i].p) || !l.isTileOnMap(destination) || 
                    (
                        tiles[i].t.Layer.Tiles[destination.X, destination.Y] is Tile t &&
                        (t.HasProperty(Keys.PushKey, out _) || t.HasProperty(Keys.PushableKey, out _)) &&
                        !tiles.Any(x => x.p == destination && x.t.Layer.Id == tiles[i].t.Layer.Id)
                    ))
                    return false;
            }
            if (!Context.PushTileDict.TryGetValue(l, out var pushed))
                Context.PushTileDict[l] = pushed = [];
            for (int i = 0; i < tiles.Count; i++)
            {
                Point destination = tiles[i].p + GetNextTile(dir);
                var (p, t) = tiles[i];
                if (t.Layer.Id == "Buildings")
                    TriggerActions([t.Layer], f, l, p, ["Push"]);
                pushed.Add(new() { Tile = t, Position = new(p.X * 64, p.Y * 64), Origin = p, Direction = dir, Farmer = f, Destination = destination });
                tiles[i].t.Layer.Tiles[p.X, p.Y] = null;
            }
            return true;
        }

        public static void PushTilesWithOthers(Farmer f, Tile t, Point start)
        {
            List<(Point, Tile)> tiles = [(start, t)];
            if (!t.HasProperty(Keys.PushAlsoKey, out var others))
            {
                _ = PushTiles(f.currentLocation, tiles, f.FacingDirection, f);
                return;
            }
            var split = others.Split(',');
            foreach (var tile in split)
            {
                try
                {
                    var other = tile.Split(' ');
                    if (split.Length != 3 || !int.TryParse(other[1], out int x) || !int.TryParse(other[2], out int y))
                        continue;
                    x += start.X;
                    y += start.Y;
                    var layer = f.currentLocation.Map.GetLayer(other[0]);
                    if (layer is null || !f.currentLocation.isTileOnMap(x, y) || layer.Tiles[x, y] is not Tile otherTile)
                        continue;
                    tiles.Add((new(x, y), otherTile));
                }
                catch { }
            }
            PushTiles(f.currentLocation, tiles, f.FacingDirection, f);
        }

        internal static Layer AddLayer(Map map, string id)
        {
            var layer = new Layer(id, map, map.Layers[0].LayerSize, Layer.m_tileSize);
            map.AddLayer(layer);
            return layer;
        }

        internal static TileSheet AddTileSheet(Map map, string id, string texturePath)
        {
            var texture = Context.Helper.GameContent.Load<Texture2D>(texturePath);
            if (texture == null)
                return null;
            var tilesheet = new TileSheet(id, map, texturePath, new xSize(texture.Width / 16, texture.Height / 16), new xSize(16, 16));
            map.AddTileSheet(tilesheet);
            return tilesheet;
        }

        internal static Item? ParseItemString(string value)
        {
            Item item = null;
            string[] split = [];

            try
            {
                item = ItemRegistry.Create(value, 1, 0, true) ?? throw new();
            }
            catch
            {
                int amount = 1;
                int quality = 0;
                if (value.Contains(','))
                {
                    split = value.Split(',');
                    value = split[0];
                    amount = int.Parse(split[1]);
                    if (split.Length == 3)
                        quality = int.Parse(split[2]);
                }
                item = ItemRegistry.Create(value, amount, quality);
            }

            return item;
        }

        internal static DynamicTileProperty? GetInternalProperty(string data)
        {
            return JsonConvert.DeserializeObject<DynamicTileProperty>(data);
        }

        internal static DynamicTileProperty? ParseProperty(KeyValuePair<string, string> kvp, IOrderedEnumerable<string> keys)
        {
            if (!kvp.Key.StartsWith(ModPrefix))
                return null;
            DynamicTileProperty prop = new() { Value = kvp.Value };
            string key = kvp.Key;
            foreach (var item in keys)
            {
                if (!key.StartsWith(item, StringComparison.OrdinalIgnoreCase))
                    continue; 
                prop.Key = item;
                key = key.Substring(item.Length);
                break;
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }
            key = key.Trim('_');
            if (key.StartsWith("Once", StringComparison.OrdinalIgnoreCase))
            {
                prop.Once = true;
                key = key[4..];
            }
            key = key.Trim('_');
            if (!string.IsNullOrWhiteSpace(key))
            {
                foreach (var item in Triggers.Regexes)
                {
                    if (!Regex.IsMatch(key, item))
                        continue;
                    prop.Trigger = key;
                    break;
                }
            }
            return prop;
        }

        internal static List<DynamicTileProperty> FilterProperties(List<DynamicTileProperty> properties)
        {
            List<string> keys = [];
            List<DynamicTileProperty> filtered = [];
            foreach (var prop in properties)
            {
                if (keys.Contains(prop.Key))
                {
                    Context.Monitor.Log($"Found duplicate key {prop.Key}{(string.IsNullOrWhiteSpace(prop.LogName) ? "" : $" For named property {prop.LogName}")}, This property was automatically filtered out. Please avoid adding duplicate actions for tiles", LogLevel.Warn);
                    continue;
                }
                keys.Add(prop.Key);
                filtered.Add(prop);
            }

            return filtered;
        }

        internal static string BuildFormattedTrigger(params object[]? parts)
        {
            if (!(parts?.Any() ?? false))
                return "";

            StringBuilder sb = new();
            sb.Append('(');
            foreach (var part in parts)
                sb.Append(part);
            sb.Append(')');
            return sb.ToString();
        }

        internal static bool IsGlobalTrigger(string trigger)
        {
            foreach (var item in Triggers.GlobalTriggers)
                if (Regex.IsMatch(trigger, item))
                    return true;

            return false;
        }

        /*
        Riding a mount as a trigger 
        Crop is grown as a trigger 
        Placing and/or clicking on a specific big craftable/furniture/flute block tone
        */

        //All of the methods below this line cost me a lot (mainly sanity wise), continue at your own risk

        public static bool TriggerActions(List<Layer> layers, Farmer? who, GameLocation location, Point tilePosition, string[] triggers)
        {
            if (!Enabled || !location.isTileOnMap(tilePosition) || (!Context.Config.TriggerDuringEvents && Game1.eventUp))
                return false;

            List<(DynamicTileProperty prop, Tile tile)> properties = GetPropertiesForTriggers(layers, who, tilePosition, triggers);
            /*foreach (var layer in layers)
            {
                var tile = layer.Tiles[tilePosition.X, tilePosition.Y];

                if (tile is null)
                    continue;
                string[] props = [.. tile.Properties.Keys];

                foreach (var key in props)
                {
                    DynamicTileProperty? prop = !key.StartsWith("DMT_PROP_INTERNAL") ? ParseOldProperty(new(key, tile.Properties[key])) : GetInternalProperty(tile.Properties[key]);

                    if (prop is null || !CheckTrigger(prop, triggers))
                        continue;

                    if (prop.Once)
                        tile.Properties.Remove(key);

                    properties.Add(new(prop, tile));
                }
            }*/
            if (properties.Count == 0)
            {
                return false;
            }
            return DoTriggerActions(who, tilePosition, properties);
        }
        public static bool DoTriggerActions(Farmer who, Point tilePosition, List<(DynamicTileProperty prop, Tile tile)> properties)
        {
            List<string> triggered = new();

            foreach (var item in properties)
            {
                bool found = true;
                try
                {
                    var value = item.prop.Value;
                    var tile = item.tile;
                    switch (item.prop.Key)
                    {
                        case Keys.AddLayerKey:
                            Actions.DoAddLayer(who, value);
                            break;
                        case Keys.AddTilesheetKey:
                            Actions.DoAddTileSheet(who, value);
                            break;
                        case Keys.AnimationKey:
                            Actions.DoAnimate(who, value, false);
                            break;
                        case Keys.AnimationOffKey:
                            Actions.DoAnimate(who, value, true);
                            break;
                        case Keys.BuffKey:
                            Actions.DoAddBuff(who, value);
                            break;
                        case Keys.ChangeIndexKey:
                            Actions.DoChangeIndex(who, value, tile, tilePosition);
                            break;
                        case Keys.ChangeMultipleIndexKey:
                            Actions.DoChangeMultipleIndexes(who, value, tile, tilePosition);
                            break;
                        case Keys.ChangePropertiesKey:
                            Actions.DoChangeProperties(value, tile);
                            break;
                        case Keys.ChangeMultiplePropertiesKey:
                            Actions.DoChangeMultipleProperties(who, value, tile);
                            break;
                        case Keys.ChestKey:
                            Actions.DoSpawnChest(who, value);
                            break;
                        case Keys.EmoteKey:
                            Actions.DoEmote(who, value);
                            break;
                        case Keys.EventKey:
                            Actions.DoPlayEvent(value);
                            break;
                        case Keys.ExplosionKey:
                            Actions.DoExplode(who, value, tilePosition);
                            break;
                        case Keys.GiveKey:
                            Actions.DoGive(who, value);
                            break;
                        case Keys.HealthKey:
                            Actions.DoUpdateHealth(who, value);
                            break;
                        case Keys.HealthPerSecondKey:
                            Actions.DoUpdateHealthPerSecond(who, value);
                            break;
                        case Keys.HealthPerSecondContKey:
                            Actions.DoUpdateHealthPerSecondCont(who, value);
                            break;
                        case Keys.InvalidateKey:
                            Actions.DoInvalidateAsset(value);
                            break;
                        case Keys.MailBoxKey:
                            Actions.DoAddMailForTomorrow(who, value);
                            break;
                        case Keys.MailKey:
                            Actions.DoAddMailflag(who, value);
                            break;
                        case Keys.MailRemoveKey:
                            Actions.DoRemoveMailflag(who, value);
                            break;
                        case Keys.MessageKey:
                            Actions.DoShowMessage(value);
                            break;
                        case Keys.MusicKey:
                            Actions.DoPlayMusic(value);
                            break;
                        case Keys.PushKey:
                            Actions.DoPushTiles(who, tile, tilePosition);
                            break;
                        case Keys.PushOthersKey:
                            Actions.DoPushOtherTiles(who, value, tile, tilePosition);
                            break;
                        case Keys.StaminaKey:
                            Actions.DoUpdateStamina(who, value);
                            break;
                        case Keys.StaminaPerSecondKey:
                            Actions.DoUpdateStaminaPerSecond(who, value);
                            break;
                        case Keys.StaminaPerSecondContKey:
                            Actions.DoUpdateStaminaPerSecondCont(who, value);
                            break;
                        case Keys.SoundKey:
                            Actions.DoPlaySound(who, value);
                            break;
                        case Keys.TakeKey:
                            Actions.DoTake(who, value);
                            break;
                        case Keys.TeleportKey:
                            Actions.DoTeleport(who, value);
                            break;
                        case Keys.TeleportTileKey:
                            Actions.DoTeleportTile(who, value);
                            break;
                        case Keys.WarpKey:
                            Actions.DoWarp(who, value);
                            break;
                        default:
                            if (Keys.ModKeys.Contains(item.prop.key))
                            {
                                Actions.ModActions[item.prop.Key]?.Invoke(who, value, tile, tilePosition);
                            }
                            else
                                found = false;
                            break;
                    }
                    if (found)
                        triggered.Add(item.prop.key);
                }
                catch (Exception ex)
                {
                    Context.Monitor.Log($"Error while trying to run action {item.prop.Key}", LogLevel.Error);
                    Context.Monitor.Log($"[{ex.GetType().Name}] {ex.Message}\n{ex.Message}", LogLevel.Error);
                }
            }
            if (triggered.Any())
            {
                //Context.Monitor.Log($"Triggered at {tilePosition} by {who?.displayName ?? "no one"}: {string.Join(',', triggered)}");
                return true;
            }
            return false;
        }

        internal static List<(DynamicTileProperty prop, Tile tile)> GetPropertiesForTriggers(List<Layer> layers, Farmer who, Point tilePosition, IEnumerable<string> triggers)
        {
            List<string> localTriggers = [];
            List<string> globalTriggers = [];
            List<(DynamicTileProperty prop, Tile tile)> properties = [];
            IOrderedEnumerable<string> allKeys = (IOrderedEnumerable<string>)Keys.AllKeys.Union(Keys.ModKeys).OrderByDescending(x => x.Length);
            foreach (var trigger in (string[])[.. triggers])
            {
                if (IsGlobalTrigger(trigger))
                    globalTriggers.Add(trigger);
                else
                    localTriggers.Add(trigger);
            }

            if (globalTriggers.Count > 0)
                properties.AddRange(GetPropertiesForGlobalTriggers(layers, globalTriggers, allKeys));
            if (localTriggers.Count > 0)
                properties.AddRange(GetPropertiesForLocalTriggers(layers, tilePosition, localTriggers, allKeys));

            return properties;
        }

        internal static List<(DynamicTileProperty prop, Tile tile)> GetPropertiesForLocalTriggers(List<Layer> layers, Point tilePosition, IEnumerable<string> triggers, IOrderedEnumerable<string> allKeys)
        {
            List<(DynamicTileProperty prop, Tile tile)> properties = [];
            foreach (var layer in layers)
            {
                var tile = layer.Tiles[tilePosition.X, tilePosition.Y];

                if (tile is null)
                    continue;
                string[] props = [.. tile.Properties.Keys];

                foreach (var key in props)
                {
                    DynamicTileProperty? prop = ParseProperty(new(key, tile.Properties[key]), allKeys);

                    if (prop is null || !CheckTrigger(prop, triggers))
                        continue;

                    if (prop.Once)
                        tile.Properties.Remove(key);

                    properties.Add(new(prop, tile));
                }
            }
            return properties;
        }

        internal static List<(DynamicTileProperty prop, Tile tile)> GetPropertiesForGlobalTriggers(List<Layer> layers, IEnumerable<string> triggers, IOrderedEnumerable<string> allKeys)
        {
            List<(DynamicTileProperty prop, Tile tile)> properties = [];
            int width = layers[0].Tiles.Array.GetLength(0);
            int height = layers[0].Tiles.Array.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    foreach (var layer in layers)
                    {
                        var tile = layer.Tiles[x, y];

                        if (tile is null)
                            continue;
                        foreach (var key in tile.Properties.Keys)
                        {
                            DynamicTileProperty? prop = ParseProperty(new(key, tile.Properties[key]), allKeys);

                            if (prop is null || !CheckTrigger(prop, triggers))
                                continue;

                            if (prop.Once)
                                tile.Properties.Remove(key);

                            properties.Add(new(prop, tile));
                        }
                    }
                }
            }
            return properties;
        }

        /// <summary>
        /// Load the tile properties to a location from the dictionary asset
        /// </summary>
        /// <param name="l">The location to load properties to</param>
        internal static void LoadLocation(GameLocation l)
        {
            Stopwatch s = new();
            s.Start();
            if (!Context.Config.Enabled)
                return;
            var dict = Context.Helper.GameContent.Load<Dictionary<string, DynamicTile>>(Context.TileDataDictPath);
            var keys = dict.Keys.ToList();
            for(int i = keys.Count - 1; i >= 0 ; i--)
            {
                if (!IsValidLocation(dict[keys[i]], l))
                {
                    keys.RemoveAt(i);
                }
            }

            foreach (var layer in l.Map.Layers)
            {
                for (int i = keys.Count - 1; i >= 0; i--)
                {
                    if (!(dict[keys[i]].Layers?.Contains(layer.Id) ?? true))
                    {
                        keys.RemoveAt(i);
                    }
                }
                int width = layer.Tiles.Array.GetLength(0);
                int height = layer.Tiles.Array.GetLength(1);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var tile = layer.Tiles[x, y];
                        if (tile is null)
                            continue;
                        foreach (var key in keys)
                        {
                            int count = 0;
                            int count2 = 0;
                            var value = dict[key];

                            Point tilePoint = new(x, y);
                            if (!IsValidTile(value, tile, tilePoint))
                                continue;
                            count++;
                            foreach (var prop in value.Properties)
                            {
                                layer.Tiles[x, y].Properties[prop.Key] = prop.Value;
                            }
                            foreach (var action in value.Actions)
                            {
                                if (action.trigger == Triggers.Load)
                                {
                                    DoTriggerActions(Game1.player, new(x, y), [(action, tile)]);
                                }
                                else
                                {
                                    ++count2;
                                    layer.Tiles[x, y].Properties[CreatePropertyKey(action)] = action.Value;
                                }
                            }
                        }
                    }
                }
            }
            Context.Monitor.Log($"Load location {l.DisplayName} took {s.ElapsedMilliseconds}ms");
        }

        internal static bool HasProperty(this Tile t, string key, [NotNullWhen(true)] out string? propertyValue)
        {
            propertyValue = null;
            if (t is null || t.Properties is null || t.Properties.Count == 0)
                return false;
            if (t.Properties.TryGetValue(key, out var prop))
            {
                propertyValue = prop;
                return true;
            }

            return false;
        }
        internal static string CreatePropertyKey(DynamicTileProperty action)
        {
            return $"{action.Key}{(action.Once ? "_Once" : "")}{(action.Trigger != null ? "_" + action.Trigger : "")}";
        }

        internal static bool IsValidLocation(DynamicTile tileData, GameLocation l)
        {
            var tileSheets = l.Map.TileSheets.ToList();
            if (!(tileData.Properties?.Any() ?? false) && !(tileData.Actions?.Any() ?? false))
                return false;

            if (tileData.Locations is not null && 
                tileData.Locations.Count > 0 && 
                !tileData.Locations.Contains(l.Name))
                return false;

            if (tileData.TileSheets is not null && 
                tileData.TileSheets.Count > 0 && 
                !tileData.TileSheets.Exists(x => tileSheets.Exists(y => y.Id == x)))
                return false;

            if (tileData.TileSheetsPaths is not null && 
                tileData.TileSheetsPaths.Count > 0 && 
                !tileData.TileSheetsPaths.Exists(x => tileSheets.Exists(y => y.ImageSource.Contains(x))))
                return false;

            return true;
        }

        internal static bool IsValidTile(DynamicTile tileData, Tile tile, Point tilePoint)
        {
            if (tileData.TileSheets is not null && tileData.TileSheets.Count > 0 && !tileData.TileSheets.Contains(tile.TileSheet.Id))
                return false;

            if (tileData.TileSheetsPaths is not null && tileData.TileSheetsPaths.Count > 0 && !tileData.TileSheetsPaths.Exists(tile.TileSheet.ImageSource.Contains))
                return false;

            if (tileData.Indexes is not null && tileData.Indexes.Count > 0 && !tileData.Indexes.Contains(tile.TileIndex))
                return false;

            if (tileData.Rectangles is not null && tileData.Rectangles.Count > 0 && !tileData.Rectangles.Exists(x => x.Contains(tilePoint)))
                return false;

            if (tileData.Tiles is not null && tileData.Tiles.Count > 0 && !tileData.Tiles.Contains(new(tilePoint.X, tilePoint.Y)))
                return false;

            return true;
        }
    }
}
