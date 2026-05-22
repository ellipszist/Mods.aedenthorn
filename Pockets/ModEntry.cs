using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Fences;
using StardewValley.Inventories;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Pockets
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        private static Dictionary<Item, Dictionary<string, Inventory>> InventoryDict = new();
        private static Dictionary<long, Dictionary<string, Inventory>> DefaultInventoryDict = new ();
        public const string dictPath = "aedenthorn.Pockets/dict";
        public const string modKey = "aedenthorn.Pockets/pocket";
        public static PocketData openPocket;

        public static Dictionary<string, Dictionary<string, PocketData>> PocketDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, Dictionary<string, PocketData>>>(dictPath);
            }
        }
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.Saving += GameLoop_Saving;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if(e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, Dictionary<string, PocketData>>(), AssetLoadPriority.Medium);
            }
        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            openPocket = null;
        }

        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            foreach(var kvp in InventoryDict)
            {
                kvp.Key.modData[modKey] = JsonConvert.SerializeObject(kvp.Value);
            }
            foreach(var kvp in DefaultInventoryDict)
            {
                Game1.GetPlayer(kvp.Key).modData[modKey] = JsonConvert.SerializeObject(kvp.Value);
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {   
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
                    name: () => SHelper.Translation.Get("ModEnabled"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
            }
        }
    }
}