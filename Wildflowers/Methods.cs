using StardewValley;
using StardewValley.TerrainFeatures;
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
            var weights = new Dictionary<string, float>();
            float totalWeight = 0;
            foreach(var kvp in crops)
            {
                if (!flowers.Contains(kvp.Value.HarvestItemId))
                    continue;
                if (!kvp.Value.Seasons.Contains(Game1.season))
                    continue;
                if(!Game1.objectData.TryGetValue(kvp.Value.HarvestItemId, out var data))
                    continue;
                float weight = Config.EnableFlowerRarity ? 1 / (float)(Config.FullFlowerRarity ? data.Price : Math.Sqrt(data.Price)) : 1;
                totalWeight += weight;
                weights.Add(kvp.Key, totalWeight);
            }
            if(weights.Count > 0)
            {
                float roll = totalWeight * (float)Game1.random.NextDouble();
                foreach(var kvp in weights)
                {
                    if(kvp.Value > roll)
                        return kvp.Key;
                }
            }
                
            return null;
        }
        private static bool IsCropDataInvalid(Crop crop, CropData cropData)
        {
            return (!string.IsNullOrEmpty(cropData.harvestName) && Game1.objectData.TryGetValue(crop.indexOfHarvest.Value, out var harvest) && harvest.Name != cropData.harvestName || (!string.IsNullOrEmpty(cropData.cropName) && Game1.objectData.TryGetValue(crop.netSeedIndex.Value, out var objData) && objData.Name != cropData.cropName));
        }
        private static int SwitchExpType(int type, Crop crop, HoeDirt dirt)
        {
            if (!Config.ModEnabled || dirt?.modData.ContainsKey(wildKey) != true)
                return type;
            return Farmer.foragingSkill;
        }
    }
}