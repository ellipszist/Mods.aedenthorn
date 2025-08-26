using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace FreeLove
{
    public static class Game1Patches
    {
        private static IMonitor Monitor;
        public static string lastGotCharacter = null;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static void getCharacterFromName_Prefix(string name)
        {
            if (EventPatches.startingLoadActors)
                lastGotCharacter = name;
        }
        public static void OnDayStarted_Postfix()
        {
            if (Game1.IsMasterGame)
            {
                foreach (var f in Game1.getAllFarmers())
                {
                    ModEntry.PlaceSpousesInFarmhouse(Game1.RequireLocation<FarmHouse>(f.homeLocation.Value, false), f);
                }
            }

        }
    }
}
