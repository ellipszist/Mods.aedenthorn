using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
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
                if (SHelper.GameContent.Load<Dictionary<string, PrismaticData>>(dictPath).TryGetValue(isCrop ? cr.indexOfHarvest.Value : co.ItemId, out var data))
                {
                    if(data.Color1 != null && data.Color2 != null)
                    {
                        return Utility.Get2PhaseColor(Utility.StringToColor(data.Color1).Value, Utility.StringToColor(data.Color2).Value, data.Offset, data.Speed, data.TimeOffset);
                    }
                    return Utility.GetPrismaticColor(data.Offset, data.Speed);
                }
                int offset = 0;
                float speed = isCrop ? Config.CropSpeed : Config.ObjectSpeed;
                if (isCrop)
                {
                    switch (Config.CropPattern)
                    {
                        case PrismaticPattern.Random:
                            offset = int.TryParse(val, out var i) ? i : offset;
                            break;
                        case PrismaticPattern.Down:
                            offset = Utility.PRISMATIC_COLORS.Length - ((int)cr.tilePosition.Y % Utility.PRISMATIC_COLORS.Length);
                            break;
                        case PrismaticPattern.Up:
                            offset = (int)cr.tilePosition.Y;
                            break;
                        case PrismaticPattern.Right:
                            offset = Utility.PRISMATIC_COLORS.Length - ((int)cr.tilePosition.X % Utility.PRISMATIC_COLORS.Length);
                            break;
                        case PrismaticPattern.Left:
                            offset = (int)cr.tilePosition.X;
                            break;
                        default:
                            break;
                    }
                }
                return Utility.GetPrismaticColor(offset, speed);
            }
            return color;
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
