
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FuzzyTime
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public Dictionary<int, string> TimeNames { get; set; } = new()
        {
            { 600, "early-morning" },
            { 800, "morning" },
            { 1000, "late-morning" },
            { 1130, "noon" },
            { 1230, "early-afternoon" },
            { 1400, "afternoon" },
            { 1600, "late-afternoon" },
            { 1900, "evening" },
            { 2100, "night" },
            { 2330, "midnight" },
            { 2430, "late-night" }
        };
    }
}
