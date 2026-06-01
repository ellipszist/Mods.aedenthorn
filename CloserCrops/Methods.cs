using StardewValley;
using StardewValley.Extensions;
using Object = StardewValley.Object;

namespace CloserCrops
{
    public partial class ModEntry
    {
        public static bool IsMiniCrop(Crop crop)
        {
            return crop != null && Config.CloserCrops.Contains(crop.netSeedIndex.Value) && (!crop.forageCrop.Value || crop.whichForageCrop.Value != "2");
        }
        public static bool IsCloserCropseed(Object obj)
        {
            return obj != null && obj.HasTypeObject() && obj.Category == Object.SeedsCategory && Config.CloserCrops.Contains(obj.ItemId);
        }
        public static float CheckScale(float value, Crop crop)
        {
            return value;
            if(!Config.ModEnabled || !IsMiniCrop(crop) || crop.currentPhase.Value == 0)
                return value;
            return 1f;
        }
    }
}