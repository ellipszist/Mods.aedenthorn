using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DoorFurniture
{
    public partial class ModEntry
    {
        public static bool IsDoor(Furniture f)
        {
            return f != null && SHelper.GameContent.Load<Dictionary<string, DoorData>>(dictPath).ContainsKey(f.ItemId);
        }
        public static bool TryGetDoorData(Furniture f, out DoorData data)
        {
            data = null;
            if(f == null || !SHelper.GameContent.Load<Dictionary<string, DoorData>>(dictPath).TryGetValue(f.ItemId, out data))
                return false;
            return true;
        }

    }
}