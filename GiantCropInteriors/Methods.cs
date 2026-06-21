using StardewModdingAPI;
using StardewValley;

namespace GiantCropInteriors
{
    public partial class ModEntry
    {
        public static void RemoveAllBuildings()
        {
            if (!Context.IsWorldReady)
                return;
            Utility.ForEachLocation((GameLocation l) =>
            {
                for(int i = l.buildings.Count - 1; i >= 0; i--)
                {
                    if (l.buildings[i]?.modData.ContainsKey(cropKey) == true)
                    {
                        l.buildings.RemoveAt(i);
                    }
                }
                return true;
            });
        }
    }
}