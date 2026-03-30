
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace ImmersiveSprinklersAndScarecrows
{
    public partial class ModEntry
    {

        public static Object GetSprinklerCached(GameLocation l, int x, int y)
        {
            if (!TryGetData(l, sprinklerGuidKey, x, y, out var guid))
            {
                guid = Guid.NewGuid().ToString();
                l.modData[$"{sprinklerGuidKey},{x},{y}"] = guid;
            }
            if (!sprinklerDict.TryGetValue(guid, out var obj))
            {
                obj = GetSprinkler(l, x, y);
            }
            return obj;
        }

        public static Object GetScarecrowCached(GameLocation l, int x, int y)
        {
            if (!TryGetData(l, scarecrowGuidKey, x, y, out var guid))
            {
                guid = Guid.NewGuid().ToString();
                l.modData[$"{scarecrowGuidKey},{x},{y}"] = guid;
            }
            if (!scarecrowDict.TryGetValue(guid, out var obj))
            {
                obj = GetScarecrow(l, x, y);
            }
            return obj;
        }
        public static Object GetSprinkler(GameLocation l, int x, int y)
        {
            if (!TryGetData(l, sprinklerKey, x, y, out var sprinklerName))
                return null;
            Object obj = null;
            if (HasData(l, sprinklerBigCraftableKey, x, y))
            {
                if (Game1.bigCraftableData.TryGetValue(sprinklerName, out var data))
                {
                    obj = new Object(Vector2.Zero, sprinklerName);
                }
            }
            else if (Game1.objectData.TryGetValue(sprinklerName, out var data))
            {
                obj = new Object(sprinklerName, 1);
            }
            if (obj != null)
            {
                obj.TileLocation = new Vector2(x, y);
                if (HasData(l, nozzleKey, x, y))
                {
                    obj.heldObject.Value = new Object("915", 1);
                }
                if (atApi is not null)
                {
                    foreach (var kvp2 in l.modData.Pairs)
                    {
                        if (kvp2.Key.StartsWith(altTextureSprinklerPrefix) && kvp2.Key.EndsWith($",{x},{y}"))
                        {
                            var key = kvp2.Key.Substring(sprinklerPrefix.Length).Split(',')[0];
                            obj.modData[key] = kvp2.Value;
                        }
                    }
                }
                if (!TryGetData(l, sprinklerGuidKey, x, y, out var guid))
                {
                    guid = Guid.NewGuid().ToString();
                    l.modData[$"{sprinklerGuidKey},{x},{y}"] = guid;
                }
                sprinklerDict[guid] = obj;
            }
            return obj;
        }
        public static Object GetScarecrow(GameLocation l, int x, int y)
        {
            if (!TryGetData(l, scarecrowKey, x, y, out var name))
                return null;
            Object obj = null;
            if (HasData(l, scarecrowBigCraftableKey, x, y))
            {
                if (Game1.bigCraftableData.TryGetValue(name, out var data))
                {
                    obj = new Object(Vector2.Zero, name);
                }
            }
            else if (Game1.objectData.TryGetValue(name, out var data))
            {
                obj = new Object(name, 1);
            }
            if (obj != null)
            {
                obj.TileLocation = new Vector2(x, y);
                if (atApi is not null)
                {
                    foreach (var kvp2 in l.modData.Pairs)
                    {
                        if (kvp2.Key.StartsWith(altTextureScarecrowPrefix) && kvp2.Key.EndsWith($",{x},{y}"))
                        {
                            var key = kvp2.Key.Substring(scarecrowPrefix.Length).Split(',')[0];
                            obj.modData[key] = kvp2.Value;
                        }
                    }
                }
                if (!TryGetData(l, scarecrowGuidKey, x, y, out var guid))
                {
                    guid = Guid.NewGuid().ToString();
                    SetData(l, scarecrowGuidKey, x, y, guid);
                }
                scarecrowDict[guid] = obj;
            }
            return obj;
        }

        public static Point GetMouseCornerTile()
        {
            var tile = Game1.currentCursorTile.ToPoint();
            var x = Game1.getMouseX() + Game1.viewport.X;
            var y = Game1.getMouseY() + Game1.viewport.Y;
            if (x % 64 < 32)
            {
                if (y % 64 < 32)
                {
                    return tile + new Point(-1, -1);
                }
                else
                {
                    return tile + new Point(-1, 0);
                }
            }
            else
            {
                if (y % 64 < 32)
                {
                    return tile + new Point(0, -1);
                }
                else
                {
                    return tile + new Point(0, 0);
                }
            }
        }
        public static Object GetSprinklerAtMouse()
        {
            if (Game1.currentLocation == null)
                return null;

            Point tile = GetMouseCornerTile();

            return GetSprinkler(Game1.currentLocation, tile.X, tile.Y);
        }
        public static Object GetScarecrowAtMouse()
        {
            if (Game1.currentLocation == null)
                return null;

            Point tile = GetMouseCornerTile();

            return GetScarecrow(Game1.currentLocation, tile.X, tile.Y);
        }

        public static bool ReturnOrDropSprinkler(GameLocation l, int x, int y, Farmer who, bool drop)
        {
            var pos = new Vector2(x, y) * 64;
            Object sprinkler = sprinkler = GetSprinkler(l, x, y);
            if (sprinkler != null)
            {
                if (drop)
                    l.debris.Add(new Debris(sprinkler, pos));
                else
                    TryReturnObject(sprinkler, who);
                SetData(l,sprinklerKey,x,y, null);
                SetData(l, sprinklerGuidKey, x,y, null);
                SetData(l, sprinklerBigCraftableKey, x,y, null);
                if (HasData(l, enricherKey, x, y))
                {
                    SetData(l, enricherKey, x, y, null);
                    var e = new Object("913", 1);
                    if (drop)
                        l.debris.Add(new Debris(e, pos));
                    else
                        TryReturnObject(e, who);
                }
                if (HasData(l, nozzleKey, x, y))
                {
                    SetData(l, nozzleKey, x, y, null);
                    var e = new Object("915", 1);
                    if (drop)
                        l.debris.Add(new Debris(e, pos));
                    else
                        TryReturnObject(e, who);

                }
                if (TryGetData(l, fertilizerKey, x, y, out var fertString))
                {
                    SetData(l, fertilizerKey, x, y, null);
                    Object e = GetFertilizer(fertString);
                    if (drop)
                        l.debris.Add(new Debris(e, pos));
                    else
                        TryReturnObject(e, who);
                }
                SMonitor.Log($"{(drop ? "Dropped" : "Returned")} {sprinkler?.Name}");
                return true;
            }
            return false;
        }

        public static bool ReturnOrDropScarecrow(GameLocation l, int x, int y, Farmer who, bool drop)
        {
            var pos = new Vector2(x, y) * 64;
            Object scarecrow = GetScarecrow(l, x, y);
            if (scarecrow != null)
            {
                if (drop)
                    l.debris.Add(new Debris(scarecrow, pos));
                else
                    TryReturnObject(scarecrow, who);
                SetData(l, scarecrowKey, x, y, null);
                SetData(l, scarecrowGuidKey, x, y, null);
                SetData(l, scarecrowBigCraftableKey, x, y, null);
                SMonitor.Log($"{(drop ? "Dropped" : "Returned")} {scarecrow?.Name}");
                return true;
            }
            return false;
        }

        public static void TryReturnObject(Object obj, Farmer who)
        {
            if (obj is null)
                return;
            SMonitor.Log($"Trying to return {obj.Name}");
            if (!who.addItemToInventoryBool(obj))
            {
                who.currentLocation.debris.Add(new Debris(obj, who.Position));
            }
        }

        public static Object GetFertilizer(string fertString)
        {
            var fertData = fertString.Split(',');
            return new Object(fertData[0], int.Parse(fertData[1]));
        }
        public static int GetSprinklerRadius(Object obj)
        {
            if (!Config.SprinklerRadii.TryGetValue(obj.ItemId, out int radius) && !Config.SprinklerRadii.TryGetValue(obj.Name, out radius))
                return obj.GetModifiedRadiusForSprinkler();
            if (obj.heldObject.Value != null && Utility.IsNormalObjectAtParentSheetIndex((Object)obj.heldObject.Value, "915"))
            {
                radius++;
            }
            return radius;
        }

        public static List<Vector2> GetSprinklerTiles(Vector2 tileLocation, int radius)
        {

            Vector2 start = tileLocation + new Vector2(-1, -1) * radius;
            List<Vector2> list = new();
            var diameter = (radius + 1) * 2;
            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    list.Add(start + new Vector2(x, y));
                }
            }
            return list;

        }
        public static List<Vector2> GetScarecrowTiles(Vector2 tileLocation, int radius)
        {
            Vector2 start = tileLocation + new Vector2(-1, -1) * (radius - 2);
            Vector2 position = tileLocation + new Vector2(0.5f, 0.5f);
            List<Vector2> list = new();
            var diameter = (radius - 1) * 2;
            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    Vector2 tile = start + new Vector2(x, y);
                    if ((int)Math.Ceiling(Vector2.Distance(position, tile)) <= radius)
                        list.Add(tile);
                }
            }
            return list;

        }
        public static bool IsScarecrowInRange(bool scarecrow, Farm f, Vector2 v)
        {
            if (!Config.EnableMod || scarecrow)
                return scarecrow;
            foreach (var p in GetScarecrowPoints(f))
            {
                var obj = GetScarecrowCached(f, p.X, p.Y);
                if (obj is not null)
                {
                    var tiles = GetScarecrowTiles(p.ToVector2(), obj.GetRadiusForScarecrow());
                    if (tiles.Contains(v))
                    {
                        int scared = 0;
                        if(TryGetData(f, scaredKey, p.X, p.Y, out var scaredString))
                        {
                            int.TryParse(scaredString, out scared);
                        }
                        scared++;
                        SetData(f, scaredKey, p.X, p.Y, scared.ToString());
                        SMonitor.Log($"Scarecrow at {p} has scared {scared} crows");
                        return true;
                    }
                }
            }
            foreach (var p in GetSprinklerPoints(f))
            {
                var obj = GetSprinklerCached(f, p.X, p.Y);
                if (obj is not null && obj.IsScarecrow())
                {
                    var tiles = GetScarecrowTiles(p.ToVector2(), obj.GetRadiusForScarecrow());
                    if (tiles.Contains(v) && TryGetData(f, scaredKey, p.X, p.Y, out var scaredString) && int.TryParse(scaredString, out var scared))
                    {
                        scared++;
                        SetData(f, scaredKey, p.X, p.Y, scared.ToString());
                        return true;
                    }
                }
            }
            return false;
        }


        public static void ActivateSprinkler(GameLocation environment, Vector2 tileLocation, Object obj, bool delay)
        {
            if (Game1.player.team.SpecialOrderRuleActive("NO_SPRINKLER", null))
            {
                return;
            }
            int radius = GetSprinklerRadius(obj);
            if (radius < 0)
                return;

            obj.Location = environment;

            foreach (Vector2 tile in GetSprinklerTiles(tileLocation, radius))
            {
                obj.ApplySprinkler(tile);
                if (environment.objects.TryGetValue(tile, out var o) && o is IndoorPot)
                {
                    (o as IndoorPot).hoeDirt.Value.state.Value = 1;
                    o.showNextIndex.Value = true;
                }
            }
            ApplySprinklerAnimation(tileLocation, radius, environment, delay ? Game1.random.Next(1000) : 0);
        }
        public static void ApplySprinklerAnimation(Vector2 tileLocation, int radius, GameLocation location, int delay)
        {
            int id = (int)(tileLocation.X * 4000f + tileLocation.Y * 10);
            if (radius < 0 || location.getTemporarySpriteByID(id) is not null)
            {
                return;
            }
            var position = tileLocation * 64 + new Vector2(32, 16);
            float layerDepth = 1;
            if (radius == 0)
            {
                float rotation = 60 * MathHelper.Pi / 180;
                int a = 24;
                int b = 40;
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(a, -b), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = rotation,
                    delayBeforeAnimationStart = delay,
                    id = id
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(b, a), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = 1.57079637f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = id
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(-a, b), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = 3.14159274f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = id
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(-b, -a), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = 4.712389f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = id
                });
                return;
            }
            if (radius == 1)
            {
                location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 1984, 192, 192), 60f, 3, 100, position + new Vector2(-96f, -96f), false, false)
                {
                    color = Color.White * 0.4f,
                    delayBeforeAnimationStart = delay,
                    id = id,
                    layerDepth = layerDepth,
                    scale = 1.3f
                });
                return;
            }
            float scale = radius / 1.6f;
            location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 2176, 320, 320), 60f, 4, 100, position + new Vector2(32f, 32f) + new Vector2(-160f, -160f) * scale, false, false)
            {
                color = Color.White * 0.4f,
                delayBeforeAnimationStart = delay,
                id = id,
                layerDepth = layerDepth,
                scale = scale
            });
        }
        public static IEnumerable<Object> GetSprinklers(GameLocation l)
        {
            return l.modData.FieldDict.Where(p => p.Key.StartsWith(sprinklerKey + ",")).Select(p => GetSprinkler(l, int.Parse(p.Key.Split(',')[1]), int.Parse(p.Key.Split(',')[2])));
        }
        public static IEnumerable<Vector2> GetSprinklerVectors(GameLocation l)
        {
            return l.modData.FieldDict.Where(p => p.Key.StartsWith(sprinklerKey + ",")).Select(p => new Vector2(int.Parse(p.Key.Split(',')[1]),int.Parse(p.Key.Split(',')[2])));
        }
        public static IEnumerable<Point> GetSprinklerPoints(GameLocation l)
        {
            return l.modData.FieldDict.Where(p => p.Key.StartsWith(sprinklerKey + ",")).Select(p => new Point(int.Parse(p.Key.Split(',')[1]),int.Parse(p.Key.Split(',')[2])));
        }
        public static IEnumerable<Object> GetScarecrows(GameLocation l)
        {
            return l.modData.FieldDict.Where(p => p.Key.StartsWith(scarecrowKey + ",")).Select(p => GetScarecrow(l, int.Parse(p.Key.Split(',')[1]), int.Parse(p.Key.Split(',')[2])));
        }
        public static IEnumerable<Vector2> GetScarecrowVectors(GameLocation l)
        {
            return l.modData.FieldDict.Where(p => p.Key.StartsWith(scarecrowKey + ",")).Select(p => new Vector2(int.Parse(p.Key.Split(',')[1]),int.Parse(p.Key.Split(',')[2])));
        }
        public static IEnumerable<Point> GetScarecrowPoints(GameLocation l)
        {
            return l.modData.FieldDict.Where(p => p.Key.StartsWith(scarecrowKey + ",")).Select(p => new Point(int.Parse(p.Key.Split(',')[1]),int.Parse(p.Key.Split(',')[2])));
        }

        public static void SetAltTextureForObject(Object obj)
        {
            if (atApi is null)
                return;
            try
            {
                AccessTools.Method(atApi.GetType(), "SetTextureForObject").Invoke(atApi, new object[] { obj });
            }
            catch
            {
            }
        }

        public static Texture2D GetAltTextureForObject(Object obj, out Rectangle sourceRect)
        {
            sourceRect = new Rectangle();
            var inputParams = new object[] { obj, sourceRect };
            Texture2D result = (Texture2D)AccessTools.Method(atApi.GetType(), "GetTextureForObject").Invoke(atApi, inputParams);
            sourceRect = (Rectangle)inputParams[1];
            return result;
        }

        public static bool RemoveWhere(KeyValuePair<Vector2, TerrainFeature> pair)
        {
            HoeDirt dirt = pair.Value as HoeDirt;
            var match = dirt != null && dirt.crop == null && Game1.random.NextDouble() < 0.8;
            if (!Config.EnableMod || !match)
                return match;

            for (int i = 0; i < 4; i++)
            {
                if (dirt.modData.TryGetValue(sprinklerKey + i, out var sprinklerString))
                {
                    SMonitor.Log($"Preventing hoedirt removal");
                    return false;
                }
            }
            return match;
        }
        public static bool TryGetData(GameLocation l, string key, int x, int y, out string value)
        {
            value = null;
            if (l.modData.TryGetValue($"{key},{x},{y}", out value) && !string.IsNullOrEmpty(value))
            {
                return true;
            }
            return false;
        }
        public static bool HasData(GameLocation l, string key, int x, int y)
        {
            return l.modData.ContainsKey($"{key},{x},{y}");
        }
        public static void SetData(GameLocation l, string key, int x, int y, string value)
        {
            if(value == null)
            {
                l.modData.Remove($"{key},{x},{y}");
            }
            else
            {
                l.modData[$"{key},{x},{y}"] = value;
            }
        }
    }
}