using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Tools;
using StardewValley.Monsters;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AreaOfEffect
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string toolsPath = "aedenthorn.AreaOfEffect/tools";
        public const string effectsPath = "aedenthorn.AreaOfEffect/effects";
        public const string spellsPath = "aedenthorn.AreaOfEffect/spells";
        public const string testWand = "aedenthorn.AreaOfEffect/wand";
        public const string testWand2 = "aedenthorn.AreaOfEffect/wand2";
        public const string chargesKey = "aedenthorn.AreaOfEffect/charges";
        public const string effectKey = "aedenthorn.AreaOfEffect/effect";

        public static Dictionary<Monster, MonsterBuffManager> BuffDict = new();

        public static Dictionary<string, AOEEffectData> EffectDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, AOEEffectData>>(effectsPath);
            }
        }

        public static Dictionary<string, AOEToolData> ToolDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, AOEToolData>>(toolsPath);
            }
        }

        public static Dictionary<string, AOESpellData> SpellDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, AOESpellData>>(spellsPath);
            }
        }

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (BuffDict.Any())
            {
                if (!Context.IsWorldReady)
                {
                    BuffDict.Clear();
                    return;
                }
                foreach (var key in BuffDict.Keys.ToArray())
                {
                    var m = BuffDict[key];
                    if (m.Update(Game1.currentGameTime))
                    {
                        BuffDict.Remove(key);
                    }
                }
            }
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Context.IsPlayerFree)
            {
                if (Config.CastButton.JustPressed() && Game1.player.CurrentTool is Tool t && TryGetTool(t, out var data) && data.Type == null)
                {
                    Game1.activeClickableMenu = new CastSpellMenu(t);
                    foreach(var k in Config.CastButton.Keybinds)
                    {
                        foreach (var b in k.Buttons)
                        {
                            SHelper.Input.Suppress(b);
                        }
                    }
                }
            }
        }

        public override object GetApi()
        {
            return new AOEAPI();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Config.Debug)
            {
                if (Context.IsWorldReady)
                {
                    //SHelper.GameContent.InvalidateCache(spellsPath);
                    if (e.Button == SButton.NumPad4)
                    {
                        File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "test.json"), JsonConvert.SerializeObject(EffectDict, Formatting.Indented));
                    }
                }
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(toolsPath))
            {
                e.LoadFrom(() => new Dictionary<string, AOEToolData>()
                {
                    {
                        testWand,
                        new()
                        {
                            MaxCharges = 100,
                            RechargeItem = "768",
                            RechargeAmount = 10,
                            MaxDistance = 10,
                            Type = "Fireball",
                            RechargeSound = "cowboy_powerup",
                            ChargeColor = Color.OrangeRed
                        }
                    },
                    {
                        testWand2,
                        new()
                        {
                            MaxCharges = 100,
                            RechargeItem = "768",
                            RechargeAmount = 10,
                            MaxDistance = 10,
                            RechargeSound = "cowboy_powerup",
                            ChargeColor = Color.CornflowerBlue
                        }
                    },
                },
                AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(spellsPath))
            {
                e.LoadFrom(() => new Dictionary<string, AOESpellData>()
                {
                    {
                        "Fireball",
                        new()
                        {
                            Type = "Fireball",
                            Sound = "fireball",
                            DisplayName = "Fireball",
                            Sequence = new()
                            {
                                SpellDirection.Up,
                                SpellDirection.DownRight,
                                SpellDirection.UpRight,
                                SpellDirection.Down
                            }
                        }
                    },
                    {
                        "Heal",
                        new()
                        {
                            Type = "Heal",
                            Sound = "yoba",
                            DisplayName = "Heal",
                            Sequence = new()
                            {
                                SpellDirection.UpLeft,
                                SpellDirection.UpRight,
                                SpellDirection.DownRight,
                                SpellDirection.UpRight,
                                SpellDirection.DownRight,
                                SpellDirection.DownLeft
                            }
                        }
                    },
                },
                AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(effectsPath))
            {
                e.LoadFrom(() => new Dictionary<string, AOEEffectData>()
                {
                    {
                        "Fireball",
                        new()
                        {
                            Radius = 3,
                            CastSound = "fireball",
                            Sprites = new()
                            {
                                new()
                                {
                                    Type = SpriteType.Fire
                                }
                            },
                            Effects = new()
                            {
                                new()
                                {
                                    EffectType = AOEEffectType.Damage,
                                    Affected = new()
                                    {
                                        AOEAffectedType.Monster,
                                        AOEAffectedType.Farmer
                                    },
                                    Value = 20
                                },
                                new()
                                {
                                    EffectType = AOEEffectType.Burn,
                                    Affected = new()
                                    {
                                        AOEAffectedType.Twig,
                                        AOEAffectedType.Weed,
                                        AOEAffectedType.Grass,
                                        AOEAffectedType.Tree
                                    }
                                }
                            }
                        }
                    },
                    {
                        "Heal",
                        new()
                        {
                            Radius = 3,
                            CastSound = "yoba",
                            Sprites = new()
                            {
                                new()
                                {
                                    Type = SpriteType.Heart
                                }
                            },
                            Effects = new()
                            {
                                new()
                                {
                                    EffectType = AOEEffectType.Heal,
                                    Affected = new()
                                    {
                                        AOEAffectedType.Monster,
                                        AOEAffectedType.Farmer
                                    },
                                    Value = 20
                                }
                            }
                        }
                    },
                },
                AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Tools"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.AsDictionary<string, ToolData>().Data;
                    dict[testWand] = new()
                    {
                        ClassName = "GenericTool",
                        Name = "Test Wand",
                        DisplayName = "Wand of Fireball",
                        Description = "Shoots exploding balls of fire. Recharge with solar essence.",
                        Texture = testWand,
                        SpriteIndex = 0
                    };
                    dict[testWand2] = new()
                    {
                        ClassName = "GenericTool",
                        Name = "Test Wand 2",
                        DisplayName = "Magic Wand",
                        Description = "Casts spells. Recharge with solar essence.",
                        Texture = testWand,
                        SpriteIndex = 0
                    };
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(testWand))
            {
                e.LoadFromModFile<Texture2D>("assets/weapon.png", AssetLoadPriority.Exclusive);
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