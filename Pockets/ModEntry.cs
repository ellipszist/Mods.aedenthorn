using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using System.Collections.Generic;

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
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.Saving += GameLoop_Saving;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(!Config.ModEnabled || !Context.IsWorldReady || 
                (Game1.activeClickableMenu is not null && Game1.activeClickableMenu is not GameMenu) || 
                (Game1.activeClickableMenu is null && Config.HoverForHotkey && !new Rectangle(Game1.GlobalToLocal(Game1.player.Position).ToPoint() + new Point(0, -64), new Point(64, 128)).Contains(Game1.getMousePosition())) || 
                !TryGetPocket(Game1.player, out var data, out var inv))
            {
                return;
            }
            GameMenu menu = Game1.activeClickableMenu as GameMenu;
            if (Game1.activeClickableMenu is null)
            {
                menu = new GameMenu(GameMenu.inventoryTab, -1, true);
                Game1.activeClickableMenu = menu;
            }
            else if(menu.currentTab != GameMenu.inventoryTab) 
            {
                menu.changeTab(GameMenu.inventoryTab);
            }
            OpenPocket(menu.GetCurrentPage() as InventoryPage, data, inv, true);
            SHelper.Input.Suppress(e.Button);
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            InventoryDict.Clear();
            DefaultInventoryDict.Clear();
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            InventoryDict.Clear();
            DefaultInventoryDict.Clear();
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
            if(e.NewMenu is not GameMenu menu || menu.currentTab != GameMenu.inventoryTab)
                openPocket = null;
        }

        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            foreach(var kvp in InventoryDict)
            {
                kvp.Key.modData[modKey] = MakeXMLFromInventories(kvp.Value);
            }
            foreach(var kvp in DefaultInventoryDict)
            {
                Game1.GetPlayer(kvp.Key).modData[modKey] = MakeXMLFromInventories(kvp.Value);
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