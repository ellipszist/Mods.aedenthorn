using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.GameData;
using System;
using System.Collections.Generic;
using System.IO;
using xTile;
using Object = StardewValley.Object;
using xTile.Dimensions;
using StardewValley.Monsters;

namespace StardewOpenWorld
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static string modKey = "aedenthorn.StardewOpenWorld";
        public static string codeBiomePath = "aedenthorn.StardewOpenWorld/code_biomes";
        public static string biomeDictPath = "aedenthorn.StardewOpenWorld/biomes";
        public static string monsterDictPath = "aedenthorn.StardewOpenWorld/monsters";
        public static string seedKey = "aedenthorn_StardewOpenWorld_seed";
        public static string mapPath = "StardewOpenWorldMap";
        public static string locName = "StardewOpenWorld";
        public static string tilePrefix = "StardewOpenWorldTile";
        public static int openWorldChunkSize = 100;
        public static int openWorldSize = 10000;
        public static bool warping = false;

        public static PerScreen<Point> playerTilePoint = new(() => new Point(-1, -1));
        public static PerScreen<Point> playerChunk = new(() => new Point(-1,-1));

        public static Dictionary<Point, Dictionary<Vector2, string>> treeCenters;
        public static Dictionary<Point, Dictionary<Vector2, string>> monsterCenters;

        public static GameLocation openWorldLocation;
        //private static Dictionary<string, Biome> biomes = new Dictionary<string, Biome>();
        public static Dictionary<string, Func<ulong, int, int, WorldChunk>> biomeCodeDict;

        public static Dictionary<Point, WorldChunk> cachedChunks = new();
        public static List<Point> loadedChunks = new();

        public static Dictionary<string, MonsterSpawnInfo> monsterDict;
        public static Dictionary<string, Biome> biomeDict;

        public static List<int> grassTiles = new List<int>() { 351, 304, 305, 300 };

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Content.AssetRequested += Content_AssetRequested;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
        public override object GetApi()
        {
            return new StardewOpenWorldAPI();
        }
        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            openWorldLocation = Game1.getLocationFromName(locName);
            ReloadMonsterDict();

            treeCenters = new();
            monsterCenters = new();
            Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, 242);
            for (int i = 0; i < r.Next(openWorldSize / Config.TilesPerTreeMax * openWorldSize, openWorldSize / Config.TilesPerTreeMin * openWorldSize + 1); i++)
            {
                Point ap = new(
                        r.Next(10, openWorldSize - 10),
                        r.Next(10, openWorldSize - 10)
                    );
                Point cp = new(ap.X / openWorldChunkSize, ap.Y / openWorldChunkSize);
                Vector2 rp = new(ap.X % openWorldChunkSize, ap.Y % openWorldChunkSize);
                if (!treeCenters.ContainsKey(cp))
                {
                    treeCenters[cp] = new();
                }
                treeCenters[cp][rp] = GetRandomTree(ap.ToVector2(), r);
            }
            for (int i = 0; i < r.Next(openWorldSize / Config.TilesPerMonsterMax * openWorldSize, openWorldSize / Config.TilesPerMonsterMin * openWorldSize + 1); i++)
            {
                Point ap = new(
                        r.Next(10, openWorldSize - 10),
                        r.Next(10, openWorldSize - 10)
                    );
                Point cp = new(ap.X / openWorldChunkSize, ap.Y / openWorldChunkSize);
                Vector2 rp = new(ap.X % openWorldChunkSize, ap.Y % openWorldChunkSize);
                if (!monsterCenters.ContainsKey(cp))
                {
                    monsterCenters[cp] = new();
                }
                monsterCenters[cp][rp] = GetRandomMonsterSpawn(ap.ToVector2(), r);
            }
        }


        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Backwoods"))
            {
                e.LoadFromModFile<Map>(Path.Combine("assets", "BackwoodsEdit.tmx"), AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.Name.Contains(mapPath))
            {
                e.LoadFromModFile<Map>("assets/StardewOpenWorld.tmx", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(codeBiomePath))
            {
                e.LoadFrom(() => new Dictionary<string, Func<ulong, int, int, WorldChunk>>(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(biomeDictPath))
            {
                e.LoadFrom(() => new Dictionary<string, Biome>(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(monsterDictPath))
            {
                e.LoadFrom(() => new Dictionary<string, MonsterSpawnInfo>(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                e.Edit(asset =>
                {
                    IDictionary<string, LocationData> data = asset.AsDictionary<string, LocationData>().Data;

                    data.Add(locName, new LocationData()
                    {
                        DisplayName = "Stardew Open World",
                        DefaultArrivalTile = new Point(50000, 50000),
                        CreateOnLoad = new CreateLocationData()
                        {
                            MapPath = SHelper.ModContent.GetInternalAssetName("assets/StardewOpenWorld.tmx").Name,
                            Type = "StardewValley.GameLocation",
                            AlwaysActive = false
                        },
                        CanPlantHere = true,
                        ExcludeFromNpcPathfinding = true,
                        ArtifactSpots = null,
                        FishAreas = null,
                        Fish = null,
                        Forage = null,
                        MinDailyWeeds = 0,
                        MaxDailyWeeds = 0,
                        FirstDayWeedMultiplier = 0,
                        MinDailyForageSpawn = 0,
                        MaxDailyForageSpawn = 0,
                        MaxSpawnedForageAtOnce = 0,
                        ChanceForClay = 0,
                        Music = null,
                        MusicDefault = "EmilyDream",
                        MusicContext = MusicContext.SubLocation,
                        MusicIgnoredInRain = false,
                        MusicIgnoredInSpring = false,
                        MusicIgnoredInSummer = false,
                        MusicIgnoredInFall = false,
                        MusicIgnoredInWinter = false,
                        MusicIgnoredInFallDebris = false,
                        MusicIsTownTheme = false
                    });
                });
            }
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady)
                return;
            if(Game1.player.currentLocation == openWorldLocation)
            {
                var cs = Game1.player.currentLocation.characters.Count;
                //SMonitor.Log(Utility.playerCanPlaceItemHere(Game1.player.currentLocation, ItemRegistry.Create("(BC)146"), (int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y, Game1.player)+"");
                //SMonitor.Log(Utility.isPlacementForbiddenHere(Game1.currentLocation) + "");
                //SMonitor.Log(Utility.isWithinTileWithLeeway(Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y, ItemRegistry.Create("(BC)146"), Game1.player)+"");
                if (Game1.player.TilePoint != playerTilePoint.Value)
                {
                    var newChunk = GetPlayerChunk(Game1.player);
                    if (newChunk != playerChunk.Value)
                    {
                        PlayerTileChanged();
                        playerChunk.Value = newChunk;
                    }
                    playerTilePoint.Value = Game1.player.TilePoint;
                }
                if(openWorldLocation.characters.Count == 0)
                {
                    var asdf = 1;
                }
                return;
            }
            else if (playerChunk.Value.X != -1)
            {
                PlayerTileChanged();
                playerTilePoint.Value = new(-1, -1);
                playerChunk.Value = new(-1, -1);
            }
            if (!Game1.isWarping && Game1.player.currentLocation.Name.Equals("Backwoods"))
            {
                Game1.warpFarmer(locName, openWorldSize / 2, openWorldSize / 2, false);
                int center = openWorldSize / openWorldChunkSize / 2;
                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        var cx = center + x;
                        var cy = center + y;
                        if (cx >= 0 && cy >= 0)
                        {
                            BuildWorldChunk(new Point(cx, cy));
                        }
                    }
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
        }

    }
}