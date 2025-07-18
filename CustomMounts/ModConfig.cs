﻿using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CustomMounts
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
        public bool CustomMountsStopGrowth { get; set; } = true;
        public bool WeededDoubleGrowth { get; set; } = false;
        public int WeedExp { get; set; } = 1;
		public int WeedGrowthPerDayMin { get; set; } = 1;
		public int WeedGrowthPerDayMax { get; set; } = 25;
		public float CustomMountstaminaUse { get; set; } = 1;
		public int WeedTintR { get; set; } = 255;
		public int WeedTintG { get; set; } = 150;
		public int WeedTintB { get; set; } = 0;
		public int WeedTintA { get; set; } = 255;
	}
}
