using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace FenceRepair
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; }
        public bool RequireMats { get; set; } = true;
        public bool TakeMatsFromInventory { get; set; } = true;
        public bool TakeMatsFromChests { get; set; } = true;
        public string HealthString1 { get; set; } = "health";
        public string HealthString2 { get; set; } = "maxhealth";
        public Color ColorHigh { get; set; } = Color.Green;
        public Color ColorMid { get; set; } = Color.Yellow;
        public Color ColorLow { get; set; } = Color.Red;
        public SButton ShowHealthKey { get; set; } = SButton.LeftShift;
    }
}
