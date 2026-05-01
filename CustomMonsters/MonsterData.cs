using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CustomMonsters
{
    public class MonsterData
    {
        public string Type { get; set; }
        public string Parameters { get; set; } = "position";
        public int Level { get; set; }
        public int Facing { get; set; }
        public string Color { get; set; }
        public string Name { get; set; }
        public bool Switch { get; set; }
        public string Sprite { get; set; }

        public float Scale { get; set; } = -1;
        public int Damage { get; set; } = -1;
        public bool HardMode { get; set; }
        public int Health { get; set; } = -1;
        public int Speed { get; set; } = -1;
        public int Experience { get; set; } = -1;
        public int Resilience { get; set; } = -1;

        public int Slipperiness { get; set; } = -1;
        public int LightType { get; set; } = -1;

        public bool? HideShadow { get; set; }
        public int ShakeTimer { get; set; } = -1;
        public List<CustomDropData> Drops { get; set; }

        public string MoveSound { get; set; }
        public string MoveSound2 { get; set; }
        public string SpawnSound { get; set; }
        public string DamageSound { get; set; }
        public string DamageSound2 { get; set; }
        public string DeathSound { get; set; }
        public string DeathSound2 { get; set; }

        public string ProjectileSound { get; set; }
        public string ProjectileSound2 { get; set; }
        public int? ProjectileIndex { get; set; }
        public string ProjectileDebuff { get; set; }
        public Rectangle? ProjectileSource { get; set; }
        public string ProjectileSprite { get; set; }
        public float? ProjectileScale { get; set; }
        public int? ProjectileDamage { get; set; }
        public int? ProjectileRange { get; set; }

        public List<MonsterSpawnData> MineSpawns { get; set; }
        public List<MonsterSpawnData> VolcanoSpawns { get; set; }

        //Angry Roger specific
        public string SprinkleColor { get; set; }


        // bat specific
        public bool CanLunge { get; set; } = false;
        public bool CursedDoll { get; set; } = false;
        public bool HauntedSkull { get; set; } = false;
        public int SpeedMin { get; set; } = -1;
        public int SpeedMax { get; set; } = -1;
        public float? ExtraVelocity { get; set; } = null;

        // big slime specific
        public List<CustomDropData> HeldItem { get; set; }

        // bug specific
        public string ArmorSound { get; set; }
        public bool? Armored { get; set; }

        // Mummy specific
        public string CrumbleSound { get; set; }
        public string UncrumbleSound { get; set; }
        public int? ReviveTimer { get; set; }

        // RockCrab specific
        public int? WaiterChance { get; set; }
        public string ShellSound { get; set; }
        public string BreakSound { get; set; }
        public bool StickBug { get; set; }

        // Serpent specific
        public int? MinSegment { get; set; }
        public int? MaxSegment { get; set; }


        // Shooter specific
        public int? ProjectileSpeed { get; set; }
        public int? ShotsPerFire { get; set; }
        public float? AimTime { get; set; }
        public float? BurstTime { get; set; }
        public float? AimEndTime { get; set; }
        public int? DesiredDistance { get; set; }
        public int? FireRange { get; set; }

        // Spiker specific
        public bool? Vulnerable { get; set; }

        // SquidKid specific
        public int? ProjectileCount { get; set; }
        public float? ProjectileTimer { get; set; }

    }

    public class MonsterSpawnData
    {
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public int Chance { get; set; }
        public List<string> Types { get; set; }
    }

    public class CustomDropData
    {
        public string ItemId { get; set; }
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public int Quality { get; set; }
        public int Chance { get; set; } = 100;
    }
}