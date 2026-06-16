using StardewValley;
using System;

namespace Clocks
{
    public partial class ModEntry
    {
        public static float GetHourRotation(float value = 0)
        {
            if (value != 0 && (!Config.ModEnabled || !Config.FixClocks))
                return value;
            float hours = (float)(Game1.timeOfDay % 1200 / 100 + (Game1.timeOfDay % 100 / 10 + (Config.SmoothMovement ? ((float)Game1.gameTimeInterval / Game1.realMilliSecondsPerGameTenMinutes) : 0)) / 6f);

            return (float)(Math.PI / 6 * hours);
        }

        public static float GetMinRotation(float value = 0)
        {
            if (value != 0 && (!Config.ModEnabled || !Config.FixClocks))
                return value;
            float mins = Game1.timeOfDay % 100 + (Config.SmoothMovement ? ((float)Game1.gameTimeInterval / Game1.realMilliSecondsPerGameTenMinutes) : 0) * 10;
            return (float)(Math.PI / 30 * mins);
        }
    }
}