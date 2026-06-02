using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace CloserCrops
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool MultiplyPlantAndHarvest { get; set; } = true;
        public bool ModKeyForCloser { get; set; } = false;
        public SButton ModKey { get; set; } = SButton.LeftControl;
        public List<string> CloserCrops { get; set; } = new()
        {
            "472", //parsnip
            "476", //garlic
            "484", //radish
            "494", //beet
            "427", //tulip
            "429", //jazz
            "453", //poppy
            "455", //spangle
            "745", //strawberry
            "CarrotSeeds", //CarrotSeeds
        };
    }
}
