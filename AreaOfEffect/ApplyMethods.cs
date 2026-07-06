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
using xTile.Tiles;
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
                if (e.PerTile && (spell.Projectiles is null || spell.AreaType != AOEType.Line))
                {
                    List<object> applied = new();
                    foreach (var tile in GetTiles(who.Tile, center, spell))
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
                if (s.PerTile && (spell.Projectiles is null || spell.AreaType != AOEType.Line))
                {
                    foreach (var tile in GetTiles(who.Tile, center, spell))
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
                        case SpriteType.Lightning:
                            Utility.drawLightningBolt(center * 64, l);
                            break;
                        default:
                            ApplySpriteToTile(l, center, s, 0);
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
                case SpriteType.Animation:
                    CreateAnimation(l, tile * 64, data); 
                    break;
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
                case SpriteType.Heart:
                    CreateHeart(l, tile * 64, data);
                    break;
                case SpriteType.Ice:
                    CreateIce(l, tile * 64, data); 
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
                case SpriteType.Texture:
                    CreateTexture(l, tile * 64, data);
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
                    foreach (var c in l.characters.Where(c => c.IsVillager))
                    {
                        if (applied.Contains(c))
                            continue;
                        var bb = c.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyNPCEffect(who, c, effect);
                            applied.Add(c);
                            result = true;
                        }
                    }
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
                case SpellAffectedType.Horse:
                    foreach (var c in l.characters.Where(c => c is Horse))
                    {
                        if (applied.Contains(c))
                            continue;
                        var bb = c.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyHorseEffect(who, c as Horse, effect);
                            applied.Add(c);
                            result = true;
                        }
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
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
                case SpellEffectType.EffectOverTime:
                    SetEffectOverTime(l, who, Vector2.Zero, a, effect);
                    break;

            }
        }
        private static void ApplyBuildingEffect(GameLocation l, Farmer who, Building a, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Water:
                    if (a is PetBowl pb)
                        pb.watered.Value = true;
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
                case SpellEffectType.EffectOverTime:
                    SetEffectOverTime(l, who, Vector2.Zero, a, effect);
                    break;
            }
        }

        private static void ApplyPetEffect(GameLocation l, Farmer who, Pet a, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Pet:
                    a.checkAction(effect.AsFarmer ? who : new Farmer(), l);
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
                case SpellEffectType.EffectOverTime:
                    SetEffectOverTime(l, who, Vector2.Zero, a, effect);
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
                case SpellEffectType.EffectOverTime:
                    SetEffectOverTime(l, who, tile, tile, effect);
                    break;
            }
        }

        public static void ApplyMonsterEffect(Farmer who, Monster a, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Damage:
                    a.takeDamage((int)effect.Value, 0, 0, false, 0, who);
                    break;
                case SpellEffectType.Buff:
                    BuffDict.Add(a, new MonsterBuffManager(a));
                    BuffDict[a].AddBuff((string)effect.Value);
                    break;
                case SpellEffectType.Freeze:
                    a.stunTime.Value = (int)effect.Value;
                    break;
                case SpellEffectType.Light:
                    var id = Guid.NewGuid().ToString();
                    a.currentLocation.sharedLights.AddLight(new(id, 1, a.GetBoundingBox().Center.ToVector2(), effect.Radius, effect.Color));
                    LightDict.Add(id, new()
                    {
                        id = id,
                        location = a.currentLocation,
                        target = a,
                        timeLeft = (int)effect.Value,
                        totalTime = (int)effect.Value,
                        radius = effect.Radius
                    });
                        break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
                case SpellEffectType.EffectOverTime:
                    SetEffectOverTime(null, who, Vector2.Zero, a, effect);
                    break;
            }
        }

        public static void ApplyNPCEffect(Farmer who, NPC a, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Light:
                    var id = Guid.NewGuid().ToString();
                    a.currentLocation.sharedLights.AddLight(new(id, 1, a.GetBoundingBox().Center.ToVector2(), effect.Radius, effect.Color));
                    LightDict.Add(id, new()
                    {
                        id = id,
                        location = a.currentLocation,
                        target = a,
                        timeLeft = (int)effect.Value,
                        totalTime = (int)effect.Value,
                        radius = effect.Radius
                    });
                        break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
                case SpellEffectType.EffectOverTime:
                    SetEffectOverTime(null, who, Vector2.Zero, a, effect);
                    break;
            }
        }

        public static void ApplyHorseEffect(Farmer who, Horse a, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Light:
                    var id = Guid.NewGuid().ToString();
                    a.currentLocation.sharedLights.AddLight(new(id, 1, a.GetBoundingBox().Center.ToVector2(), effect.Radius, effect.Color));
                    LightDict.Add(id, new()
                    {
                        id = id,
                        location = a.currentLocation,
                        target = a,
                        timeLeft = (int)effect.Value,
                        totalTime = (int)effect.Value,
                        radius = effect.Radius
                    });
                        break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
                case SpellEffectType.EffectOverTime:
                    SetEffectOverTime(null, who, Vector2.Zero, a, effect);
                    break;
            }
        }

        public static void ApplyFarmerEffect(Farmer a, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Buff:
                    a.applyBuff((string)effect.Value);
                    break;
                case SpellEffectType.Damage:
                    a.takeDamage((int)effect.Value, true, null);
                    break;
                case SpellEffectType.Heal:
                    a.health = Math.Clamp(a.health + (int)effect.Value, 0, a.maxHealth);
                    break;
                case SpellEffectType.Invincible:
                    a.temporarilyInvincible = true;
                    a.flashDuringThisTemporaryInvincibility = true;
                    a.temporaryInvincibilityTimer = 0;
                    a.currentTemporaryInvincibilityDuration = (int)effect.Value;
                    break;
                case SpellEffectType.Light:
                    var id = Guid.NewGuid().ToString();
                    a.currentLocation.sharedLights.AddLight(new(id, 1, new Vector2(a.Position.X + 32f, a.Position.Y + 64f), effect.Radius, effect.Color, LightSource.LightContext.None, a.UniqueMultiplayerID));
                    LightDict.Add(id, new()
                    {
                        id = id,
                        location = a.currentLocation,
                        target = a,
                        timeLeft = (int)effect.Value,
                        totalTime = (int)effect.Value,
                        radius = effect.Radius
                    });
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;

                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
                case SpellEffectType.EffectOverTime:
                    SetEffectOverTime(null, null, Vector2.Zero, a, effect);
                    break;
            }
        }

        public static void ApplyObjectEffect(GameLocation l, Farmer who, Vector2 tile, Object a, SpellEffect effect)
        {
            if (effect.Unaffected?.Count > 0)
            {
                if ((a.IsTwig() && effect.Unaffected.Contains(SpellAffectedType.Twig)) || (a.IsBreakableStone() && effect.Unaffected.Contains(SpellAffectedType.Stone)) || (a.IsWeeds() && effect.Unaffected.Contains(SpellAffectedType.Weed)))
                    return;
            }
            switch (effect.EffectType)
            {
                case SpellEffectType.Burn:
                    CreateBurn(l, tile, a, null);
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Destroy:
                    l.Objects.Remove(tile);
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
                case SpellEffectType.EffectOverTime:
                    SetEffectOverTime(l, who, tile, a, effect);
                    break;
            }
        }

        public static void ApplyResourceClumpEffect(GameLocation l, Farmer who, Vector2 tile, ResourceClump a, SpellEffect effect)
        {

            switch (effect.EffectType)
            {
                case SpellEffectType.Burn:
                    CreateBurn(l, tile, a, null);
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Destroy:
                    l.Objects.Remove(tile);
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
                case SpellEffectType.EffectOverTime:
                    SetEffectOverTime(l, who, tile, a, effect);
                    break;
            }
        }

        public static void ApplyTerrainFeatureEffect(GameLocation l, Farmer who, Vector2 tile, TerrainFeature a, SpellEffect effect)
        {
            if(effect.Unaffected?.Count > 0)
            {
                if ((a is HoeDirt dirt && (effect.Unaffected.Contains(SpellAffectedType.HoeDirt)|| (dirt.crop is not null && effect.Unaffected.Contains(SpellAffectedType.Crop)))) || (a is Grass && effect.Unaffected.Contains(SpellAffectedType.Grass)) || (a is Tree && effect.Unaffected.Contains(SpellAffectedType.Tree)) || (a is FruitTree && effect.Unaffected.Contains(SpellAffectedType.FruitTree)))
                    return;
            }
            switch (effect.EffectType)
            {
                case SpellEffectType.Burn:
                    CreateBurn(l, tile, a, null);
                    break;
                case SpellEffectType.Destroy:
                    l.terrainFeatures.Remove(tile);
                    break;
                case SpellEffectType.Fertilize:
                    if(a is HoeDirt)
                    {
                        (a as HoeDirt).plant((string)effect.Value, who, true);
                    }
                    break;
                case SpellEffectType.Grow:
                    if(a is Grass)
                    {
                        (a as Grass).numberOfWeeds.Value = effect.Value is null ? 4 : (int)effect.Value;
                    }
                    else if (a is FruitTree)
                    {
                        (a as FruitTree).growthStage.Value = effect.Value is null ? 5 : (int)effect.Value;
                    }
                    else if(a is Tree)
                    {
                        (a as Tree).growthStage.Value = effect.Value is null ? 4 : (int)effect.Value;
                    }
                    break;
                case SpellEffectType.Harvest:
                    if(a is Grass)
                    {
                        (a as Grass).TryDropItemsOnCut((Tool)ItemRegistry.Create(effect.Value as string ?? "(W)47"), false);
                    }
                    else if (a is FruitTree && (a as FruitTree).struckByLightningCountdown.Value <= 0)
                    {
                        foreach (var f in (a as FruitTree).fruit)
                        {
                            TryReturnObject(f, who);
                        }
                        (a as FruitTree).fruit.Clear();
                    }
                    break;
                case SpellEffectType.Plant:
                    if(a is HoeDirt)
                    {
                        (a as HoeDirt).plant((string)effect.Value, who, false);
                    }
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Water:
                    if (a is HoeDirt)
                    {
                        (a as HoeDirt).state.Value = 1;
                    }
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
                case SpellEffectType.EffectOverTime:
                    SetEffectOverTime(l, who, tile, a, effect);
                    break;
            }
        }

        public static void ApplyCropEffect(GameLocation l, Farmer who, Vector2 tile, HoeDirt a, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Burn:
                    CreateBurn(l, tile, a, null);
                    break;
                case SpellEffectType.Grow:
                    if(effect.Value is null)
                    {
                        a.crop?.growCompletely();
                    }
                    else
                    {
                        a.crop.currentPhase.Value = Math.Min(a.crop.phaseDays.Count - 1, (int)effect.Value);
                        a.crop.dayOfCurrentPhase.Value = 0;
                    }
                    break;
                case SpellEffectType.Harvest:
                    a.crop?.harvest((int)tile.X,(int)tile.Y, a, null, true);
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Water:
                    a.state.Value = 1;
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a.crop, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
                case SpellEffectType.EffectOverTime:
                    SetEffectOverTime(l, who, tile, a, effect);
                    break;
            }
        }
    }
}