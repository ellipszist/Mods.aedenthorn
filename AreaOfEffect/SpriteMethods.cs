using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using System;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace AreaOfEffect
{
    public partial class ModEntry
    {
        public static void CreateIce(GameLocation l, Vector2 position)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var t = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(118, 227, 16, 13), position + new Vector2(0, 9), false, 0f, Color.White)
            {
                layerDepth = (float)(position.Y + 33) / 10000f,
                animationLength = 1,
                interval = 2000f,
                scale = 4f
            };
            l.TemporarySprites.Add(t);
        }
        public static void CreateFire(GameLocation l, Vector2 position)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), position + new Vector2(32f, -32f) + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-16, 16)), false, 0f, Color.White)
            {
                interval = 30f,
                totalNumberOfLoops = 99999,
                animationLength = 4,
                scale = 4f,
                alphaFade = 0.01f
            };
            l.TemporarySprites.Add(t);
        }
        public static void CreateBurn(GameLocation l, Vector2 tile, object o)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), tile * 64 + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-16, 16)), false, 0f, Color.White)
            {
                interval = 30f,
                totalNumberOfLoops = 15,
                animationLength = 4,
                scale = 4f
            };
            Game1.delayedActions.Add(new(1000, () =>
            {
                l.playSound("fireball", tile);
                DestroyAt(l, tile, o);
            }));
            l.TemporarySprites.Add(t);
        }
        public static void CreateSmoke(GameLocation l, Vector2 position)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), position + new Vector2(32f) + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-32, 16)), false, 0.002f, Color.White)
            {
                alphaFade = 0.0043333336f,
                alpha = 0.75f,
                motion = new Vector2((float)Game1.random.Next(-10, 11) / 20f, -1f),
                acceleration = new Vector2(0f, 0f),
                interval = 99999f,
                layerDepth = 1f,
                scale = 3f,
                scaleChange = 0.01f,
                rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f
            };
            l.TemporarySprites.Add(t);
        }
        public static void CreateSparkle(GameLocation l, Rectangle r, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            l.TemporarySprites.AddRange(Utility.sparkleWithinArea(r, data.Number, data.Color, (int)data.Interval, data.Delay, ""));
        }
        public static void CreateHeart(GameLocation l, Vector2 position, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(211, 428, 7, 6), 2000f, 1, 0, position, false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
            {
                motion = new Vector2(0f, -0.5f),
                alphaFade = 0.01f
            };
            l.TemporarySprites.Add(t);
        }
    }
}