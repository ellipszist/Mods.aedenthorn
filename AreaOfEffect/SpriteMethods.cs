using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;

namespace AreaOfEffect
{
    public partial class ModEntry
    {
        public static void CreateIce(GameLocation l, Vector2 position, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var t = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(118, 227, 16, 13), position + new Vector2(0, 9), false, 0f, Color.White)
            {
                layerDepth = (float)(position.Y + 33) / 10000f,
                animationLength = 1,
                interval = 2000f,
                scale = 4f,
                acceleration = data.Acceleration,
                alpha = data.Alpha,
                alphaFade = data.AlphaFade,
                drawAboveAlwaysFront = data.DrawAbove,
                motion = data.Motion,
                rotation = data.Rotation,
                rotationChange = data.RotationChange,
                totalNumberOfLoops = data.Loops,
                yStopCoordinate = data.YStop,
            };
            l.TemporarySprites.Add(t);
        }
        public static void CreateFire(GameLocation l, Vector2 position, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), position + new Vector2(32f, -32f) + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-16, 16)), false, 0f, Color.White)
            {
                interval = 30f,
                totalNumberOfLoops = 99999,
                animationLength = 4,
                scale = 4f,
                alphaFade = 0.01f,
                acceleration = data.Acceleration,
                alpha = data.Alpha,
                drawAboveAlwaysFront = data.DrawAbove,
                motion = data.Motion,
                rotation = data.Rotation,
                rotationChange = data.RotationChange,
                yStopCoordinate = data.YStop,
            };
            l.TemporarySprites.Add(t);
        }
        public static void CreateBurn(GameLocation l, Vector2 tile, object o, SpriteData data)
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
            if(data != null)
            {
                t.delayBeforeAnimationStart = data.Delay;
                t.interval = data.Interval;
                t.totalNumberOfLoops = data.Loops;
                t.animationLength = data.Length; 
                t.scale = data.Scale ?? 4f;
            }
            Game1.delayedActions.Add(new(1000, () =>
            {
                l.playSound("fireball", tile);
                DestroyAt(l, tile, o);
            }));
            l.TemporarySprites.Add(t);
        }
        public static void CreateSmoking(GameLocation l, Vector2 position, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), position + new Vector2(32f) + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-32, 16)), false, 0.002f, Color.White)
            {
                alphaFade = 0.0043333336f,
                alpha = 0.75f,
                delayBeforeAnimationStart = data.Delay,
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
        public static void CreatePoof(GameLocation l, Vector2 position, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var t = new TemporaryAnimatedSprite(5, position, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0);
            l.TemporarySprites.Add(t);
        }
        public static void CreateButterfly(GameLocation l, Vector2 position, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var rect = Game1.random.ChooseFrom(new Rectangle[] { 
                new(128, 96, 16, 16), new(192, 96, 16, 16), new(256, 96, 16, 16), new(192, 96, 16, 16), 
                new(128, 112, 16, 16), new(192, 112, 16, 16), new(256, 112, 16, 16), new(192, 112, 16, 16), 
                new(64, 288, 16, 16), new(128, 288, 16, 16), new(192, 288, 16, 16), new(256, 288, 16, 16), 
                new(128, 336, 16, 16), new(192, 336, 16, 16), 
                new(0, 384, 16, 16), new(64, 384, 16, 16) });
            var t = new TemporaryAnimatedSprite("TileSheets\\critters", rect, new Vector2(52.5f, 0f) * 64f - new Vector2(131.5f, -60f) * 4f, false, 0f, Color.White)
            {
                interval = 148f,
                animationLength = 4,
                pingPong = true,
                totalNumberOfLoops = 99999,
                scale = 4f,
                xPeriodic = true,
                xPeriodicRange = 32f,
                xPeriodicLoopTime = 2800f,
                alpha = 0.01f,
                alphaFade = -0.01f,
                yPeriodic = true,
                yPeriodicRange = 8f,
                yPeriodicLoopTime = 3800f
            };
            l.TemporarySprites.Add(t);
        }
        public static void CreateGlitter(GameLocation l, Vector2 position, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(432, 1435, 16, 16), 100f, 6, 99999, position + data.Offset, false, false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f, false)
            {
                acceleration = data.Acceleration,
                alpha = data.Alpha,
                alphaFade = data.AlphaFade,
                animationLength = data.Length,
                drawAboveAlwaysFront = data.DrawAbove,
                interval = data.Interval,
                layerDepth = data.LayerDepth,
                motion = data.Motion,
                rotation = data.Rotation,
                rotationChange = data.RotationChange,
                scale = data.Scale ?? 4f,
                totalNumberOfLoops = data.Loops,
                yStopCoordinate = data.YStop,
            };
            l.TemporarySprites.Add(t);
        }
        public static void CreateBalloon(GameLocation l, Vector2 position, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;

            int scale = Game1.random.Next(2, 5);
            l.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(424, 1266, 8, 8), 60f + (float)Game1.random.Next(-10, 10), 7, 999999, position + data.Offset, false, false, 0.99f, 0f, Color.White, (float)scale, 0f, 0f, 0f, false)
            {
                local = true,
                motion = new Vector2(0.1625f, -0.25f) * (float)scale,
                acceleration = data.Acceleration,
                alpha = data.Alpha,
                alphaFade = data.AlphaFade,
                animationLength = data.Length,
                drawAboveAlwaysFront = data.DrawAbove,
                interval = data.Interval,
                layerDepth = data.LayerDepth,
                rotation = data.Rotation,
                rotationChange = data.RotationChange,
                scale = data.Scale ?? 4f,
                totalNumberOfLoops = data.Loops,
                yStopCoordinate = data.YStop,
            }); 
        }
        public static void CreateEvilRabbit(GameLocation l, Vector2 position, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;

            l.temporarySprites.Add(new TemporaryAnimatedSprite
            {
                texture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters"),
                sourceRect = new Microsoft.Xna.Framework.Rectangle(264, 209, 19, 16),
                sourceRectStartingPos = new Vector2(264f, 209f),
                animationLength = 1,
                totalNumberOfLoops = 999,
                interval = 999f,
                scale = 4f,
                position = position + data.Offset,
                layerDepth = 1f,
                motion = new Vector2(-2f, -2f),
                acceleration = new Vector2(0f, 0.1f),
                yStopCoordinate = 204,
                xStopCoordinate = 316,
                flipped = true
            });
        }
        public static void CreateExplosion(GameLocation l, Vector2 position, float distance, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            if (Game1.random.NextDouble() < 0.45)
            {
                if (Game1.random.NextBool())
                {
                    l.temporarySprites.Add(new TemporaryAnimatedSprite(362, (float)Game1.random.Next(30, 90), 6, 1, position, false, Game1.random.NextBool())
                    {
                        delayBeforeAnimationStart = Game1.random.Next(700),
                        alpha = data.Alpha,
                        alphaFade = data.AlphaFade,
                        acceleration = data.Acceleration,
                        drawAboveAlwaysFront = data.DrawAbove,
                        motion = data.Motion,
                        rotation = data.Rotation,
                        rotationChange = data.RotationChange,
                        scale = data.Scale ?? 1,
                        yStopCoordinate = data.YStop,
                    });
                }
                else
                {
                    l.temporarySprites.Add(new TemporaryAnimatedSprite(5, position, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
                    {
                        delayBeforeAnimationStart = Game1.random.Next(200),
                        scale = (float)Game1.random.Next(5, 15) / 10f,
                        alpha = data.Alpha,
                        alphaFade = data.AlphaFade,
                        acceleration = data.Acceleration,
                        drawAboveAlwaysFront = data.DrawAbove,
                        motion = data.Motion,
                        rotation = data.Rotation,
                        rotationChange = data.RotationChange,
                        yStopCoordinate = data.YStop,
                    });
                }
            }
            l.temporarySprites.Add(new TemporaryAnimatedSprite(6, position, Color.White, 8, Game1.random.NextBool(), distance * 20f, 0, -1, -1f, -1, 0));
        }
        public static void CreateFountain(GameLocation l, Vector2 tile, Farmer who, SpellCastData data, SpriteData sdata)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            int id = (int)(tile.X * 4000f + tile.Y * 10);
            float layerDepth = 1;

            float scale =  data.Radius / 2f;
            var t = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 2176, 320, 320), 60f, 4, 20, tile * 64 + new Vector2(32f, 32f) + new Vector2(-160f, -160f) * scale, false, false)
            {
                color = Color.White * 0.4f,
                delayBeforeAnimationStart = sdata.Delay,
                id = id,
                layerDepth = layerDepth,
                scale = scale
            };
            t.endFunction = new((int id) =>
            {
                foreach (var e in data.Effects)
                {
                    List<object> applied = new();
                    foreach (var tile in GetTiles(tile, data.Radius))
                    {
                        ApplyEffectToTile(l, who, tile, e, applied);
                    }
                }
            });
            l.temporarySprites.Add(t);
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
                motion = data.Motion == Vector2.Zero ? new Vector2(0f, -0.5f) : data.Motion,
                alphaFade = data.AlphaFade == 0 ? 0.01f : data.AlphaFade,
                acceleration = data.Acceleration,
                alpha = data.Alpha,
                animationLength = data.Length,
                drawAboveAlwaysFront = data.DrawAbove,
                interval = data.Interval,
                layerDepth = data.LayerDepth,
                rotation = data.Rotation,
                rotationChange = data.RotationChange,
                scale = data.Scale ?? 4f,
                totalNumberOfLoops = data.Loops,
                yStopCoordinate = data.YStop,
            };
            l.TemporarySprites.Add(t);
        }
        public static void CreateTexture(GameLocation l, Vector2 position, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var t = GetTextureSprite(position, data);
            if (data.Bounce)
            {
                t.reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(t.bounce);
            }
            l.TemporarySprites.Add(t);
        }

        public static void CreateAnimation(GameLocation l, Vector2 position, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            var t = GetAnimationSprite(position, data);
            if (data.Bounce)
            {
                t.reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(t.bounce);
            }
            l.TemporarySprites.Add(t);
        }

        public static TemporaryAnimatedSprite GetTextureSprite(Vector2 position, SpriteData data)
        {
            return new TemporaryAnimatedSprite(data.Texture, data.SourceRect, position + data.Offset, data.Flipped ?? Game1.random.NextBool(), data.AlphaFade, data.Color)
            {
                acceleration = data.Acceleration,
                alpha = data.Alpha,
                alphaFade = data.AlphaFade,
                animationLength = data.Length,
                drawAboveAlwaysFront = data.DrawAbove,
                delayBeforeAnimationStart = data.Delay,
                interval = data.Interval,
                layerDepth = data.LayerDepth,
                motion = data.Motion,
                rotation = data.Rotation,
                rotationChange = data.RotationChange,
                scale = data.Scale ?? 4f,
                totalNumberOfLoops = data.Loops,
                yStopCoordinate = data.YStop,
            };
        }

        private static TemporaryAnimatedSprite GetAnimationSprite(Vector2 position, SpriteData data)
        {
            return new TemporaryAnimatedSprite(data.Index, position + data.Offset, data.Color, data.Length, data.Flipped ?? Game1.random.NextBool(), data.Interval, data.Loops, data.SourceWidth, data.LayerDepth, data.SourceHeight, data.Delay)
            {
                alpha = data.Alpha,
                alphaFade = data.AlphaFade,
                acceleration = data.Acceleration,
                drawAboveAlwaysFront = data.DrawAbove,
                motion = data.Motion,
                rotation = data.Rotation,
                rotationChange = data.RotationChange,
                scale = data.Scale ?? 1,
                yStopCoordinate = data.YStop,
                delayBeforeAnimationStart = data.Delay,
            };
        }

    }
}