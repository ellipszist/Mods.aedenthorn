using StardewModdingAPI;

namespace StardewOpenWorld
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public int TilesPerForestMin { get; set; } = 500;
        public int TilesPerForestMax { get; set; } = 1000;
        public int TilesPerMonsterMin { get; set; } = 800;
        public int TilesPerMonsterMax { get; set; } = 1500;
        public int MinTreesPerForest { get; set; } = 30;
        public int MaxTreesPerForest { get; set; } = 80;
        public double MonsterDensity { get; set; } = 0.2;
        public double TreeDensity { get; set; } = 0.3;
    }
}
