using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using xTile.Dimensions;
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

        public const string dataKey = "aedenthorn.ImmersiveSprinklersAndScarecrows/data";
        public const string altTextureKey = "AlternativeTexture";

        public const int ridiculous = 10000;

        public static Dictionary<GameLocation, Dictionary<Vector2, Object>> sprinklerDict = new();
        public static Dictionary<GameLocation, Dictionary<Vector2, Object>> scarecrowDict = new();
        
        public static object atApi;
        public static Vector2 atPosition;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.Saving += GameLoop_Saving;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            Helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;

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

        private void Multiplayer_ModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.FromModID == ModManifest.UniqueID && e.Type == "UpdateImmersiveObjects")
            {
                IEnumerable<Object> sp = new List<Object>();
                IEnumerable<Object> sc = new List<Object>();

                MyMessage m = e.ReadAs<MyMessage>();
                var loc = Game1.getLocationFromName(m.Location);
                if (loc == null)
                {
                    foreach (var l in Game1.locations)
                    {
                        ReloadSprinklers(l);
                        ReloadScarecrows(l);
                    }
                    return;
                }
                switch (m.Which)
                {
                    case "sprinklers":
                        ReloadSprinklers(loc);
                        break;
                    case "scarecrows":
                        ReloadScarecrows(loc);
                        break;
                    default:
                        ReloadSprinklers(loc);
                        ReloadScarecrows(loc);
                        break;
                }
            }
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            foreach (var l in Game1.locations)
            {
                ReloadSprinklers(l);
                ReloadScarecrows(l);
                
            }
        }

        private void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            foreach(var kvp in sprinklerDict)
            {
                foreach(var kvp2 in kvp.Value)
                {
                    if (kvp2.Value is null)
                        continue;
                    var data = GetImmersiveData(kvp2.Value, true);
                    SetData(kvp.Key, (int)kvp2.Key.X, (int)kvp2.Key.Y, data);
                }
            }
            foreach(var kvp in scarecrowDict)
            {
                foreach(var kvp2 in kvp.Value)
                {
                    if (kvp2.Value is null)
                        continue;
                    var data = GetImmersiveData(kvp2.Value, false);
                    SetData(kvp.Key, (int)kvp2.Key.X, (int)kvp2.Key.Y, data);
                }
            }
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsWorldReady)
                return;
            if(e.NewMenu == null && Game1.currentLocation.Objects.TryGetValue(new Vector2(-ridiculous, -ridiculous), out var obj))
            {
                obj.TileLocation = atPosition;
                StoreObj(obj);
                Game1.currentLocation.Objects.Remove(new Vector2(-ridiculous, -ridiculous));
                SendMessage(Game1.currentLocation.NameOrUniqueName, "both");
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

            var sp = Helper.Input.IsDown(Config.ShowSprinklerRangeButton);
            var sc = Helper.Input.IsDown(Config.ShowScarecrowRangeButton);
            if (!sc && !sp)
                return;

            HashSet<Vector2> sprinklerTiles = new();
            HashSet<Vector2> scarecrowTiles = new();
            IEnumerable<Object> sprinklers;

            if (sp)
            {
                sprinklers = GetSprinklers(Game1.currentLocation);
                foreach (var obj in sprinklers)
                {
                    if (obj is null)
                        continue;
                    foreach (var t in GetSprinklerTiles(obj.TileLocation, GetSprinklerRadius(obj)))
                    {
                        sprinklerTiles.Add(t);
                    }
                }

            }
            if (sc)
            {
                IEnumerable<Object> scarecrows = GetAsScarecrows(Game1.currentLocation);
                foreach (var obj in scarecrows)
                {
                    if (obj is null)
                        continue;
                    foreach (var t in GetScarecrowTiles(obj.TileLocation, obj.GetRadiusForScarecrow()))
                    {
                        scarecrowTiles.Add(t);
                    }
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
                    foreach(var obj in GetSprinklers(Game1.currentLocation))
                    {
                        var distance = Vector2.Distance(obj.TileLocation * 64, Game1.player.position.Value);
                        if (distance > Config.PickupNearbyRange * 64)
                            continue;
                        if (ReturnOrDropSprinkler(Game1.currentLocation, (int)obj.TileLocation.X, (int)obj.TileLocation.Y, Game1.player, false))
                        {
                            Helper.Input.Suppress(e.Button);
                            return;
                        }
                    }
                    foreach(var obj in GetScarecrows(Game1.currentLocation))
                    {
                        var distance = Vector2.Distance(obj.TileLocation * 64, Game1.player.position.Value);
                        if (distance > 64)
                            continue;
                        if (ReturnOrDropScarecrow(Game1.currentLocation, (int)obj.TileLocation.X, (int)obj.TileLocation.Y, Game1.player, false))
                        {
                            Helper.Input.Suppress(e.Button);
                            return;
                        }
                    }
                }
            }
            else if (e.Button == Config.ActivateButton && Context.CanPlayerMove)
            {
                if (Config.ActivateNearby || Constants.TargetPlatform == GamePlatform.Android)
                {
                    foreach (var obj in GetSprinklers(Game1.currentLocation))
                    {
                        if (Config.ActivateNearbyRange > 0)
                        {
                            var distance = Vector2.Distance(obj.TileLocation * 64, Game1.player.position.Value);
                            if (distance > 64 * Config.ActivateNearbyRange)
                                continue;
                        }
                        obj.Location = Game1.currentLocation;
                        ActivateSprinkler(Game1.currentLocation, obj.TileLocation, obj, false);
                        Helper.Input.Suppress(e.Button);
                    }
                }
                else
                {

                    Point tile = GetMouseCornerTile();

                    var obj = GetSprinklerCached(Game1.currentLocation, tile.X, tile.Y);
                    if (obj is not null)
                    {
                        obj.Location = Game1.currentLocation;
                        ActivateSprinkler(Game1.currentLocation, tile.ToVector2(), obj, false);
                        Helper.Input.Suppress(e.Button);
                    }
                }
            }
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
                name: () => SHelper.Translation.Get("GMCM.EnableMod.Name"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.ShowRangeWhenPlacing.Name"),
                getValue: () => Config.ShowRangeWhenPlacing,
                setValue: value => Config.ShowRangeWhenPlacing = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.PickupButton.Name"),
                getValue: () => Config.PickupButton,
                setValue: value => Config.PickupButton = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.PickupNearby.Name"),
                getValue: () => Config.PickupNearby,
                setValue: value => Config.PickupNearby = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.ActivateButton.Name"),
                getValue: () => Config.ActivateButton,
                setValue: value => Config.ActivateButton = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.ActivateNearby.Name"),
                getValue: () => Config.ActivateNearby,
                setValue: value => Config.ActivateNearby = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.ActivateNearbyRange.Name"),
                getValue: () => Config.ActivateNearbyRange,
                setValue: value => Config.ActivateNearbyRange = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.ShowSprinklerRangeButton.Name"),
                getValue: () => Config.ShowSprinklerRangeButton,
                setValue: value => Config.ShowSprinklerRangeButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.ShowScarecrowRangeButton.Name"),
                getValue: () => Config.ShowScarecrowRangeButton,
                setValue: value => Config.ShowScarecrowRangeButton = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.Scale.Name"),
                getValue: () => Config.Scale + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.Scale = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.Alpha.Name"),
                getValue: () => Config.Alpha + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.Alpha = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.RangeAlpha.Name"),
                getValue: () => Config.RangeAlpha + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.RangeAlpha = f; } }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.DrawOffsetX.Name"),
                getValue: () => Config.DrawOffsetX,
                setValue: value => Config.DrawOffsetX = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.DrawOffsetY.Name"),
                getValue: () => Config.DrawOffsetY,
                setValue: value => Config.DrawOffsetY = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.DrawOffsetZ.Name"),
                getValue: () => Config.DrawOffsetZ,
                setValue: value => Config.DrawOffsetZ = value
            );
        }
    }
}
