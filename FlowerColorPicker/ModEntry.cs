using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Globalization;
using System.Linq;

namespace FlowerColorPicker
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static IPrismaticFlowersAPI prismaticFlowersAPI;
        public const string prismaticKey = "aedenthorn.PrismaticFlowers/prismatic";
        public const string prismaticKeyDisabled = "aedenthorn.PrismaticFlowers/prismatic_";

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Config.Debug && e.Button == SButton.OemPeriod)
            {
                foreach (var kvp in Game1.getFarm().terrainFeatures.Pairs.Where(kvp => kvp.Value is HoeDirt))
                {
                    //var crop = new Crop(Game1.random.Choose("455", "453", "429", "427", "425", "431"), (int)kvp.Key.X, (int)kvp.Key.Y, Game1.getFarm());
                    var crop = new Crop("455", (int)kvp.Key.X, (int)kvp.Key.Y, Game1.getFarm());
                    crop.growCompletely();
                    (kvp.Value as HoeDirt).crop = crop;
                }
            }
        }

        private void Input_MouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree || !Game1.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out var tf) || tf is not HoeDirt dirt || dirt.crop is not Crop crop || !crop.programColored.Value)
                return;
            if (SHelper.Input.IsDown(Config.PrismaticModKey))
            {
                if (crop.modData.TryGetValue(prismaticKey, out var prismatic))
                {
                    crop.modData[prismaticKeyDisabled] = prismatic;
                    crop.modData.Remove(prismaticKey);
                    Game1.playSound("yoba");
                    SHelper.Input.SuppressScrollWheel();
                }
                else if (crop.modData.TryGetValue(prismaticKeyDisabled, out var prismatic2))
                {
                    crop.modData[prismaticKey] = prismatic2;
                    crop.modData.Remove(prismaticKeyDisabled);
                    Game1.playSound("yoba");
                    SHelper.Input.SuppressScrollWheel();
                }
                else if (prismaticFlowersAPI?.MakePrismatic(crop) == true)
                {
                    Game1.playSound("yoba");
                    SHelper.Input.SuppressScrollWheel();
                }
            }
            else
            {
                var tintColors = crop.GetData()?.TintColors;
                var newColor = GetNewColor(tintColors, crop.tintColor.Value, e.Delta);
                if (newColor is null)
                    return;
                crop.tintColor.Value = newColor.Value;
                Game1.playSound("shiny4");
                SHelper.Input.SuppressScrollWheel();
            }
        }

        

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            prismaticFlowersAPI = Helper.ModRegistry.GetApi<IPrismaticFlowersAPI>("aedenthorn.PrismaticFlowers");

            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
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
                var props = typeof(ModConfig).GetProperties().ToArray();
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");

                foreach (var p in props)
                {
                    if (p.Name == nameof(Config.ModEnabled) || p.Name == nameof(Config.Debug))
                        continue;
                    if (p.PropertyType == typeof(bool))
                    {
                        configMenu.AddBoolOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (bool)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (int)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) { p.SetValue(Config, f); } }
                        );
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) { p.SetValue(Config, d); } }
                        );
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (string)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(SButton))
                    {
                        configMenu.AddKeybind(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (SButton)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(Color) && configMenuExt is not null)
                    {
                        configMenuExt.AddColorOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (Color)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                }
            }
        }
    }
}