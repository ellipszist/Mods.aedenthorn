using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ImmersiveSprinklersAndScarecrows
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string sprinklerPrefix = "aedenthorn.ImmersiveSprinklers/";
        public static string scarecrowPrefix = "aedenthorn.ImmersiveScarecrows/";
        public static string sprinklerKey = "aedenthorn.ImmersiveSprinklers/sprinkler";
        public static string scarecrowKey = "aedenthorn.ImmersiveScarecrows/scarecrow";
        public static string sprinklerBigCraftableKey = "aedenthorn.ImmersiveSprinklers/bigCraftable";
        public static string scarecrowBigCraftableKey = "aedenthorn.ImmersiveScarecrows/bigCraftable";
        public static string sprinklerGuidKey = "aedenthorn.ImmersiveSprinklers/guid";
        public static string scarecrowGuidKey = "aedenthorn.ImmersiveScarecrows/guid";
        public static string enricherKey = "aedenthorn.ImmersiveSprinklers/enricher";
        public static string fertilizerKey = "aedenthorn.ImmersiveSprinklers/fertilizer";
        public static string nozzleKey = "aedenthorn.ImmersiveSprinklers/nozzle";
        public static string altTextureSprinklerPrefix = "aedenthorn.ImmersiveSprinklers/AlternativeTexture";
        public static string altTextureScarecrowPrefix = "aedenthorn.ImmersiveScarecrows/AlternativeTexture";
        public static string altTextureKey = "AlternativeTexture";
        public static string scaredKey = "aedenthorn.ImmersiveScarecrows/scared";
        public static string hatKey = "aedenthorn.ImmersiveScarecrows/hat";

        public static Dictionary<string, Object> sprinklerDict = new();
        public static Dictionary<string, Object> scarecrowDict = new();
        public static object atApi;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            HarmonyMethod prefix = new(typeof(ModEntry), nameof(ModEntry.Modded_Farm_AddCrows_Prefix));
            Type prismaticPatches = AccessTools.TypeByName("PrismaticTools.Framework.PrismaticPatches");
            if (prismaticPatches is not null)
            {
                MethodInfo prismaticPrefix = AccessTools.Method(prismaticPatches, "Farm_AddCrows");
                if (prismaticPrefix is not null)
                {
                    harmony.Patch(prismaticPrefix, prefix: prefix);
                    Monitor.Log("Found Prismatic Tools, patching for compat", LogLevel.Info);
                }
            }

            Type radioactivePatches = AccessTools.TypeByName("RadioactiveTools.Framework.RadioactivePatches");
            if (radioactivePatches is not null)
            {
                MethodInfo radioactivePrefix = AccessTools.Method(radioactivePatches, "Farm_AddCrows");
                if (radioactivePrefix is not null)
                {
                    harmony.Patch(radioactivePrefix, prefix: prefix);
                    Monitor.Log("Found Radioactive Tools, patching for compat", LogLevel.Info);
                }
            }
        }
        public override object GetApi()
        {
            return new ImmersiveApi();
        }
        public void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree || Game1.currentLocation?.terrainFeatures is null)
                return;

            var sc = Helper.Input.IsDown(Config.ShowScarecrowRangeButton);
            var sp = Helper.Input.IsDown(Config.ShowSprinklerRangeButton);
            if (!sc && !sp)
                return;

            HashSet<Vector2> sprinklerTiles = new();
            HashSet<Vector2> scarecrowTiles = new();
            foreach (var kvp in Game1.currentLocation.modData.Pairs)
            {
                if (sp && kvp.Key.StartsWith(sprinklerKey+","))
                {
                    try
                    {
                        int x = int.Parse(kvp.Key.Split(',')[1]);
                        int y = int.Parse(kvp.Key.Split(',')[2]);
                        var obj = GetSprinklerCached(Game1.currentLocation, x, y);
                        if (obj.IsSprinkler())
                        {
                            foreach (var t in GetSprinklerTiles(new Vector2(x, y), GetSprinklerRadius(obj)))
                            {
                                sprinklerTiles.Add(t);
                            }
                        }
                    }
                    catch { }
                }
                if (sc && kvp.Key.StartsWith(scarecrowKey + ","))
                {
                    try
                    {
                        int x = int.Parse(kvp.Key.Split(',')[1]);
                        int y = int.Parse(kvp.Key.Split(',')[2]);
                        var obj = GetScarecrowCached(Game1.currentLocation, x, y);
                        if (obj.IsScarecrow())
                        {
                            foreach (var t in GetScarecrowTiles(new Vector2(x, y), obj.GetRadiusForScarecrow()))
                            {
                                scarecrowTiles.Add(t);
                            }
                        }
                    }
                    catch { }
                }
            }
            foreach (var tile in sprinklerTiles)
            {
                e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)((int)tile.X * 64), (float)((int)tile.Y * 64))), new Rectangle?(new Rectangle(194, 388, 16, 16)), (scarecrowTiles.Contains(tile) ? Config.BothRangeTint : Config.SprinklerRangeTint) * Config.RangeAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
            }
            foreach (var tile in scarecrowTiles)
            {
                if (sprinklerTiles.Contains(tile))
                    continue;
                e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)((int)tile.X * 64), (float)((int)tile.Y * 64))), new Rectangle?(new Rectangle(194, 388, 16, 16)), Config.ScarecrowRangeTint * Config.RangeAlpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
            }
        }

        public void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if(e.Button == SButton.OemOpenBrackets && Context.IsWorldReady)
            {
                //foreach(var key in Game1.currentLocation.terrainFeatures.Keys)
                //{
                //    if (Game1.currentLocation.terrainFeatures[key] is HoeDirt dirt && dirt.crop != null)
                //    {
                //        dirt.crop.growCompletely();
                //        Game1.currentLocation.terrainFeatures[key] = dirt;
                //    }
                //}
            }
            if (e.Button == Config.PickupButton && Context.CanPlayerMove)
            {
                var tile = GetMouseCornerTile();
                if (ReturnOrDropSprinkler(Game1.currentLocation, tile.X, tile.Y, Game1.player, false))
                {
                    Helper.Input.Suppress(e.Button);
                }
                else if (ReturnOrDropScarecrow(Game1.currentLocation, tile.X, tile.Y, Game1.player, false))
                {
                    Helper.Input.Suppress(e.Button);
                }
                else if (Config.PickupNearby || Constants.TargetPlatform == GamePlatform.Android)
                {
                    foreach(var p in GetSprinklerVectors(Game1.currentLocation))
                    {
                        var distance = Vector2.Distance(new Vector2(p.X + 1, p.Y + 1) * 64, Game1.player.position.Value);
                        if (distance > 64)
                            continue;
                        if (ReturnOrDropSprinkler(Game1.currentLocation, (int)p.X, (int)p.Y, Game1.player, false))
                        {
                            Helper.Input.Suppress(e.Button);
                            return;
                        }
                    }
                    foreach(var p in GetScarecrowVectors(Game1.currentLocation))
                    {
                        var distance = Vector2.Distance(new Vector2(p.X + 1, p.Y + 1) * 64, Game1.player.position.Value);
                        if (distance > 64)
                            continue;
                        if (ReturnOrDropScarecrow(Game1.currentLocation, (int)p.X, (int)p.Y, Game1.player, false))
                        {
                            Helper.Input.Suppress(e.Button);
                            return;
                        }
                    }
                }
            }
            else if (e.Button == Config.ActivateButton && Context.CanPlayerMove)
            {
                Point tile = GetMouseCornerTile();

                var obj = GetSprinkler(Game1.currentLocation, tile.X, tile.Y);
                if (obj is not null)
                {
                    obj.Location = Game1.currentLocation;
                    ActivateSprinkler(Game1.currentLocation, tile.ToVector2(), obj, false);
                    Helper.Input.Suppress(e.Button);
                }
                else if (Config.ActivateNearby || Constants.TargetPlatform == GamePlatform.Android)
                {
                    foreach (var v in GetSprinklerVectors(Game1.currentLocation))
                    {
                        if(Config.ActivateNearbyRange > 0)
                        {
                            var distance = Vector2.Distance(v * 64, Game1.player.position.Value);
                            if (distance > 64 * Config.ActivateNearbyRange)
                                continue;
                        }
                        obj = GetSprinkler(Game1.currentLocation, tile.X, tile.Y);
                        if (obj is not null)
                        {
                            obj.Location = Game1.currentLocation;
                            ActivateSprinkler(Game1.currentLocation, tile.ToVector2(), obj, false);
                            Helper.Input.Suppress(e.Button);
                        }
                    }
                }
            }
        }

        public void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            sprinklerDict.Clear();
            scarecrowDict.Clear();
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            atApi = Helper.ModRegistry.GetApi("PeacefulEnd.AlternativeTextures");
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Range When Placing",
                getValue: () => Config.ShowRangeWhenPlacing,
                setValue: value => Config.ShowRangeWhenPlacing = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Pickup Key",
                getValue: () => Config.PickupButton,
                setValue: value => Config.PickupButton = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Pickup Nearby",
                getValue: () => Config.PickupNearby,
                setValue: value => Config.PickupNearby = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Activate Key",
                getValue: () => Config.ActivateButton,
                setValue: value => Config.ActivateButton = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Activate Nearby",
                getValue: () => Config.ActivateNearby,
                setValue: value => Config.ActivateNearby = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Activation Range",
                getValue: () => Config.ActivateNearbyRange,
                setValue: value => Config.ActivateNearbyRange = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Show Sprinkler Range",
                getValue: () => Config.ShowSprinklerRangeButton,
                setValue: value => Config.ShowSprinklerRangeButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Show Scarecrow Range",
                getValue: () => Config.ShowScarecrowRangeButton,
                setValue: value => Config.ShowScarecrowRangeButton = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Scale",
                getValue: () => Config.Scale + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.Scale = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Alpha",
                getValue: () => Config.Alpha + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.Alpha = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "RangeAlpha",
                getValue: () => Config.RangeAlpha + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.RangeAlpha = f; } }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Offset X",
                getValue: () => Config.DrawOffsetX,
                setValue: value => Config.DrawOffsetX = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Offset Y",
                getValue: () => Config.DrawOffsetY,
                setValue: value => Config.DrawOffsetY = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Offset Z",
                getValue: () => Config.DrawOffsetZ,
                setValue: value => Config.DrawOffsetZ = value
            );
        }
    }
}
