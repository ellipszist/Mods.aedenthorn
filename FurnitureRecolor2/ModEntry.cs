using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace FurnitureRecolor
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string dictPath = "aedenthorn.FurnitureRecolor/dict";
        public const string colorsKey = "aedenthorn.FurnitureRecolor/colors";

        public static Dictionary<string, Dictionary<Color, Texture2D>> tileSheetDict = new();
        public static Dictionary<string, List<Color>> parsedFurniture = new();

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Content.AssetReady += Content_AssetReady;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetReady(object sender, AssetReadyEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/furniture") || e.NameWithoutLocale.IsEquivalentTo("TileSheets/furniture_2") || e.NameWithoutLocale.IsEquivalentTo("TileSheets/furniture_3") || e.NameWithoutLocale.IsEquivalentTo("TileSheets/furnitureFront") || e.NameWithoutLocale.IsEquivalentTo("TileSheets/furniture_2Front") || e.NameWithoutLocale.IsEquivalentTo("TileSheets/furniture_3Front") || SHelper.GameContent.Load<Dictionary<string, string>>(dictPath).ContainsKey(e.NameWithoutLocale.ToString()))
            {
                var t = SHelper.GameContent.Load<Texture2D>(e.NameWithoutLocale);
                Color[] pixels = new Color[t.Width * t.Height];
                t.GetData(pixels);
                Dictionary<Color, List<Color>> colorDict = new();
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i] != Color.Transparent && !colorDict.TryGetValue(pixels[i], out var list))
                    {
                        list = new();
                        colorDict[pixels[i]] = list;
                    }
                }
                for (int i = 0; i < pixels.Length; i++)
                {
                    foreach (var key in colorDict.Keys)
                    {
                        if (key == pixels[i])
                        {
                            colorDict[key].Add(Color.White);
                        }
                        else
                            colorDict[key].Add(Color.Transparent);
                    }
                }

                Dictionary<Color, Texture2D> textureDict = new();
                foreach (var kvp in colorDict)
                {
                    var tex = new Texture2D(Game1.graphics.GraphicsDevice, t.Width, t.Height);
                    tex.SetData(kvp.Value.ToArray());
                    textureDict[kvp.Key] = tex;
                }
                tileSheetDict[e.NameWithoutLocale.ToString()] = textureDict;
            }
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if(!Config.ModEnabled) 
                return;
            if(Context.IsPlayerFree && Config.ColorButton.JustPressed())
            {
                parsedFurniture.Clear();
                var tile = Game1.currentCursorTile;
                Furniture f = Game1.currentLocation.GetFurnitureAt(tile);
                if (f == null)
                    return;
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(f.QualifiedItemId);
                var tn = itemData.TextureName.Replace("\\", "/");
                if (itemData.IsErrorItem || !tileSheetDict.TryGetValue(tn, out var dict))
                {
                    return;
                }
                if(!parsedFurniture.TryGetValue(f.ItemId, out var list))
                {
                    list = new();
                    Texture2D texture = itemData.GetTexture();
                    Color[] data = new Color[texture.Width * texture.Height];
                    texture.GetData(data);
                    var rect = itemData.GetSourceRect();
                    for(int x = 0; x < texture.Width; x++)
                    {
                        for (int y = 0; y < texture.Width; y++)
                        {
                            var c = data[x + y * texture.Width];
                            if (c != Color.Transparent && !list.Contains(c))
                                list.Add(c);
                        }
                    }
                    parsedFurniture[f.ItemId] = list;
                }
                Game1.activeClickableMenu = new ColorPickMenu(f, list);
                foreach (var bind in Config.ColorButton.Keybinds)
                {
                    foreach (var key in bind.Buttons)
                    {
                        SHelper.Input.Suppress(key);
                    }
                }
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if(!Config.ModEnabled) 
                return;
            
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
            }
        }


        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
        
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
                    else if (p.PropertyType == typeof(KeybindList))
                    {
                        configMenu.AddKeybindList(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (KeybindList)p.GetValue(Config),
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