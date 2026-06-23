using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace AreaOfEffect
{
    public partial class ModEntry
    {

        public static void ApplyAOEEffect(GameLocation l, Farmer who, Vector2 center, AOEEffectData data)
        {
            if (!Context.IsWorldReady || l is null || data is null)
                return;
            foreach (var e in data.Effects)
            {
                List<object> applied = new();
                foreach (var tile in GetTiles(center, data.Radius))
                {
                    ApplyEffectToTile(l, who, tile, e, applied);
                }
            }
            foreach (var s in data.Sprites)
            {
                foreach (var tile in GetTiles(center, data.Radius))
                {
                    if (s.PerTile)
                    {
                        ApplySpriteToTile(l, tile, s);
                    }
                }
            }
        }


        public static List<Vector2> GetTiles(Vector2 tileLocation, int radius)
        {
            Vector2 start = tileLocation + new Vector2(-1, -1) * radius;
            Vector2 position = tileLocation;
            List<Vector2> list = new();
            var diameter = (radius) * 2;
            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    Vector2 tile = start + new Vector2(x, y);
                    var distance = (int)Math.Round(Vector2.Distance(position, tile));
                    if (distance <= radius)
                        list.Add(tile);
                }
            }
            return list;

        }
        public static void ApplySpriteToTile(GameLocation l, Vector2 tile, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            switch (data.Type)
            {
                case SpriteType.Fire:
                    CreateFire(l, tile * 64); 
                    break;
                case SpriteType.Ice:
                    CreateIce(l, tile * 64); 
                    break;
                case SpriteType.Heart:
                    CreateHeart(l, tile * 64, data); 
                    break;
                case SpriteType.Smoke:
                    CreateSmoke(l, tile * 64); 
                    break;
                case SpriteType.Sparkle:
                    Rectangle r = new(tile.ToPoint(), data.Rect.Value.Size);
                    CreateSparkle(l, r, data); 
                    break;
            }
        }
        public static void ApplyEffectToTile(GameLocation l, Farmer who, Vector2 tile, AOEEffect effect, List<object> applied)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            foreach(var t in effect.Affected)
            {
                ApplyEffect(l, who, tile, t, effect, applied);
            }
        }

        public static void ApplyEffect(GameLocation l, Farmer who, Vector2 tile, AOEAffectedType t, AOEEffect effect, List<object> applied)
        {
            Object o;
            TerrainFeature tf;
            switch (t)
            {
                case AOEAffectedType.Monster:
                    foreach(var c in l.characters.Where(c => c is Monster))
                    {
                        if (applied.Contains(c))
                            continue;
                        var bb = c.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyMonsterEffect(who, c as Monster, effect);
                            applied.Add(c);
                        }
                    }
                    break;
                case AOEAffectedType.NPC:
                case AOEAffectedType.Farmer:
                    foreach (var f in l.farmers)
                    {
                        if (applied.Contains(f))
                            continue;
                        var bb = f.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyFarmerEffect(f, effect);
                            applied.Add(f);
                        }
                    }
                    break;
                case AOEAffectedType.Tile:
                case AOEAffectedType.Object:
                    if(l.Objects.TryGetValue(tile, out o) && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, tile, o, effect);
                        applied.Add(o);
                    }
                    break;
                case AOEAffectedType.ResourceClump:
                case AOEAffectedType.HoeDirt:
                case AOEAffectedType.Crop:
                case AOEAffectedType.Grass:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is Grass && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, tile, tf, effect);
                        applied.Add(tf);
                    }
                    break;
                case AOEAffectedType.Stone:
                case AOEAffectedType.Twig:
                    if (l.Objects.TryGetValue(tile, out o) && o.IsTwig() && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, tile, o, effect);
                        applied.Add(o);
                    }
                    break;
                case AOEAffectedType.Weed:
                    if (l.Objects.TryGetValue(tile, out o) && o.IsWeeds() && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, tile, o, effect);
                        applied.Add(o);
                    }
                    break;
                case AOEAffectedType.Tree:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is Tree && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, tile, tf, effect);
                        applied.Add(tf);
                    }
                    break;
                case AOEAffectedType.FruitTree:
                    break;
                
            }
        }

        public static void ApplyMonsterEffect(Farmer who, Monster c, AOEEffect effect)
        {
            switch (effect.EffectType)
            {
                case AOEEffectType.Damage:
                    c.takeDamage((int)effect.Value, 0, 0, false, 0, who);
                    break;
            }
        }

        public static void ApplyFarmerEffect(Farmer f, AOEEffect effect)
        {
            switch (effect.EffectType)
            {
                case AOEEffectType.Heal:
                    f.health = Math.Clamp(f.health + (int)effect.Value, 0, f.maxHealth);
                    break;
            }
        }

        public static void ApplyObjectEffect(GameLocation l, Vector2 tile, Object o, AOEEffect effect)
        {
            switch (effect.EffectType)
            {
                case AOEEffectType.Burn:
                    CreateBurn(l, tile, o);
                    break;
            }
        }

        public static void ApplyTerrainFeatureEffect(GameLocation l, Vector2 tile, TerrainFeature tf, AOEEffect effect)
        {
            switch (effect.EffectType)
            {
                case AOEEffectType.Burn:
                    CreateBurn(l, tile, tf);
                    break;
            }
        }

        public static Vector2 GetTargetTile(Farmer f, AOEEffectData t, int maxDistance)
        {
            var target = Game1.currentCursorTile;
            int d = maxDistance;
            float m = Vector2.Distance(target, f.Tile);
            if(m > d)
            {
                target = Vector2.Lerp(f.Tile, target, d / m);
                target = new Vector2((int)Math.Round(target.X), (int)Math.Round(target.Y));
            }
            return target;
        }

        public static int GetCurrentCharges(Tool tool, int max)
        {
            if (!tool.modData.TryGetValue(chargesKey, out var str))
            {
                SetCurrentCharges(tool, max);
                return max;
            }
            return int.Parse(str);
        }
        public static void SetCurrentCharges(Tool tool, int value)
        {
            tool.modData[chargesKey] = value.ToString();
        }

        public static void DestroyAt(GameLocation l, Vector2 tile, object o)
        {
            if (o is Object)
            {
                l.Objects.Remove(tile);
            }
            else if (o is TerrainFeature tf)
            {
                if (tf is Tree t)
                {
                    if (t.growthStage.Value >= 5)
                    {
                        t.health.Value = 0;
                        AccessTools.Method(typeof(Tree), "performTreeFall").Invoke(t, new object[] { null, 1, tile });
                    }
                    else
                    {
                        l.terrainFeatures.Remove(tile);
                    }
                }
                else if(tf is Grass g)
                {
                    g.reduceBy(4, true);
                }
            }
        }

        public static bool TryGetTool(Tool __instance, out AOEToolData data)
        {
            var key = __instance.ItemId;
            if(!ToolDict.TryGetValue(__instance.ItemId, out data))
            {
                data = null;
                return false;
            }
            return true;
        }
        public static bool TryGetEffect(Tool __instance, out AOEEffectData data)
        {
            var key = __instance.ItemId;
            if (__instance.modData.TryGetValue(effectKey, out key))
            {
            }
            else if(TryGetTool(__instance, out var tdata) && tdata.Type != null)
            {
                key = tdata.Type;
            }
            else
            {
                data = null;
                return false;
            }
            if (!EffectDict.TryGetValue(key, out data))
            {
                data = null;
                return false;
            }
            return true;
        }

        public static void SetEffect(Tool tool, string type)
        {
            Game1.playSound(Config.SetEffectSound);
            tool.modData[effectKey] = type;
        }
    }
}