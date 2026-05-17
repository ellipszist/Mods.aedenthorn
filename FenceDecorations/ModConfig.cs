using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace FenceDecorations
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; }
        public List<string> AllowedDecorations { get; set; } = new List<string>()
        {
            "(O)746",
            "(F)1369"
        };
        
    }
}
