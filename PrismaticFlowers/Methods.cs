using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace PrismaticFlowers
{
    public partial class ModEntry : Mod
    {
        public static Color GetPrismaticColor(Color color, object obj)
        {
            if (!Config.ModEnabled)
                return color;
            ColoredObject co = obj as ColoredObject;
            Crop cr = obj as Crop;
            if (co?.modData.TryGetValue(prismaticKey, out var val) == true|| cr?.modData.TryGetValue(prismaticKey, out val) == true)
            {
                bool isCrop = cr != null;
                int offset = 0;
                float speed = isCrop ? Config.CropSpeed : Config.ObjectSpeed;
                if (SHelper.GameContent.Load<Dictionary<string, PrismaticData>>(dictPath).TryGetValue(isCrop ? cr.indexOfHarvest.Value : co.ItemId, out var data))
                {
                    offset = data.Offset;
                    if (isCrop && data.CropType != null && Enum.TryParse(typeof(PrismaticPattern), data.CropType, out var pattern))
                    {
                        offset = GetOffset((PrismaticPattern)pattern, cr, val, offset);
                    }
                    if (!isCrop && data.ObjectType != null && Enum.TryParse(typeof(PrismaticPattern), data.ObjectType, out pattern))
                    {
                        offset = GetOffset((PrismaticPattern)pattern, cr, val, offset);
                    }
                    if(data.Color1 != null && data.Color2 != null)
                    {
                        return Utility.Get2PhaseColor(Utility.StringToColor(data.Color1).Value, Utility.StringToColor(data.Color2).Value, offset, 1, data.TimeOffset);
                    }
                    return Utility.GetPrismaticColor(offset, data.Speed);
                }
                if (isCrop)
                {
                    GetOffset(Config.CropPattern, cr, val, offset);
                }
                return Utility.GetPrismaticColor(offset, speed);
            }
            return color;
        }

        private static int GetOffset(PrismaticPattern pattern, Crop cr, string val, int fallback)
        {
            switch (pattern)
            {
                case PrismaticPattern.Random:
                    return int.TryParse(val, out var i) ? i : fallback;
                case PrismaticPattern.Down:
                    return Utility.PRISMATIC_COLORS.Length - ((int)cr.tilePosition.Y % Utility.PRISMATIC_COLORS.Length);
                case PrismaticPattern.Up:
                    return (int)cr.tilePosition.Y;
                case PrismaticPattern.Right:
                    return Utility.PRISMATIC_COLORS.Length - ((int)cr.tilePosition.X % Utility.PRISMATIC_COLORS.Length);
                case PrismaticPattern.Left:
                    return (int)cr.tilePosition.X;
                default:
                    return fallback;
            }
        }

        public static ColoredObject CheckPrismaticHarvest(ColoredObject obj, Crop crop)
        {
            if (!Config.ModEnabled)
                return obj;
            if (crop.modData.TryGetValue(prismaticKey, out var val))
            {
                obj.modData[prismaticKey] = val;
            }
            return obj;
        }
    }
}
