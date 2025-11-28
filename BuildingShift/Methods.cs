using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using System;

namespace BuildingShift
{
    public partial class ModEntry
    {

        public static bool TryGetShift(Building building, out Vector2 shift)
        {
            shift = Vector2.Zero;
            if (!building.modData.TryGetValue(shiftKey, out var shiftString))
                return false;
            string[] split = shiftString.Split(',');
            if (split.Length != 2 || !int.TryParse(split[0], out var x) || !int.TryParse(split[1], out var y))
                return false;
            shift = new Vector2(x, y);
            return true;
        }
        public static Vector2 ShiftVector(Vector2 vector, Building building)
        {
            if(!Config.EnableMod || !TryGetShift(building, out var amount)) 
                return vector;
            return vector + amount * 4;
        }

        public void ShiftBuilding(Building b, int x, int y)
        {
            if (SHelper.Input.IsDown(Config.ModKey))
            {
                x *= Config.ShiftAmount;
                y *= Config.ShiftAmount;
            }
            TryGetShift(b, out var shift);
            shift += new Vector2(x, y);
            SMonitor.Log($"{b.buildingType.Value} is shifted by {x},{y}");
            b.modData[shiftKey] = $"{shift.X},{shift.Y}";
        }
    }
}