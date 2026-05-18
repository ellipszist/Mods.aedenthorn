using System.Collections.Generic;

namespace FenceDecorations
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; }
        public List<string> DisallowedDecorations { get; set; } = new List<string>();
        
    }
}
