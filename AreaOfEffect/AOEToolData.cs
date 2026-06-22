using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AreaOfEffect
{
    public enum AOEAffectedType
    {
        Monster,
        NPC,
        Farmer,
        Tile,
        Object,
        ResourceClump,
        HoeDirt,
        Crop,
        Grass,
        Stone,
        Tree,
        FruitTree,

    }
    public enum AOEEffectType
    {
        Heal,
        Damage,
        Water,
        Fertilize,
        Grow,
        Explode,
        Debuff,
        Buff,
        Index,
        TileSheet,
        Property,
        ModData,
        Custom
    }
    public class AOEToolData
    {
        public int MaxCharges { get; set; }
        public string RechargeItem { get; set; }
        public int RechargeAmount { get; set; } = 1;
        public int MaxDistance { get; set; }
        public string CastSound { get; set; } 
        public string RechargeSound { get; set; } 
        public ProjectileData Projectile { get; set; }
        public List<SpriteData> Sprites { get; set; }
        public List<AOEEffect> Effects { get; set; }
    }

    public class SpriteData
    {
        public bool PerTile { get; set; }

    }

    public class ProjectileData
    {
    }

    public class AOEEffect
    {
        public AOEAffectedType Affected { get; set; }
        public AOEEffectType EffectType { get; set; }
    }
}