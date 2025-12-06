using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Object = StardewValley.Object;

namespace BuildingShift
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static string shiftKey = "aedenthorn.BuildingShift/shift";


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();


            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            foreach (var t in typeof(Game1).Assembly.GetTypes())
            {
                if(t == typeof(Building) || t.IsSubclassOf(typeof(Building)))
                {
                    foreach (var m in new MethodInfo[]{ t.GetMethod("draw"), t.GetMethod("drawShadow"), t.GetMethod("drawBackground"), t.GetMethod("DrawEntranceTiles") })
                    {
                        if (m != null && m.DeclaringType == t)
                        {
                            SMonitor.Log($"Transpiling {t.Name}.{m.Name}");

                            harmony.Patch(
                                m,
                                transpiler: new HarmonyMethod(typeof(ModEntry).GetMethod("Draw_Transpiler"))
                            );
                        }
                    }
                }
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree || Game1.currentLocation?.buildings  == null)
                return;
            if (e.Button == Config.ShiftDown)
            {
                Building b = GetHoveredBuilding();
                if (b != null)
                {
                    ShiftBuilding(b, 0, 1);
                }
            }
            else if (e.Button == Config.ShiftUp)
            {
                Building b = GetHoveredBuilding();
                if (b != null)
                {
                    ShiftBuilding(b, 0, -1);
                }
            }
            else if (e.Button == Config.ShiftRight)
            {
                Building b = GetHoveredBuilding();
                if (b != null)
                {
                    ShiftBuilding(b, 1, 0);
                }
            }
            else if (e.Button == Config.ShiftLeft)
            {
                Building b = GetHoveredBuilding();
                if (b != null)
                {
                    ShiftBuilding(b, -1, 0);
                }
            }
            else if (e.Button == Config.ResetKey)
            {
                Building b = GetHoveredBuilding();
                if (b != null)
                {
                    SMonitor.Log($"{b.buildingType.Value} shift reset");
                    b.modData[shiftKey] = "0,0";
                }
            }
        }


        private Building GetHoveredBuilding()
        {
            var x = Game1.viewport.X + Game1.getOldMouseX();
            var y = Game1.viewport.Y + Game1.getOldMouseY();
            using (List<Building>.Enumerator enumerator = Game1.currentLocation.buildings.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var building = enumerator.Current;
                    if(TryGetShift(building, out var amount))
                    {
                        x -= (int)amount.X;
                        y -= (int)amount.Y;
                    }
                    if(building.occupiesTile(new Vector2(x / 64, y / 64)))
                        return building;
                }
            }
            return null;
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

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.ShiftAmountNormal"),
                getValue: () => Config.ShiftAmountNormal,
                setValue: value => Config.ShiftAmountNormal = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.ShiftAmountMod"),
                getValue: () => Config.ShiftAmountMod,
                setValue: value => Config.ShiftAmountMod = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.ShiftUp"),
                getValue: () => Config.ShiftUp,
                setValue: value => Config.ShiftUp = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.ShiftDown"),
                getValue: () => Config.ShiftDown,
                setValue: value => Config.ShiftDown = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.ShiftLeft"),
                getValue: () => Config.ShiftLeft,
                setValue: value => Config.ShiftLeft = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.ShiftRight"),
                getValue: () => Config.ShiftRight,
                setValue: value => Config.ShiftRight = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.ModKey"),
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.ResetKey"),
                getValue: () => Config.ResetKey,
                setValue: value => Config.ResetKey = value
            );
        }
    }
}
