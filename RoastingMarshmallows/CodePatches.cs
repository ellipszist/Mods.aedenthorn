using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using System;

namespace RoastingMarshmallows
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw), [typeof(SpriteBatch)])]
        public static class Farmer_draw_Patch
        {
            public static void Postfix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || Game1.eventUp)
                    return;
                int roastProgress = GetRoastProgress(__instance, false, false);
                if (roastProgress < 0)
                    return;
                if(roastProgress >= Config.RoastFrames)
                {
                    __instance.modData.Remove(modKey);
                    return;
                }
                var flip = (__instance.flip || (__instance.Sprite.CurrentAnimation != null && __instance.Sprite.CurrentAnimation[__instance.Sprite.currentAnimationIndex].flip));
                var texture = SHelper.GameContent.Load<Texture2D>(texturePath);
                var offset = flip ? new Vector2(-24, 8) : new Vector2(64, 8);
                var position = __instance.getLocalPosition(Game1.viewport) + offset;
                b.Draw(texture, position, new(0,0,texture.Height, texture.Height), Color.White, __instance.rotation, new Vector2((float)(__instance.Sprite.SpriteWidth / 2), (float)__instance.Sprite.SpriteHeight * 3f / 4f), 2, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, ((float)__instance.StandingPixel.Y / 10000f)) + 1 / 10000f);
                int intervalLength = Config.RoastFrames / (texture.Width / texture.Height);
                int frame = roastProgress / intervalLength;

                if(frame > 0)
                {
                    Rectangle sourceRect = new(frame * texture.Height, 0, texture.Height, texture.Height);
                    b.Draw(texture, position, sourceRect, Color.White, __instance.rotation, new Vector2((float)(__instance.Sprite.SpriteWidth / 2), (float)__instance.Sprite.SpriteHeight * 3f / 4f), 2f, (__instance.flip || (__instance.Sprite.CurrentAnimation != null && __instance.Sprite.CurrentAnimation[__instance.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, ((float)__instance.StandingPixel.Y / 10000f)) + 2 / 10000f);
                    int frames = (texture.Width / texture.Height) - 2;
                    if (frame > 1 && roastProgress % (int)(50f * frames / frame) == 0)
                    {
                        TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), __instance.Position + offset + (flip ? new Vector2(-texture.Height / 2 - 4, -texture.Height - 16) : new Vector2(texture.Height / 2 + 4, -texture.Height - 16)), false, 0.002f, Color.Gray);
                        sprite.alpha = 0.5f * frame / frames;
                        sprite.motion = new Vector2(0f, -0.5f);
                        sprite.acceleration = new Vector2(0.002f, 0f);
                        sprite.interval = 99999f;
                        sprite.layerDepth = 1f;
                        sprite.scale = 1;
                        sprite.scaleChange = 0.02f;
                        sprite.rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f;
                        __instance.currentLocation.temporarySprites.Add(sprite);
                    }
                }
            }
        }
    }
}
