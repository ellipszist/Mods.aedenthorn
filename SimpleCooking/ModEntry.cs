using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace SimpleCooking
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string cookingKey = "aedenthorn.SimpleCooking/cooking";
        public const string cookableDictPath = "aedenthorn.SimpleCooking/cookable";
        public const string cookerDictPath = "aedenthorn.SimpleCooking/cooker";
        public const string grilledPrefix = "aedenthorn.SimpleCooking/grilled_";

        public static Dictionary<string, CookableData> CookableDict 
        { 
            get
            {                SHelper.GameContent.InvalidateCache(cookableDictPath);

                return SHelper.GameContent.Load<Dictionary<string, CookableData>>(cookableDictPath);
            } 
        }
        public static Dictionary<string, CookerData> CookerDict 
        { 
            get
            {
                //SHelper.GameContent.InvalidateCache(cookerDictPath);
                return SHelper.GameContent.Load<Dictionary<string, CookerData>>(cookerDictPath);
            } 
        }
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(cookableDictPath))
            {
                e.LoadFrom(() => new Dictionary<string, CookableData>(), AssetLoadPriority.Exclusive);
            }
            if (e.NameWithoutLocale.IsEquivalentTo(cookerDictPath))
            {
                e.LoadFrom(() => new Dictionary<string, CookerData>()
                {
                    { "(BC)143", new() { CookOffset = new(8, -60, 7.5f) } },
                    { "(BC)144", new() { CookOffset = new(8, -60, 7.5f) } },
                    { "(BC)145", new() { CookOffset = new(8, -60, 7.5f) } },
                    { "(BC)146", new() { CookOffset = new(8, -8,  9.5f) } },
                    { "(BC)147", new() { CookOffset = new(8, -60, 7.5f) } },
                    { "(BC)148", new() { CookOffset = new(8, -60, 7.5f) } },
                    { "(BC)150", new() { CookOffset = new(8, -60, 7.5f) } },
                    { "(BC)151", new() { CookOffset = new(8, -60, 7.5f) } }
                }, AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.AsDictionary<string, ObjectData>().Data;
                    foreach (var kvp in dict.Where(kvp => kvp.Value.Edibility > 0 && (kvp.Value.Category == Object.FishCategory || kvp.Value.Category == Object.VegetableCategory)).ToArray())
                    {
                        dict[grilledPrefix + kvp.Key] = new ObjectData()
                        {
                            Name = grilledPrefix + kvp.Key,
                            DisplayName = string.Format(Helper.Translation.Get("grilled-x"), TokenParser.ParseText(kvp.Value.DisplayName)),
                            Description = Helper.Translation.Get("grilled-desc"),
                            Edibility = (int)Math.Round(kvp.Value.Edibility * Config.GrilledEdibilityMult),
                            Price = (int)Math.Round(kvp.Value.Edibility * Config.GrilledPriceMult),
                            Type = "Cooking",
                            Category = -7,
                            Texture = kvp.Value.Texture,
                            SpriteIndex = kvp.Value.SpriteIndex
                        };
                    }
                });
            }
        }
        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            //SMonitor.Log(string.Join(",", Game1.objectData.Where(kvp => kvp.Value.Type == "Arch").Select(kvp => kvp.Key)));
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
                    if (p.PropertyType == typeof(bool))
                    {
                        configMenu.AddBoolOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (bool)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (int)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (float)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) { p.SetValue(Config, d); } }
                        );
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (string)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(KeybindList))
                    {
                        configMenu.AddKeybindList(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (KeybindList)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(SButton))
                    {
                        configMenu.AddKeybind(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (SButton)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(Color) && configMenuExt is not null)
                    {
                        configMenuExt.AddColorOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (Color)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                }
            }
        }

        public static string AddSpaces(string str)
        {
            string newStr = "";
            foreach (var c in str)
            {
                if (c >= 'A' && c <= 'Z' && newStr.Length > 0)
                {
                    newStr += " ";
                }
                newStr += c;
            }
            return newStr;
        }
    }
}