using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace FlowerColors
{
    public partial class ModEntry
    {
        private bool TryGetColoredCrop(out Crop crop)
        {
            crop = Game1.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out var tf) && tf is HoeDirt dirt && dirt.crop is Crop acrop && acrop.programColored.Value ? acrop : null;
            return crop != null;
        }

        public static Color? GetNewColor(List<string> tintColors, Color oldColor, int delta)
        {
            if (tintColors == null)
                return null;
            var color = ColorToHexString(oldColor);
            int which = tintColors.IndexOf(color);
            if (which == -1)
                which = 0;
            which += Math.Sign(delta);
            if (which < 0)
                which = tintColors.Count - 1;
            else if (which >= tintColors.Count)
                which = 0;
            return Utility.StringToColor(tintColors[which]);
        }

        private static string ColorToHexString(Color value)
        {
            return $"#{value.R:X2}{value.G:X2}{value.B:X2}";
        }
    }
}