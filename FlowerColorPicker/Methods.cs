using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FlowerColorPicker
{
    public partial class ModEntry
    {
        public static Color? GetNewColor(List<string> tintColors, Color oldColor, int delta)
        {
            if (tintColors == null)
                return null;
            var color = ColorToHexString(oldColor);
            int which = tintColors.IndexOf(color);
            if (which == -1)
                return null;
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