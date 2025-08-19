using StardewModdingAPI;
using System.Collections.Generic;

namespace StardewOpenWorld
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool CreateEntrance { get; set; } = true;
        public bool NewMapDaily { get; set; } = false;
        public bool DrawMap { get; set; } = true;
        public int OpenWorldSize { get; set; } = 10000;
        public int MaxOutcropLevel { get; set; } = 1000;
        public int TilesPerForestMin { get; set; } = 500;
        public int TilesPerForestMax { get; set; } = 1000;
        public int TilesPerLakeMin { get; set; } = 2000;
        public int TilesPerLakeMax { get; set; } = 4000;
        public int TilesPerOutcropMin { get; set; } = 600;
        public int TilesPerOutcropMax { get; set; } = 1200;
        public int TilesPerMonsterMin { get; set; } = 800;
        public int TilesPerMonsterMax { get; set; } = 1500;
        public int MinLakeSize { get; set; } = 36;
        public int MaxLakeSize { get; set; } = 300;
        public int MinTreesPerForest { get; set; } = 30;
        public int MaxTreesPerForest { get; set; } = 80;
        public int MinRocksPerOutcrop { get; set; } = 5;
        public int MaxRocksPerOutcrop { get; set; } = 40;
        public int MinGrassPerField { get; set; } = 5;
        public int MaxGrassPerField { get; set; } = 40;
        public double MonsterDensity { get; set; } = 0.2;
        public double TreeDensity { get; set; } = 0.3;
        public double RockDensity { get; set; } = 0.3;
        public double GrassDensity { get; set; } = 0.3;
        public string BackgroundMusic { get; set; } = "desolate";
        public int TilesPerChestMin { get; set; } = 1000;
        public int TilesPerChestMax { get; set; } = 10000;
        public int TilesPerGrassMin { get; set; } = 500;
        public int TilesPerGrassMax { get; set; } = 1000;
        public int MaxItems { get; set; } = 5;
        public int ItemsBaseMaxValue { get; set; } = 100;
        public int MinItemValue { get; set; } = 20;
        public int MaxItemValue { get; set; } = -1;
        public int CoinBaseMin { get; set; } = 20;
        public int CoinBaseMax { get; set; } = 100;
        public float RarityChance { get; set; } = 0.2f;
        public float IncreaseRate { get; set; } = 0.3f;
        public Dictionary<string, int> ItemListChances { get; set; } = new Dictionary<string, int>
        {
            {"MeleeWeapon", 100},
            {"Shirt", 50},
            {"Pants", 50},
            {"Hat", 50},
            {"Boots", 100},
            {"BigCraftable", 50},
            {"Ring", 100},
            {"Seed", 100},
            {"Mineral", 100},
            {"Relic", 100},
            {"Cooking", 50},
            {"Fish", 0},
            {"BasicObject", 50}
        };
    }
}
