
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AreaOfEffect
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public KeybindList RechargeButton { get; set; } = new(SButton.MouseMiddle);
        public bool Debug { get; set; } = false;
    }
}
