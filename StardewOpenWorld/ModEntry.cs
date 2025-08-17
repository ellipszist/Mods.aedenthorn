using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using xTile;
using xTile.Tiles;

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
        private const string modCoinKey = "aedenthorn.StardewOpenWorld/Coin";
        public static string codeBiomePath = "aedenthorn.StardewOpenWorld/code_biomes";
        public static string landmarkDictPath = "aedenthorn.StardewOpenWorld/biomes";
        public static string monsterDictPath = "aedenthorn.StardewOpenWorld/monsters";
        public static string seedKey = "aedenthorn_StardewOpenWorld_seed";
        public static string mapPath = "aedenthorn_StardewOpenWorld_Map";
        public static string locName = "StardewOpenWorld";
        public static string tilePrefix = "StardewOpenWorldTile";
        public static int openWorldChunkSize = 100;
        public static bool warping = false;

        public static PerScreen<Point> playerTilePoint = new(() => new Point(-1, -1));
        public static PerScreen<Point> playerChunk = new(() => new Point(-1,-1));

        public static Dictionary<string, Dictionary<int, AnimatedTile>> animatedTiles = new();
        
        public static Dictionary<Point, Dictionary<Vector2, string>> treeCenters = new();
        public static Dictionary<Point, List<Vector2>> rockCenters = new();
        public static Dictionary<Point, List<Point>> lakeCenters = new();
        public static Dictionary<Point, Dictionary<Vector2, string>> monsterCenters = new();

        public static GameLocation openWorldLocation;
        //private static Dictionary<string, Biome> biomes = new Dictionary<string, Biome>();
        public static Dictionary<string, Func<ulong, int, int, WorldChunk>> biomeCodeDict;

        public static Dictionary<Point, WorldChunk> cachedChunks = new();
        public static List<Point> loadedChunks = new();

        public static Dictionary<string, Landmark> landmarkDict;
        public static int RandomSeed = -1;
        public static List<int> grassTiles = new List<int>() { 351, 304, 305, 300 };

        private static IAdvancedLootFrameworkApi advancedLootFrameworkApi;
        private static List<object> treasuresList = new();
        private static Debris debris;
        private static readonly Color[] tintColors = new Color[]
        {
            Color.DarkGray,
            Color.Brown,
            Color.Silver,
            Color.Gold,
            Color.Purple,
        };


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
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(Config.Debug && e.Button == SButton.L)
            {
            }
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            ReloadOpenWorld(Config.NewMapDaily);
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

            CreateAnimatedTiles();

            treeCenters = new();
            rockCenters = new();
            monsterCenters = new();
            ReloadOpenWorld(true);

            //CacheAllChunks();
        }


        private void CacheAllChunks()
        {
            Stopwatch s = new();
            s.Start();
            int num = Config.OpenWorldSize / openWorldChunkSize;
            for (int x = 0; x < num; x++)
                for (int y = 0; y < num; y++)
                {
                    if(y == 0)
                        SMonitor.Log($"Caching {(x * num)/(num*num)} chunks");
                    BuildWorldChunk(new Point(x, y));
                }
            s.Stop();
            SMonitor.Log($"Cached {num} chunks in {s.Elapsed.TotalSeconds}s");
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(mapPath))
            {
                e.LoadFromModFile<Map>("assets/StardewOpenWorld.tmx", AssetLoadPriority.Exclusive);

                e.Edit(asset =>
                {
                    Map map = asset.AsMap().Data;
                    map.Properties["Warp"] = $"{Config.OpenWorldSize / 2 + openWorldChunkSize / 2} {Config.OpenWorldSize - 1} Mine 68 7 {Config.OpenWorldSize / 2 + openWorldChunkSize / 2 + 1} {Config.OpenWorldSize - 1} Mine 69 7";
                });
            }
            else if (Config.CreateEntrance && e.NameWithoutLocale.IsEquivalentTo("Maps/Mine"))
            {
                e.Edit(asset =>
                {
                    Map map = asset.AsMap().Data;
                    var bck = map.GetLayer("Back");
                    var bld = map.GetLayer("Buildings");
                    TileSheet sheet = bck.Tiles[68, 6].TileSheet;
                    for(int x = 0; x < 4; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            if (bck.Tiles[x, y] != null)
                            {
                                bck.Tiles[x, y].TileIndex = 218;
                            }
                            else
                            {
                                bck.Tiles[x + 67, y + 3] = new StaticTile(bck, sheet, BlendMode.Alpha, 218);
                            }
                        }
                    }
                    bld.Tiles[67, 3].TileIndex = 118;
                    bld.Tiles[68, 3].TileIndex = 121;
                    bld.Tiles[69, 3].TileIndex = 122;
                    bld.Tiles[70, 3].TileIndex = 141;
                    bld.Tiles[67, 4].TileIndex = 87;
                    bld.Tiles[68, 4] = null;
                    bld.Tiles[69, 4] = null;
                    bld.Tiles[70, 4].TileIndex = 88;
                    bld.Tiles[67, 5].TileIndex = 103;
                    bld.Tiles[68, 5] = null;
                    bld.Tiles[69, 5] = null;
                    bld.Tiles[70, 5].TileIndex = 104;
                    bld.Tiles[67, 6].TileIndex = 119;
                    bld.Tiles[68, 6] = null;
                    bld.Tiles[69, 6] = null;
                    bld.Tiles[70, 6].TileIndex = 120;
                    map.Properties["Warp"] = (string)map.Properties["Warp"] + $" 68 6 {locName} {Config.OpenWorldSize / 2 + openWorldChunkSize / 2 + 1} {Config.OpenWorldSize - 2} 69 6 {locName} {Config.OpenWorldSize / 2 + openWorldChunkSize / 2 + 2} {Config.OpenWorldSize - 2}";
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(codeBiomePath))
            {
                e.LoadFrom(() => new Dictionary<string, Func<ulong, int, int, WorldChunk>>(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(landmarkDictPath))
            {
                e.LoadFrom(() => new Dictionary<string, Landmark>(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(monsterDictPath))
            {
                e.LoadFrom(() => GetMonsterDict(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                e.Edit(asset =>
                {
                    IDictionary<string, LocationData> data = asset.AsDictionary<string, LocationData>().Data;

                    data.Add(locName, new LocationData()
                    {
                        DisplayName = "Stardew Open World",
                        CreateOnLoad = new CreateLocationData()
                        {
                            MapPath = mapPath,
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
                        MusicDefault = !string.IsNullOrEmpty(Config.BackgroundMusic) ? Config.BackgroundMusic : "",
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
                Game1.warpFarmer(locName, Config.OpenWorldSize / 2 + openWorldChunkSize / 2 + 1, Config.OpenWorldSize - 2, 0);
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {           // Get Advanced Loot Framework's API
            advancedLootFrameworkApi = Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
            if (advancedLootFrameworkApi is not null)
            {
                Monitor.Log($"Loaded AdvancedLootFramework API");
                UpdateTreasuresList();
                Monitor.Log($"Got {treasuresList.Count} possible treasures");
            }

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
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
}