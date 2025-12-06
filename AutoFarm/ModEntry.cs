using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Object = StardewValley.Object;

namespace AutoFarm
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static string plotsKey = "aedenthorn.AutoFarm/plots";
        public static Dictionary<GameLocation, List<AutoPlot>> locationDict = new Dictionary<GameLocation, List<AutoPlot>>();

        public static PerScreen<Rectangle?> draggingRect = new PerScreen<Rectangle?>(() => null);
        public static PerScreen<Vector2> startTile = new PerScreen<Vector2>(() => new Vector2(-1,-1));

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();


            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsWorldReady || Game1.currentLocation is null || Game1.activeClickableMenu is not AutoFarmMenu || !TryGetAutoPlots(out var list))
                return;
            foreach (var plot in list)
            {
                foreach(var t in plot.tiles)
                {
                }
            }
        }

        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if (!Config.EnableMod || Game1.currentLocation?.IsFarm != true)
                return;
            if (Config.MenuKey.JustPressed())
            {
                if (Game1.activeClickableMenu == null)
                {
                    Game1.activeClickableMenu = new AutoFarmMenu();
                }
            }
            else if (Game1.activeClickableMenu is AutoFarmMenu)
            {
                Vector2 tile = Game1.currentCursorTile;
                if (!Game1.activeClickableMenu.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
                {
                    if (e.Pressed.Contains(Config.CreateKey))
                    {
                        SHelper.Input.Suppress(Config.CreateKey);
                        draggingRect.Value = new Rectangle((int)tile.X, (int)tile.Y, 1, 1);
                        startTile.Value = tile;
                        return;
                    }
                    if (e.Released.Contains(Config.CreateKey))
                    {
                        if (draggingRect.Value != null)
                        {
                            AddAutoPlot(draggingRect.Value.Value, startTile.Value);
                            draggingRect.Value = null;
                            return;
                        }
                    }
                    else if(!e.Held.Contains(Config.CreateKey))
                    {
                        draggingRect.Value = null;
                        return;
                    }
                }
                draggingRect.Value = null;
            }
        }


        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                name: () => SHelper.Translation.Get("Config.EnableMod"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.MenuKey"),
                getValue: () => Config.MenuKey,
                setValue: value => Config.MenuKey= value
            );
        }
    }
}
