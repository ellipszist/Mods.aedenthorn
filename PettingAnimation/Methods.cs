using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using System;

namespace PettingAnimation
{
    public partial class ModEntry
    {
        public static Vector2 GetOffset(string str)
        {
            int amount = 32;
            return str switch
            {
                "1" => new(0, -amount),
                "2" => new(amount, 0),
                "3" => new(0, amount),
                "4" => new(-amount, 0),
                _ => new(0, 0),
            };
        }
        public static void PetPet(Farmer who)
        {
            who.modData[facingKey] = (who.FacingDirection + 1).ToString();
            int which = who.FacingDirection switch
            {
                0 => FarmerSprite.swordswipeUp,
                1 => FarmerSprite.swordswipeRight,
                2 => FarmerSprite.swordswipeDown,
                _ => FarmerSprite.swordswipeLeft,
            };
            who.animateOnce(which);
            int speed = 300;
            who.FarmerSprite.timer = speed;
            for (int i = 0; i < who.FarmerSprite.currentAnimation.Count; i++)
            {
                var f = who.FarmerSprite.currentAnimation[i];
                who.FarmerSprite.currentAnimation[i] = new FarmerSprite.AnimationFrame(f.frame, speed, f.positionOffset)
                {
                    flip = f.flip,
                    armOffset = f.armOffset,
                    xOffset = f.xOffset,
                    frameEndBehavior = i == who.FarmerSprite.currentAnimation.Count - 1 ? new AnimatedSprite.endOfAnimationBehavior((Farmer f) =>
                    {
                        f.modData.Remove(facingKey);
                    }) : null
                };
            }
        }

    }
}