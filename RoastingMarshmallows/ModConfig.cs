using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace RoastingMarshmallows
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public KeybindList RoastKey { get; set; } = new KeybindList(SButton.Space);
		public int RoastFrames { get; set; } = 1440;
		public string Product { get; set; } = "aedenthorn.RoastingMarshmallows_Marshmallow";
		public string BurntProduct { get; set; } = "aedenthorn.RoastingMarshmallows_BurntMarshmallow";
    }
}
