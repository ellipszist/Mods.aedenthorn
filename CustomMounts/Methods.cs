using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Objects;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace CustomMounts
{
    public partial class ModEntry
    {

        private static void DrawHat(Hat hat, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction, bool useAnimalTexture, Horse horse)
        {
            if (!Config.ModEnabled || !horse.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data))
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

            if (!Config.ModEnabled || !farmer.mount.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data))
                return speed;
            return data.Speed;
        }
        private static float SetSpeedBonus(float speedBonus, Farmer farmer)
        {

            if (!Config.ModEnabled || farmer.mount is null || !farmer.mount.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data))
                return speedBonus;
            return data.EatSpeedBonus;
        }

        private static string SetStepSound(string which, Horse horse)
        {
            if (!Config.ModEnabled || !horse.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data))
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
        private static string SetCarrotItem(string item, Horse horse)
        {
            if (!Config.ModEnabled || !horse.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data))
                return item;
            return data.EatItem;
        }
        private static string SetEatSound(string sound, Horse horse)
        {
            if (!Config.ModEnabled || !horse.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data))
                return sound;
            return data.EatSound;
        }
        private static string SetFluteItem(string value, Object obj)
        {
            if (!Config.ModEnabled)
                return value;
            Utility.ForEachBuilding<Stable>(delegate (Stable stable)
            {
                Horse curHorse = stable.getStableHorse();
                var owner = curHorse?.getOwner();
                var ownerId = curHorse?.ownerId;
                if (owner == Game1.player)
                {
                    if(curHorse.modData.TryGetValue(modKey, out var key) && MountDict.TryGetValue(key, out var data))
                    {
                        if(obj.QualifiedItemId == data.FluteItem)
                        {
                            value = data.FluteItem;
                            return false;
                        }
                    }
                }
                return true;
            }, true);
            return value;
        }
        private static string SetFluteSound(string value, Object obj)
        {
            if (!Config.ModEnabled)
                return value;
            Utility.ForEachBuilding<Stable>(delegate (Stable stable)
            {
                Horse curHorse = stable.getStableHorse();
                if (curHorse?.getOwner() == Game1.player)
                {
                    if (curHorse.modData.TryGetValue(modKey, out var key) && MountDict.TryGetValue(key, out var data))
                    {
                        if (obj.QualifiedItemId == data.FluteItem)
                        {
                            value = data.FluteSound;
                            return false;
                        }
                    }
                }
                return true;
            }, true);
            return value;
        }
        private static Horse GetHorseForFlute(long uid)
        {
            Farmer farmer = Game1.GetPlayer(uid, false);
            Object obj = farmer.ActiveObject;
            Horse horse = null;
            Utility.ForEachBuilding<Stable>(delegate (Stable stable)
            {
                Horse curHorse = stable.getStableHorse();
                if (curHorse != null && curHorse.getOwner() == farmer && (!Config.ModEnabled || IsFluteFor(curHorse, obj)))
                {
                    horse = curHorse;
                    return false;
                }
                return true;
            }, true);
            return horse;
        }

        private static bool IsFluteFor(Horse horse, Object obj)
        {
            if (!horse.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data))
                return obj.QualifiedItemId == "(O)911";
            return obj.QualifiedItemId == data.FluteItem;
        }

        private static void PreventOwnershipErasure(NetLong l, long v)
        {
            if (!Config.ModEnabled || !Config.AllowMultipleMounts)
                l.Value = v;
        }
    }
}