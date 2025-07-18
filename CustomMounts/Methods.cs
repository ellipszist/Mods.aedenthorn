using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;

namespace CustomMounts
{
    public partial class ModEntry
    {

        private static void DrawHat(Hat hat, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction, bool useAnimalTexture, Horse horse)
        {
            if (!Config.ModEnabled || !horse.modData.TryGetValue(modKey, out var key) || !mountDict.TryGetValue(key, out var data))
            {

            }
            else
            {
                scaleSize = (int)Math.Round(scaleSize * data.HatScale);
                location += data.HatOffsets[horse.FacingDirection];
            }
            hat.draw(spriteBatch, location, scaleSize, transparency, layerDepth, direction, useAnimalTexture);

        }


        private static void SetSprite(Horse horse, MountData data)
        {
            horse.Sprite = new AnimatedSprite(data.TexturePath, 0, data.FrameWidth, data.FrameHeight);
            horse.Sprite.textureUsesFlippedRightForLeft = true;
            horse.Sprite.loop = true;
            horse.spriteOverridden = true;
        }

        private static int SetSpeed(int speed, Farmer farmer)
        {

            if (!Config.ModEnabled || !farmer.mount.modData.TryGetValue(modKey, out var key) || !mountDict.TryGetValue(key, out var data))
                return speed;
            return data.Speed;
        }

        private static string SetStepSound(string which, Horse horse)
        {
            if (!Config.ModEnabled || !horse.modData.TryGetValue(modKey, out var key) || !mountDict.TryGetValue(key, out var data))
                return which;
            switch (which)
            {
                case "thudStep":
                    return data.FootstepSound ?? which;
                case "woodyStep":
                    return data.FootstepSoundWood ?? which;
                case "stoneStep":
                    return data.FootstepSoundStone ?? which;
                default:
                    return which;
            }
        }
    }
}