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
                if (s.PerTile)
                {
                    foreach (var tile in GetTiles(center, data.Radius)) 
                    {
                        ApplySpriteToTile(l, tile, s);
                    }
                }
                else
                {
                    switch (s.Type)
                    {
                        case SpriteType.Fountain:
                            CreateFountain(l, center, who, data, s);
                            break;
                    }
                }
            }
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
                    break;
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
                    break;
                case AOEAffectedType.Object:
                    if(l.Objects.TryGetValue(tile, out o) && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, tile, o, effect);
                        applied.Add(o);
                    }
                    break;
                case AOEAffectedType.ResourceClump:
                    break;
                case AOEAffectedType.HoeDirt:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is HoeDirt && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, who, tile, tf, effect);
                        applied.Add(tf);
                    }
                    break;
                case AOEAffectedType.Crop:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is HoeDirt dirt && dirt.crop is not null && !applied.Contains(tf))
                    {
                        ApplyCropEffect(l, who, tile, dirt, effect);
                        applied.Add(tf);
                    }
                    break;
                case AOEAffectedType.Grass:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is Grass && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, who, tile, tf, effect);
                        applied.Add(tf);
                    }
                    break;
                case AOEAffectedType.Stone:
                    if (l.Objects.TryGetValue(tile, out o) && o.IsBreakableStone() && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, tile, o, effect);
                        applied.Add(o);
                    }
                    break;
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
                        ApplyTerrainFeatureEffect(l, who, tile, tf, effect);
                        applied.Add(tf);
                    }
                    break;
                case AOEAffectedType.FruitTree:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is FruitTree && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, who, tile, tf, effect);
                        applied.Add(tf);
                    }
                    break;
                
            }
        }

        public static void ApplyMonsterEffect(Farmer who, Monster m, AOEEffect effect)
        {
            switch (effect.EffectType)
            {
                case AOEEffectType.Damage:
                    m.takeDamage((int)effect.Value, 0, 0, false, 0, who);
                    break;
                case AOEEffectType.Buff:
                    BuffDict.Add(m, new MonsterBuffManager(m));
                    BuffDict[m].AddBuff((string)effect.Value);
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
                case AOEEffectType.Damage:
                    f.takeDamage((int)effect.Value, true, null);
                    break;
                case AOEEffectType.Buff:
                    f.applyBuff((string)effect.Value);
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
                case AOEEffectType.Destroy:
                    l.Objects.Remove(tile);
                    break;
            }
        }

        public static void ApplyTerrainFeatureEffect(GameLocation l, Farmer who, Vector2 tile, TerrainFeature tf, AOEEffect effect)
        {
            switch (effect.EffectType)
            {
                case AOEEffectType.Burn:
                    CreateBurn(l, tile, tf);
                    break;
                case AOEEffectType.Destroy:
                    l.terrainFeatures.Remove(tile);
                    break;
                case AOEEffectType.Water:
                    if(tf is HoeDirt)
                    {
                        (tf as HoeDirt).state.Value = 1;
                    }
                    break;
                case AOEEffectType.Fertilize:
                    if(tf is HoeDirt)
                    {
                        (tf as HoeDirt).plant((string)effect.Value, who, true);
                    }
                    break;
            }
        }

        public static void ApplyCropEffect(GameLocation l, Farmer who, Vector2 tile, HoeDirt dirt, AOEEffect effect)
        {
            switch (effect.EffectType)
            {
                case AOEEffectType.Grow:
                    dirt.crop?.growCompletely();
                    break;
            }
        }
    }
}