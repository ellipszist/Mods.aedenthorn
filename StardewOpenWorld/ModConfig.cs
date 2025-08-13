using StardewModdingAPI;

namespace StardewOpenWorld
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public int TilesPerTreeMin { get; set; } = 2000;
        public int TilesPerTreeMax { get; set; } = 4000;
        public int TilesPerMonsterMin { get; set; } = 3000;
        public int TilesPerMonsterMax { get; set; } = 10000;
        public double MonsterDensity { get; set; } = 0.3;
    }
}
