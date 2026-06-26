using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
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
        public static void DoCastSpell(GameLocation l, Farmer who, Vector2 center, SpellCastData spell)
        {
            if (!Context.IsWorldReady || l is null || spell is null)
                return;
            foreach (var str in spell.Buffs)
            {
                who.applyBuff(str);
            }

            foreach (var e in spell.Effects)
            {
                if (e.PerTile)
                {
                    List<object> applied = new();
                    foreach (var tile in GetTiles(center, spell.Radius))
                    {
                        ApplyEffectToTile(l, who, tile, e, applied);
                    }
                }
                else
                {
                    switch (e.EffectType)
                    {
                        case SpellEffectType.Explode:
                            l.explode(center, e.Radius > 0 ? (int)e.Radius : spell.Radius, who, e.Affected.Contains(SpellAffectedType.Farmer), (int)e.Value, e.Affected.Contains(SpellAffectedType.Object));
                            break;
                    }
                }
            }
            foreach (var s in spell.Sprites)
            {
                if (s.PerTile)
                {
                    foreach (var tile in GetTiles(center, spell.Radius))
                    {
                        ApplySpriteToTile(l, tile, s, Vector2.Distance(center, tile));
                    }
                }
                else
                {
                    switch (s.Type)
                    {
                        case SpriteType.Fountain:
                            CreateFountain(l, center, who, spell, s);
                            break;
                    }
                }
            }
        }
        public static void ApplySpriteToTile(GameLocation l, Vector2 tile, SpriteData data, float distance)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            switch (data.Type)
            {
                case SpriteType.Balloon:
                    CreateBalloon(l, tile * 64, data); 
                    break;
                case SpriteType.Butterfly:
                    CreateButterfly(l, tile * 64, data); 
                    break;
                case SpriteType.Explosion:
                    CreateExplosion(l, tile * 64, distance, data); 
                    break;
                case SpriteType.EvilRabbit:
                    CreateEvilRabbit(l, tile * 64, data); 
                    break;
                case SpriteType.Fire:
                    CreateFire(l, tile * 64, data); 
                    break;
                case SpriteType.Ice:
                    CreateIce(l, tile * 64, data); 
                    break;
                case SpriteType.Heart:
                    CreateHeart(l, tile * 64, data);
                    break;
                case SpriteType.Poof:
                    CreatePoof(l, tile * 64, data); 
                    break;
                case SpriteType.Smoking:
                    CreateSmoking(l, tile * 64, data); 
                    break;
                case SpriteType.Sparkle:
                    Rectangle r = new((tile * 64).ToPoint(), new Point(64, 64));
                    CreateSparkle(l, r, data); 
                    break;
            }
        }
        public static bool ApplyEffectToTile(GameLocation l, Farmer who, Vector2 tile, SpellEffect effect, List<object> applied)
        {
            if (!Context.IsWorldReady || l is null)
                return false;
            bool result = false;
            foreach(var t in effect.Affected)
            {
                result = ApplyEffect(l, who, tile, t, effect, applied);
                if (effect.First && result)
                    return result;
            }
            return result;
        }

        public static bool ApplyEffect(GameLocation l, Farmer who, Vector2 tile, SpellAffectedType t, SpellEffect effect, List<object> applied)
        {
            Object o;
            TerrainFeature tf;
            bool result = false;
            switch (t)
            {
                case SpellAffectedType.Animal:
                    foreach(var a in l.animals.Values)
                    {
                        if (applied.Contains(a))
                            continue;
                        var bb = a.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyAnimalEffect(l, who, a, effect);
                            applied.Add(a);
                            result = true;
                        }
                    }
                    break;
                case SpellAffectedType.Building:
                    foreach(var b in l.buildings)
                    {
                        if (applied.Contains(b))
                            continue;
                        var bb = b.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyBuildingEffect(l, who, b, effect);
                            applied.Add(b);
                            result = true;
                        }
                    }
                    break;
                case SpellAffectedType.Pet:
                    foreach(var p in l.characters.Where(c => c is Pet))
                    {
                        if (applied.Contains(p))
                            continue;
                        var bb = p.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyPetEffect(l, who, p as Pet, effect);
                            applied.Add(p);
                            result = true;
                        }
                    }
                    break;
                case SpellAffectedType.Monster:
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
                            result = true;
                        }
                    }
                    break;
                case SpellAffectedType.NPC:
                    break;
                case SpellAffectedType.Farmer:
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
                            result = true;
                        }
                    }
                    break;
                case SpellAffectedType.Tile:
                    ApplyTileEffect(l, who, tile, effect);
                    break;
                case SpellAffectedType.Object:
                    if(l.Objects.TryGetValue(tile, out o) && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, who, tile, o, effect);
                        applied.Add(o);
                        result = true;
                    }
                    break;
                case SpellAffectedType.ResourceClump:
                    var rc = l.resourceClumps.FirstOrDefault(c => c.occupiesTile((int)tile.X, (int)tile.Y));
                    if (rc is not null)
                    {
                        ApplyResourceClumpEffect(l, who, tile, rc, effect);
                        //applied.Add(rc);
                        result = true;
                    }
                    break;
                case SpellAffectedType.HoeDirt:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is HoeDirt && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, who, tile, tf, effect);
                        applied.Add(tf);
                        result = true;
                    }
                    break;
                case SpellAffectedType.Crop:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is HoeDirt dirt && dirt.crop is not null && !applied.Contains(tf))
                    {
                        ApplyCropEffect(l, who, tile, dirt, effect);
                        applied.Add(tf);
                        result = true;
                    }
                    break;
                case SpellAffectedType.Grass:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is Grass && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, who, tile, tf, effect);
                        applied.Add(tf);
                        result = true;
                    }
                    break;
                case SpellAffectedType.Stone:
                    if (l.Objects.TryGetValue(tile, out o) && o.IsBreakableStone() && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, who, tile, o, effect);
                        applied.Add(o);
                        result = true;
                    }
                    break;
                case SpellAffectedType.Twig:
                    if (l.Objects.TryGetValue(tile, out o) && o.IsTwig() && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, who, tile, o, effect);
                        applied.Add(o);
                        result = true;
                    }
                    break;
                case SpellAffectedType.Weed:
                    if (l.Objects.TryGetValue(tile, out o) && o.IsWeeds() && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, who, tile, o, effect);
                        applied.Add(o);
                        result = true;
                    }
                    break;
                case SpellAffectedType.Tree:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is Tree && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, who, tile, tf, effect);
                        applied.Add(tf);
                        result = true;
                    }
                    break;
                case SpellAffectedType.FruitTree:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is FruitTree && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, who, tile, tf, effect);
                        applied.Add(tf);
                        result = true;
                    }
                    break;
            }
            return result;
        }

        private static void ApplyAnimalEffect(GameLocation l, Farmer who, FarmAnimal a, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Pet:
                    a.pet(effect.AsFarmer ? who : new Farmer(), true);
                    break;
                case SpellEffectType.Harvest:
                    if(a.currentProduce.Value != null && a.isAdult())
                    {
                        Object produce = ItemRegistry.Create<Object>("(O)" + a.currentProduce.Value, 1, 0, false);
                        produce.CanBeSetDown = false;
                        produce.Quality = a.produceQuality.Value;
                        if (a.hasEatenAnimalCracker.Value)
                        {
                            produce.Stack = 2;
                        }
                        TryReturnObject(produce, who);
                        a.HandleStatsOnProduceCollected(produce, (uint)produce.Stack);
                        a.currentProduce.Value = null;
                        if(effect.AsFarmer)
                            a.friendshipTowardFarmer.Value = Math.Min(1000, a.friendshipTowardFarmer.Value + 5);
                        a.ReloadTextureIfNeeded(false);
                        who.gainExperience(0, 5);
                    }
                    break;
            }
        }

        private static void ApplyBuildingEffect(GameLocation l, Farmer who, Building b, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Water:
                    if (b is PetBowl pb)
                        pb.watered.Value = true;
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(b, effect);
                    break;
            }
        }

        private static void ApplyPetEffect(GameLocation l, Farmer who, Pet p, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Pet:
                    p.checkAction(effect.AsFarmer ? who : new Farmer(), l);
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(p, effect);
                    break;
            }
        }

        private static void ApplyTileEffect(GameLocation l, Farmer who, Vector2 tile, SpellEffect effect)
        {

            switch (effect.EffectType)
            {
                case SpellEffectType.Light:
                    var id = Guid.NewGuid().ToString();
                    l.sharedLights.AddLight(new(id, 1, tile * 64 + new Vector2(32, 32), effect.Radius, effect.Color));
                    LightDict.Add(id, new()
                    {
                        id = id,
                        location = l,
                        timeLeft = (int)effect.Value,
                        totalTime = (int)effect.Value,
                        radius = effect.Radius
                    });
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Explode:
                    l.explode(tile, 0, who, effect.Affected.Contains(SpellAffectedType.Farmer), (int)effect.Value, effect.Affected.Contains(SpellAffectedType.Object));
                    break;
            }
        }

        public static void ApplyMonsterEffect(Farmer who, Monster m, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Damage:
                    m.takeDamage((int)effect.Value, 0, 0, false, 0, who);
                    break;
                case SpellEffectType.Buff:
                    BuffDict.Add(m, new MonsterBuffManager(m));
                    BuffDict[m].AddBuff((string)effect.Value);
                    break;
                case SpellEffectType.Light:
                    var id = Guid.NewGuid().ToString();
                    m.currentLocation.sharedLights.AddLight(new(id, 1, m.GetBoundingBox().Center.ToVector2(), effect.Radius, effect.Color));
                    LightDict.Add(id, new()
                    {
                        id = id,
                        location = m.currentLocation,
                        target = m,
                        timeLeft = (int)effect.Value,
                        totalTime = (int)effect.Value,
                        radius = effect.Radius
                    });
                        break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(m, effect);
                    break;
            }
        }

        public static void ApplyFarmerEffect(Farmer f, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Buff:
                    f.applyBuff((string)effect.Value);
                    break;
                case SpellEffectType.Damage:
                    f.takeDamage((int)effect.Value, true, null);
                    break;
                case SpellEffectType.Heal:
                    f.health = Math.Clamp(f.health + (int)effect.Value, 0, f.maxHealth);
                    break;
                case SpellEffectType.Invincible:
                    f.temporarilyInvincible = true;
                    f.flashDuringThisTemporaryInvincibility = true;
                    f.temporaryInvincibilityTimer = 0;
                    f.currentTemporaryInvincibilityDuration = (int)effect.Value;
                    break;
                case SpellEffectType.Light:
                    var id = Guid.NewGuid().ToString();
                    f.currentLocation.sharedLights.AddLight(new(id, 1, new Vector2(f.Position.X + 32f, f.Position.Y + 64f), effect.Radius, effect.Color, LightSource.LightContext.None, f.UniqueMultiplayerID));
                    LightDict.Add(id, new()
                    {
                        id = id,
                        location = f.currentLocation,
                        target = f,
                        timeLeft = (int)effect.Value,
                        totalTime = (int)effect.Value,
                        radius = effect.Radius
                    });
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(f, effect);
                    break;
            }
        }

        public static void ApplyObjectEffect(GameLocation l, Farmer who, Vector2 tile, Object o, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Burn:
                    CreateBurn(l, tile, o, null);
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Destroy:
                    l.Objects.Remove(tile);
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(o, effect);
                    break;
            }
        }

        public static void ApplyResourceClumpEffect(GameLocation l, Farmer who, Vector2 tile, ResourceClump rc, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Burn:
                    CreateBurn(l, tile, rc, null);
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Destroy:
                    l.Objects.Remove(tile);
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(rc, effect);
                    break;
            }
        }

        public static void ApplyTerrainFeatureEffect(GameLocation l, Farmer who, Vector2 tile, TerrainFeature tf, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Burn:
                    CreateBurn(l, tile, tf, null);
                    break;
                case SpellEffectType.Destroy:
                    l.terrainFeatures.Remove(tile);
                    break;
                case SpellEffectType.Fertilize:
                    if(tf is HoeDirt)
                    {
                        (tf as HoeDirt).plant((string)effect.Value, who, true);
                    }
                    break;
                case SpellEffectType.Grow:
                    if(tf is Grass)
                    {
                        (tf as Grass).numberOfWeeds.Value = 4;
                    }
                    else if (tf is FruitTree)
                    {
                        (tf as FruitTree).growthStage.Value = 5;
                    }
                    else if(tf is Tree)
                    {
                        (tf as Tree).growthStage.Value = 4;
                    }
                    break;
                case SpellEffectType.Harvest:
                    if(tf is Grass)
                    {
                        (tf as Grass).TryDropItemsOnCut((Tool)ItemRegistry.Create(effect.Value as string ?? "(W)47"), false);
                    }
                    else if (tf is FruitTree && (tf as FruitTree).struckByLightningCountdown.Value <= 0)
                    {
                        foreach (var f in (tf as FruitTree).fruit)
                        {
                            TryReturnObject(f, who);
                        }
                        (tf as FruitTree).fruit.Clear();
                    }
                    break;
                case SpellEffectType.Plant:
                    if(tf is HoeDirt)
                    {
                        (tf as HoeDirt).plant((string)effect.Value, who, false);
                    }
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Water:
                    if (tf is HoeDirt)
                    {
                        (tf as HoeDirt).state.Value = 1;
                    }
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(tf, effect);
                    break;
            }
        }

        public static void ApplyCropEffect(GameLocation l, Farmer who, Vector2 tile, HoeDirt dirt, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Burn:
                    CreateBurn(l, tile, dirt, null);
                    break;
                case SpellEffectType.Grow:
                    dirt.crop?.growCompletely();
                    break;
                case SpellEffectType.Harvest:
                    dirt.crop?.harvest((int)tile.X,(int)tile.Y, dirt, null, true);
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Water:
                    dirt.state.Value = 1;
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(dirt.crop, effect);
                    break;
            }
        }
    }
}