using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wildflowers
{
    public partial class ModEntry
    {
        private static string GetRandomFlowerSeed(string[] flowers)
        {
            var crops = Game1.cropData;
            var idxs = new List<string>();
            foreach(var kvp in crops)
            {
                if (!flowers.Contains(kvp.Value.HarvestItemId))
                    continue;
                if (!kvp.Value.Seasons.Contains(Game1.season))
                    continue;
                idxs.Add(kvp.Key);
            }
            if(idxs.Count > 0)
                return ""+idxs[Game1.random.Next(idxs.Count)];
            return null;
        }
        private static bool IsCropDataInvalid(Crop crop, CropData cropData)
        {
            return (!string.IsNullOrEmpty(cropData.harvestName) && Game1.objectData.TryGetValue(crop.indexOfHarvest.Value, out var harvest) && harvest.Name != cropData.harvestName || (!string.IsNullOrEmpty(cropData.cropName) && Game1.objectData.TryGetValue(crop.netSeedIndex.Value, out var objData) && objData.Name != cropData.cropName));
        }
        private static int SwitchExpType(int type, Crop crop)
        {
            if (!Config.ModEnabled || crop.whichForageCrop.Value != "-424242")
                return type;
            return Farmer.foragingSkill;
        }
    }
}