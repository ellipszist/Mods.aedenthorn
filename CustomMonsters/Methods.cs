using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomMonsters
{
	public partial class ModEntry : Mod
    {
        public static string ChangeSpawnSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.SpawnSound == null) ? value : data.SpawnSound;
        }
        public static string ChangeDamageSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.DamageSound == null) ? value : (value == "rockGolemHit" && m is Bat ? data.DeathSound ?? value : data.DamageSound);
        }
        public static string ChangeDamageSound2(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.DamageSound2 == null) ? value : data.DamageSound2;
        }
        public static string ChangeDeathSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.DeathSound == null) ? value : data.DeathSound;
        }
        public static string ChangeDeathSound2(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.DeathSound2 == null) ? value : data.DeathSound2;
        }
        public static string ChangeMoveSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.MoveSound == null) ? value : data.MoveSound;
        }
        public static string ChangeCrumbleSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.CrumbleSound == null) ? value : data.CrumbleSound;
        }
        public static string ChangeUncrumbleSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.UncrumbleSound == null) ? value : data.UncrumbleSound;
        }
        public static string ChangeHitSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ShellSound == null) ? value : data.ShellSound;
        }
        public static string ChangeBreakSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.BreakSound == null) ? value : data.BreakSound;
        }
        public static int ChangeReviveTimer(int value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ReviveTimer == null) ? value : data.ReviveTimer.Value;
        }
        public static string ChangeSpritePath(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.Sprite == null) ? value : data.Sprite;
        }
        public static string ChangeMoveSound2(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.MoveSound2 == null) ? value : data.MoveSound2;
        }
        public static string ChangeProjectileSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileSound == null) ? value : data.ProjectileSound;
        }
        public static string ChangeProjectileSound2(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileSound2 == null) ? value : data.ProjectileSound2;
        }
        public static string ChangeProjectileDebuff(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileDebuff == null) ? value : data.ProjectileDebuff;
        }
        public static int ChangeProjectileDamage(int value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileDamage == null) ? value : data.ProjectileDamage.Value;
        }
        public static int ChangeProjectileCount(int value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileCount == null) ? value : data.ProjectileCount.Value;
        }
        public static float ChangeProjectileTimer(float value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileTimer == null) ? value : data.ProjectileTimer.Value;
        }
        public static int ChangeProjectileDamage2(int value, DinoMonster.BreathProjectile bp)
        {
            return (!Config.ModEnabled || bp is not CustomBreathProjectile cbp|| cbp.data.ProjectileDamage == null) ? value : cbp.data.ProjectileDamage.Value;
        }
        public static int ChangeProjectileIndex(int value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ProjectileIndex == null) ? value : data.ProjectileIndex.Value;
        }
        public static string ChangeArmorSound(string value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.ArmorSound == null) ? value : data.ArmorSound;
        }
        public static Color ChangeSprinkleColor(Color value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.SprinkleColor == null) ? value : MakeColor(data.SprinkleColor);
        }
        public static int ChangeLightType(int value, Monster m)
        {
            return (!Config.ModEnabled || !m.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.LightType == -1) ? value : data.LightType;
        }
        public static Color MakeColor(string value)
        {
            try
            {
                return new Color(
                    int.Parse(value.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(value.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(value.Substring(5, 2), System.Globalization.NumberStyles.HexNumber)
                );
            }
            catch
            {
                return Color.White;
            }
        }
        public static Monster GetSpawnMonster(Monster old, string newId, List<MonsterSpawnData> list, int level, Vector2 position)
        {
            if (old == null)
                return null;
            var spawnData = list.FirstOrDefault(m => m.MinLevel <= level && m.MaxLevel >= level && m.Types?.Contains(old.GetType().ToString()) != false);
            if (spawnData != default && Game1.random.NextDouble() < spawnData.Chance / 100.0)
            {
                var m = CreateMonster(newId, position);
                if (m != null)
                {
                    return m;
                }
            }
            return null;
        }
        public static Monster CreateMonster(string id, Vector2 position)
        {
            if (!Monsters.TryGetValue(id, out var data))
            {
                return null;
            }

            SMonitor.Log($"Creating monster '{id}' of type '{data.Type}' at position {position}.");

            var type = typeof(Monster).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Monster))).FirstOrDefault(t => t.Name == data.Type);
            if (type == null)
            {
                SMonitor.Log($"Invalid monster type {data.Type}", LogLevel.Warn);
                return null;
            }
            List<object> parameters = new List<object>();
            foreach (var param in data.Parameters.Split(','))
            {
                switch (param.Trim().ToLower())
                {
                    case "position":
                        parameters.Add(position);
                        break;
                    case "level":
                        parameters.Add(data.Level);
                        break;
                    case "facing":
                        parameters.Add(data.Facing);
                        break;
                    case "name":
                        parameters.Add(data.Name);
                        break;
                    case "color":
                        parameters.Add(MakeColor(data.Color));
                        break;
                    case "switch":
                        parameters.Add(data.Switch);
                        break;
                    default:
                        SMonitor.Log($"Unknown parameter '{param}' for monster '{id}'.", LogLevel.Warn);
                        break;
                }
            }
            if (type.GetConstructor(parameters.Select(p => p.GetType()).ToArray()) == null)
            {
                SMonitor.Log($"Invalid parameters for monster type {data.Type} {string.Join(",", parameters.Select(p => p.GetType()))}", LogLevel.Warn);
                return null;
            }
            var monster = (Monster)Activator.CreateInstance(type, parameters.ToArray());
            if (!parameters.Any())
            {
                monster.Position = position;
            }
            monster.modData[monsterKey] = id;
            if (monster is RockCrab crab)
            {
                if (data.StickBug)
                {
                    crab.makeStickBug();
                }
                if (data.WaiterChance != null)
                {
                    crab.waiter = Game1.random.NextDouble() < data.WaiterChance.Value / 100.0;
                }
                monster = crab;
            }
            if (data.Scale > -1)
            {
                monster.Scale = data.Scale;
            }
            if (data.Slipperiness > -1)
            {
                monster.Slipperiness = data.Slipperiness;
            }
            if (data.Speed > -1)
            {
                monster.speed = data.Speed;
            }
            if (data.Damage > -1)
            {
                monster.DamageToFarmer = data.Damage;
            }
            if (data.Health > -1)
            {
                monster.Health = data.Health;
                monster.MaxHealth = data.Health;
            }
            if (data.Experience > -1)
            {
                monster.ExperienceGained = data.Experience;
            }
            if (data.Resilience > -1)
            {
                monster.resilience.Value = data.Resilience;
            }
            monster.reloadSprite();
            if (monster is not DwarvishSentry && monster is not RockGolem && data.SpawnSound != null)
            {
                DelayedAction.playSoundAfterDelay(data.SpawnSound, 500, null, null, -1, false);
            }
            if (data.HardMode)
            {
                monster.isHardModeMonster.Value = true;
            }
            if (monster is AngryRoger)
            {
                monster.HideShadow = true;
            }
            else if (monster is Bat b)
            {
                b.canLunge.Value = data.CanLunge;
                b.hauntedSkull.Value = data.HauntedSkull;
                b.cursedDoll.Value = data.CursedDoll;
                if (data.SpeedMin > -1 && data.SpeedMax > -1)
                {
                    AccessTools.FieldRefAccess<Bat, float>(b, "maxSpeed") = Game1.random.Next(data.SpeedMin, data.SpeedMax + 1);
                }
                if (data.ExtraVelocity != null)
                {
                    AccessTools.FieldRefAccess<Bat, float>(b, "extraVelocity") = data.ExtraVelocity.Value;
                }
                if (data.HideShadow != null)
                {
                    b.HideShadow = data.HideShadow.Value;
                }
                if (data.ShakeTimer > -1)
                {
                    b.shakeTimer = data.ShakeTimer;
                }
                b.reloadSprite();
                return b;
            }
            else if (monster is BigSlime bs)
            {
                if (data.HeldItem != null)
                {
                    int totalWeight = data.HeldItem.Sum(i => i.Chance);
                    int roll = Game1.random.Next(0, totalWeight);
                    int cumulativeWeight = 0;
                    foreach (var item in data.HeldItem)
                    {
                        cumulativeWeight += item.Chance;
                        if (roll < cumulativeWeight)
                        {
                            bs.heldItem.Value = item.ItemId != null ? ItemRegistry.Create(item.ItemId, Game1.random.Next(item.MinQuantity, item.MaxQuantity + 1), item.Quality) : null;
                            break;
                        }
                    }
                }
                if (data.Color != null)
                {
                    bs.c.Value = MakeColor(data.Color);
                }
                return bs;
            }
            else if (monster is Bug bug)
            {
                if (data.Armored != null)
                {
                    bug.isArmoredBug.Value = data.Armored.Value;
                    return bug;
                }
            }
            else if (monster is DinoMonster dino)
            {
                if (data.ProjectileIndex != null || data.ProjectileSprite != null || data.ProjectileSource != null)
                {
                    for (int i = 0; i < dino.projectiles.Length; i++)
                    {
                        dino.projectiles[i] = new CustomBreathProjectile(data);
                    }
                }
                return dino;
            }
            else if (monster is Fly fly)
            {
                fly.HideShadow = data.HideShadow ?? true;
                return fly;
            }
            else if (monster is GreenSlime gs)
            {
                if (data.Color != null)
                {
                    gs.color.Value = MakeColor(data.Color);
                }
                if (data.MinStacks > -1 && data.MaxStacks > -1)
                {
                    gs.stackedSlimes.Value = Game1.random.Next(data.MinStacks, data.MaxStacks + 1);
                }
                if (data.HideShadow != null)
                {
                    gs.HideShadow = data.HideShadow.Value;
                }
                return gs;
            }
            else if (monster is MetalHead mh)
            {
                if (data.Color != null)
                {
                    mh.c.Value = MakeColor(data.Color);
                }
                return mh;
            }
            else if (monster is Serpent s)
            {
                if (data.MinSegment != null && data.MaxSegment != null)
                {
                    s.segmentCount.Value = Game1.random.Next(data.MinSegment.Value, data.MaxSegment.Value + 1);
                    s.reloadSprite();
                }
                return s;
            }
            else if (monster is Shooter sh)
            {
                if (data.ProjectileIndex != null)
                {
                    sh.firedProjectile = data.ProjectileIndex.Value;
                }
                if (data.ProjectileDebuff != null)
                {
                    sh.projectileDebuff = data.ProjectileDebuff;
                }
                if (data.AimTime != null)
                {
                    sh.aimTime = data.AimTime.Value;
                }
                if (data.AimEndTime != null)
                {
                    sh.aimEndTime = data.AimEndTime.Value;
                }
                if (data.BurstTime != null)
                {
                    sh.burstTime = data.BurstTime.Value;
                }
                if (data.ShotsPerFire != null)
                {
                    sh.numberOfShotsPerFire = data.ShotsPerFire.Value;
                }
                if (data.ProjectileRange != null)
                {
                    sh.projectileRange = data.ProjectileRange.Value;
                }
                if (data.DesiredDistance != null)
                {
                    sh.desiredDistance = data.DesiredDistance.Value;
                }
                if (data.FireRange != null)
                {
                    sh.fireRange = data.FireRange.Value;
                }
                if (data.ProjectileSound != null)
                {
                    sh.fireSound = data.ProjectileSound;
                }
                if (data.DamageSound != null)
                {
                    sh.damageSound = data.DamageSound;
                }
                return sh;
            }
            return monster;
        }
    }

}
