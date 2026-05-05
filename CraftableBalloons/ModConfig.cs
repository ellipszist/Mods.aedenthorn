using StardewModdingAPI;
using System.Collections.Generic;

namespace CraftableBalloons
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public int PrismaticChance { get; set; } = 5;
		public SButton ModKey { get; set; } = SButton.LeftShift;
    }
}
