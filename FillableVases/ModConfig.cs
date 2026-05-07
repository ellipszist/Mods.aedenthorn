using StardewModdingAPI;
using System.Collections.Generic;

namespace FillableVases
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool Debug { get; set; } = false;
		public int PrismaticChance { get; set; } = 5;
		public SButton ModKey { get; set; } = SButton.LeftShift;
    }
}
