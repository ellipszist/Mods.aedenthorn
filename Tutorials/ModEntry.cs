using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Tutorials
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string dictPath = "aedenthorn.Tutorials/dict";
        public const string triggersPath = "aedenthorn.Tutorials/triggers";

        public static Dictionary<string, ITutorialData> TutorialDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, ITutorialData>>(dictPath);
            }
        }

        public static Dictionary<string, TutorialTrigger> TutorialTriggerDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, TutorialTrigger>>(triggersPath);
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
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }
        public override object GetApi()
        {
            return new TutorialAPI();
        }
        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Context.IsPlayerFree)
            {
                if (Config.OpenTutorialKey.JustPressed())
                {
                    OpenTutorial();
                }
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Config.Debug)
            {
                if (Context.IsWorldReady)
                {
                    if (e.Button == SButton.NumPad7)
                    {
                        SHelper.GameContent.InvalidateCache(dictPath);
                        OpenTutorial();
                    }
                }
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.StartsWith("aedenthorn.Tutorials/picture"))
            {
                e.LoadFromModFile<Texture2D>(e.NameWithoutLocale.ToString().Replace("aedenthorn.Tutorials/picture", "assets/"), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(triggersPath))
            {
                e.LoadFrom(() => new Dictionary<string, TutorialTrigger>(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("aedenthorn.LauncherDrawer/dict"))
            {
                e.Edit((IAssetData data) =>
                {
                    data.AsDictionary<string, Dictionary<string, object>>().Data["aedenthorn.Tutorials"] = new()
                    {
                        { "Name", SHelper.Translation.Get("tutorials") },
                        { "Description", SHelper.Translation.Get("open-tutorials") },
                        { "Action", new Action(() => OpenTutorial()) }
                    };
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, ITutorialData>()
                {
                    { "aedenthorn.Test/1", new TutorialData()
                        {
                            Title = "Farming Basics",
                            Category = "Farming",
                            Frames = new() 
                            {
                                new TutorialFrame()
                                {
                                    Texture = "aedenthorn.Tutorials/picturefireball",
                                    StartRect = new Rectangle(0, 0, 1970, 1109),
                                    Text = "To cast fireball, draw up, down-right, up-right, down.",
                                    Frames = 4,
                                    FrameRate = 60
                                }
                            }
                        }
                    },
                    { "aedenthorn.Test/2", new TutorialData()
                        {
                            Title = "Cooking Basics",
                            Category = "Cooking",
                            Frames = new() 
                            {
                                new TutorialFrame()
                                {
                                    Texture = "aedenthorn.Tutorials/picturefireball",
                                    StartRect = new Rectangle(0, 0, 1970, 1109),
                                    Text = "To cast fireball, draw up, down-right, up-right, down.",
                                    Frames = 4,
                                    FrameRate = 60
                                }
                            }
                        }
                    },
                    { "aedenthorn.Test/3", new TutorialData()
                        {
                            Title = "Mining Basics",
                            Category = "Mining",
                            Frames = new() 
                            {
                                new TutorialFrame()
                                {
                                    Texture = "aedenthorn.Tutorials/picturefireball",
                                    StartRect = new Rectangle(0, 0, 1970, 1109),
                                    Text = "To cast fireball, draw up, down-right, up-right, down.",
                                    Frames = 4,
                                    FrameRate = 60
                                }
                            }
                        }
                    },
                    { "aedenthorn.Test/4", new TutorialData()
                        {
                            Title = "Foraging Basics",
                            Category = "Foraging",
                            Frames = new() 
                            {
                                new TutorialFrame()
                                {
                                    Texture = "aedenthorn.Tutorials/picturefireball",
                                    StartRect = new Rectangle(0, 0, 1970, 1109),
                                    Text = "To cast fireball, draw up, down-right, up-right, down.",
                                    Frames = 4,
                                    FrameRate = 60
                                }
                            }
                        }
                    },
                    { "aedenthorn.Test/5", new TutorialData()
                        {
                            Title = "Combat Basics",
                            Category = "Combat",
                            Frames = new() 
                            {
                                new TutorialFrame()
                                {
                                    Texture = "aedenthorn.Tutorials/picturefireball",
                                    StartRect = new Rectangle(0, 0, 1970, 1109),
                                    Text = "To cast fireball, draw up, down-right, up-right, down.",
                                    Frames = 4,
                                    FrameRate = 60
                                }
                            }
                        }
                    },
                    { "aedenthorn.Test/6", new TutorialData()
                        {
                            Title = "Fishing Basics",
                            Category = "Fishing",
                            Frames = new() 
                            {
                                new TutorialFrame()
                                {
                                    Texture = "aedenthorn.Tutorials/picturefireball",
                                    StartRect = new Rectangle(0, 0, 1970, 1109),
                                    Text = "To cast fireball, draw up, down-right, up-right, down.",
                                    Frames = 4,
                                    FrameRate = 60
                                }
                            }
                        }
                    },
                    { "aedenthorn.Test/7", new TutorialData()
                        {
                            Title = "Friendship Basics",
                            Category = "Friendship",
                            Frames = new() 
                            {
                                new TutorialFrame()
                                {
                                    Texture = "aedenthorn.Tutorials/picturefireball",
                                    StartRect = new Rectangle(0, 0, 1970, 1109),
                                    Text = "To cast fireball, draw up, down-right, up-right, down.",
                                    Frames = 4,
                                    FrameRate = 60
                                }
                            }
                        }
                    },
                    { "aedenthorn.Test/8", new TutorialData()
                        {
                            Title = "Bundle Basics",
                            Category = "Bundles",
                            Frames = new() 
                            {
                                new TutorialFrame()
                                {
                                    Texture = "aedenthorn.Tutorials/picturefireball",
                                    StartRect = new Rectangle(0, 0, 1970, 1109),
                                    Text = "To cast fireball, draw up, down-right, up-right, down.",
                                    Frames = 4,
                                    FrameRate = 60
                                }
                            }
                        }
                    },
                    { "aedenthorn.Test/9", new TutorialData()
                        {
                            Title = "Ranching Basics",
                            Category = "Ranching",
                            Frames = new() 
                            {
                                new TutorialFrame()
                                {
                                    Texture = "aedenthorn.Tutorials/picturefireball",
                                    StartRect = new Rectangle(0, 0, 1970, 1109),
                                    Text = "To cast fireball, draw up, down-right, up-right, down.",
                                    Frames = 4,
                                    FrameRate = 60
                                }
                            }
                        }
                    }
                }, AssetLoadPriority.Exclusive);
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

                var exclude = new List<string>()
                {
                    "Debug"
                };
                var props = typeof(ModConfig).GetProperties().ToArray();
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");


                foreach (var p in props)
                {
                    if (exclude.Contains(p.Name))
                        continue;
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