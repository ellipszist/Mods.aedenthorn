using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.GameData.Pets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomBouquets
{
    public partial class ModEntry
    {
        private static Color GetColor(string color)
        {
            string[] bytes = color.Split(',');
            return new Color(byte.Parse(bytes[0]), byte.Parse(bytes[1]), byte.Parse(bytes[2]));
        }
    }
}