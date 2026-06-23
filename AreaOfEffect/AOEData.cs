using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AreaOfEffect
{
    public enum SpellDirection
    {
        Up, 
        UpRight, 
        Right,
        DownRight,
        Down, 
        DownLeft, 
        Left,
        UpLeft,
        None
    }
    public enum SpriteType
    {
        Ice,
        Fire,
        Smoke,
        Sparkle,
        Heart
    }
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
        Twig,
        Weed,
        Tree,
        FruitTree,

    }
    public enum AOEEffectType
    {
        Heal,
        Damage,
        Destroy,
        Burn,
        Freeze,
        Water,
        Fertilize,
        Grow,
        Explode,
        Buff,
        Index,
        TileSheet,
        Property,
        ModData,
        Transform,
        Custom
    }
    public class AOEToolData
    {
        public int MaxCharges { get; set; }
        public string RechargeItem { get; set; }
        public int RechargeAmount { get; set; } = 1;
        public int MaxDistance { get; set; }
        public string RechargeSound { get; set; }
        public string Type { get; set; }
        public Color ChargeColor { get; set; } = Color.White;
    }
    public class AOESpellData
    {
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public string Sound { get; set; }
        public List<SpellDirection> Sequence { get; set; }
    }
    public class AOEEffectData
    {
        public string CastSound { get; set; }
        public int Radius { get; set; }
        public ProjectileData Projectile { get; set; }
        public List<SpriteData> Sprites { get; set; }
        public List<AOEEffect> Effects { get; set; }
    }

    public class SpriteData
    {
        public SpriteType Type { get; set; }
        public bool PerTile { get; set; } = true;
        public string Texture { get; set; }
        public Rectangle SourceRect { get; set; }
        public Rectangle? Rect { get; set; }
        public float Alpha { get; set; }
        public Color Color { get; set; } = Color.White;
        public int? Index { get; set; }
        public float? Interval { get; set; }
        public int Length { get; set; }
        public int Loops { get; set; }
        public Vector2 Offset { get; set; }
        public Vector2 Acceleration { get; set; }
        public int YStop { get; set; }
        public bool Flicker { get; set; }
        public bool? Flipped { get; set; }
        public bool FlippedRandom { get; set; }
        public bool FlippedVertical { get; set; }
        public float Rotation { get; set; }
        public float RotationChange { get; set; }
        public int Row { get; set; }
        public int SourceWidth { get; set; }
        public int SourceHeight { get; set; }
        public float LayerDepth { get; set; }
        public float Scale { get; set; }
        public float ScaleChange { get; set; }
        public int Delay { get; set; }
        public int Number { get; set; }
        public bool DrawAbove { get; set; }
    }

    public class ProjectileData
    {
    }

    public class AOEEffect
    {
        public List<AOEAffectedType> Affected { get; set; }
        public AOEEffectType EffectType { get; set; }
        public object Value { get; set; }
    }
}