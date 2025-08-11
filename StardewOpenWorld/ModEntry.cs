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

namespace StardewOpenWorld
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static string biomePath = "aedenthorn.StardewOpenWorld/biomes";
        public static string dictPath = "aedenthorn.StardewOpenWorld/dict";
        public static string seedKey = "aedenthorn_StardewOpenWorld_seed";
        public static string mapPath = "StardewOpenWorldMap";
        public static string locName = "StardewOpenWorld";
        public static string tilePrefix = "StardewOpenWorldTile";
        public static int openWorldChunkSize = 100;
        public static int openWorldSize = 100000;
        public static bool warping = false;
        public static PerScreen<Point> playerTilePoint = new(() => new Point(-1, -1));
        public static PerScreen<Point> playerChunk = new(() => new Point(-1,-1));

        public static GameLocation openWorldLocation;
        //private static Dictionary<string, Biome> biomes = new Dictionary<string, Biome>();
        public static Dictionary<string, Func<ulong, int, int, WorldChunk>> biomes;

        public static Dictionary<Point, WorldChunk> cachedChunks;
        public static Dictionary<string, Biome> biomeDict;

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
            else if (e.NameWithoutLocale.IsEquivalentTo(biomePath))
            {
                e.LoadFrom(() => new Dictionary<string, Func<ulong, int, int, WorldChunk>>(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, Biome>(), AssetLoadPriority.Exclusive);
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
                if(Game1.player.TilePoint != playerTilePoint.Value)
                {
                    var newChunk = GetPlayerChunk(Game1.player);
                    if (newChunk != playerChunk.Value)
                    {
                        PlayerTileChanged();
                        playerChunk.Value = newChunk;
                    }
                    playerTilePoint.Value = Game1.player.TilePoint;
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
                            PreloadWorldChunk(cx, cy);
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