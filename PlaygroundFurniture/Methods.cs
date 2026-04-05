using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PlaygroundMod
{
    public partial class ModEntry
    {
        public static bool CanBePlaygrounding(Farmer who)
        {
            if (who.currentLocation is Town)
                return true;
            if (!who.IsSitting())
                return false;
            if (who.sittingFurniture is Furniture f && f.ItemId.StartsWith(furniturePrefix))
                return true;
            return false;
        }

        private static bool IsSwinging(Farmer who)
        {
            if (!who.IsSitting())
                return false;
            if (who.currentLocation is Town && (who.TilePoint == new Point(15, 12) || who.TilePoint == new Point(17, 12)))
                return true;
            if (who.sittingFurniture is Furniture f && f.ItemId == furniturePrefix + "Swings")
                return true;
            return false;
        }
    }
}