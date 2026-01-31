using StardewModdingAPI;

namespace MapTokens
{
	public partial class ModEntry : Mod
    {
        public static void NotifyMapChanged()
        {
            MapPropertyTile.changed = true;
            MapDefaultArrivalTile.changed = true;
        }
    }
}
