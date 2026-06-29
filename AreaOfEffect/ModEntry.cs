using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
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
        public const string lightKey = "aedenthorn.AreaOfEffect/light";
        public const string texturePrefix = "aedenthorn.AreaOfEffect/textures/";
        public const string internalPrefix = "aedenthorn.AreaOfEffect/internal/";

        public static Dictionary<string, Texture2D> TextureDict { get; set; } = new();
        public static Dictionary<string, SpellLightData> LightDict { get; set; } = new();
        public static Dictionary<Monster, MonsterBuffManager> BuffDict { get; set; } = new();
        public static Dictionary<SpellProjectile, LinearProjectileInstance> ProjectileDict { get; set; } = new();

        public static Dictionary<string, SpellToolData> ToolDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, SpellToolData>>(toolsPath);
            }
        }

        public static Dictionary<string, SpellData> SpellDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, SpellData>>(spellsPath);
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
            if(Game1.activeClickableMenu is null)
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
                if (LightDict.Any())
                {
                    foreach (var key in LightDict.Keys.ToArray())
                    {
                        var m = LightDict[key];
                        if (m.Update(Game1.currentGameTime))
                        {
                            LightDict.Remove(key);
                        }
                    }
                }
            }
            if (ProjectileDict.Any())
            {
                foreach (var p in ProjectileDict.Keys.ToArray())
                {
                    var data = ProjectileDict[p];
                    if(!data.location.projectiles.Contains(p))
                    {
                        ProjectileDict.Remove(p);
                        continue;
                    }
                    foreach(var tile in GetRoundedTiles(p.position.Value / 64))
                    {
                        if (Vector2.Distance(data.firer.Tile, tile) < 2)
                            continue;
                        if (!data.affectedTiles.Contains(tile))
                        {
                            foreach (var effect in data.spell.Effects)
                            {
                                if (effect.PerTile)
                                {
                                    List<object> applied = new();
                                    ApplyEffectToTile(data.location, data.firer, tile, effect, applied);
                                }
                            }
                            foreach (var s in data.spell.Sprites)
                            {
                                if (s.PerTile)
                                {
                                    ApplySpriteToTile(data.location, tile, s, Vector2.Distance(data.target, tile));
                                }
                            }
                            data.affectedTiles.Add(tile);
                        }
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
                if (Config.CastButton.JustPressed() && Game1.player.CurrentTool is Tool t && TryGetTool(t, out var data) && data.Spells.Count != 1)
                {
                    Game1.activeClickableMenu = new CastSpellMenu(t, data.Spells);
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
                        //foreach(var k in TextureDict.Keys)
                        //{
                        //    SHelper.GameContent.InvalidateCache(k);
                        //}
                        //CreateTutorials();
                        //File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "test.json"), JsonConvert.SerializeObject(SpellDict, Formatting.Indented));
                    }
                }
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.StartsWith(texturePrefix))
            {
                if (TextureDict.TryGetValue(e.NameWithoutLocale.ToString(), out Texture2D tex))
                {
                    e.LoadFrom(() => tex, Priority.Last);
                }
            }
            else if (e.NameWithoutLocale.StartsWith(internalPrefix))
            {
                e.LoadFromModFile<Texture2D>(e.NameWithoutLocale.ToString().Replace(internalPrefix, "assets/"), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(toolsPath))
            {
                e.LoadFrom(() => new Dictionary<string, SpellToolData>()
                {
                    {
                        testWand,
                        new()
                        {
                            MaxCharges = 20,
                            RechargeItem = "768",
                            RechargeAmount = 10,
                            MaxDistance = 10,
                            Spells = new(){ "Fireball" },
                            RechargeSound = "cowboy_powerup",
                            ChargesColor = Color.OrangeRed,
                            AuraColor = Color.OrangeRed,
                        }
                    },
                    {
                        testWand2,
                        new()
                        {
                            MaxCharges = 20,
                            RechargeItem = "768",
                            RechargeAmount = 10,
                            MaxDistance = 10,
                            RechargeSound = "cowboy_powerup",
                            ChargesColor = Color.CornflowerBlue
                        }
                    },
                },
                AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(spellsPath))
            {
                e.LoadFrom(() => new Dictionary<string, SpellData>()
                {
                    {
                        "Illuminate",
                        new()
                        {
                            SetSound = "discoverMineral",
                            DisplayName = "Illuminate",
                            Sequence = new()
                            {
                                SpellDirection.DownRight,
                                SpellDirection.Up,
                                SpellDirection.Down,
                                SpellDirection.UpRight
                            },
                            SpellLevels = new()
                            {
                                new()
                                {
                                    CastSound = "discoverMineral",
                                    Radius = 0,
                                    Charges = 1,
                                    Sprites = new()
                                    {
                                        new()
                                        {
                                            Type = SpriteType.Sparkle,
                                            Interval = 300,
                                            Number = 5,
                                        }
                                    },
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Light,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer,
                                                SpellAffectedType.Tile
                                            },
                                            First = true,
                                            Value = 10000,
                                            Color = new Color(0, 50, 170),
                                            Radius = 5
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "discoverMineral",
                                    Radius = 0,
                                    Charges = 2,
                                    Sprites = new()
                                    {
                                        new()
                                        {
                                            Type = SpriteType.Sparkle,
                                            Interval = 300,
                                            Number = 5,
                                        }
                                    },
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Light,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer,
                                                SpellAffectedType.Tile
                                            },
                                            First = true,
                                            Value = 20000,
                                            Color = new Color(0, 50, 170),
                                            Radius = 10
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "discoverMineral",
                                    Radius = 0,
                                    Charges = 3,
                                    Sprites = new()
                                    {
                                        new()
                                        {
                                            Type = SpriteType.Sparkle,
                                            Interval = 300,
                                            Number = 5,
                                        }
                                    },
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Light,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer,
                                                SpellAffectedType.Tile
                                            },
                                            First = true,
                                            Value = 30000,
                                            Color = new Color(0, 50, 170),
                                            Radius = 20
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "Lightning",
                        new()
                        {
                            SetSound = "thunder",
                            DisplayName = "Lightning Bolt",
                            Sequence = new()
                            {
                                SpellDirection.DownLeft,
                                SpellDirection.Right,
                                SpellDirection.DownLeft
                            },
                            SpellLevels = new()
                            {
                                new()
                                {
                                    CastSound = "thunder",
                                    Radius = 1,
                                    Charges = 1,
                                    Sprites = new()
                                    {
                                        new()
                                        {
                                            Type = SpriteType.Lightning,
                                            PerTile = false
                                        }
                                    },
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Light,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Tile
                                            },
                                            Value = 1000,
                                            Color = Color.White,
                                            Radius = 1
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Damage,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer
                                            },
                                            Value = 30
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Burn,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Twig,
                                                SpellAffectedType.Weed,
                                                SpellAffectedType.Grass,
                                                SpellAffectedType.Tree
                                            }
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "thunder",
                                    Charges = 2,
                                    Radius = 2,
                                    Sprites = new()
                                    {
                                        new()
                                        {
                                            Type = SpriteType.Lightning,
                                            PerTile = false
                                        }
                                    },
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Light,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Tile
                                            },
                                            Value = 1000,
                                            Color = Color.White,
                                            Radius = 2
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Damage,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer
                                            },
                                            Value = 50
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Burn,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Twig,
                                                SpellAffectedType.Weed,
                                                SpellAffectedType.Grass,
                                                SpellAffectedType.Tree
                                            }
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "thunder",
                                    Charges = 3,
                                    Radius = 4,
                                    Sprites = new()
                                    {
                                        new()
                                        {
                                            Type = SpriteType.Lightning,
                                            PerTile = false
                                        }
                                    },
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Light,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Tile
                                            },
                                            Value = 1000,
                                            Color = Color.White,
                                            Radius = 4
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Damage,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer
                                            },
                                            Value = 80
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Burn,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Twig,
                                                SpellAffectedType.Weed,
                                                SpellAffectedType.Grass,
                                                SpellAffectedType.Tree
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "Fireball",
                        new()
                        {
                            SetSound = "fireball",
                            DisplayName = "Fireball",
                            Sequence = new()
                            {
                                SpellDirection.Up,
                                SpellDirection.DownRight,
                                SpellDirection.UpRight,
                                SpellDirection.Down
                            },
                            SpellLevels = new()
                            {
                                new()
                                {
                                    CastSound = "fireball",
                                    TriggerSound = "fireball",
                                    Radius = 3,
                                    Projectiles = new()
                                    {
                                        new(){ SpriteIndex = 10 },
                                    },
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
                                            EffectType = SpellEffectType.Damage,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer
                                            },
                                            Value = 20
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Burn,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Twig,
                                                SpellAffectedType.Weed,
                                                SpellAffectedType.Grass,
                                                SpellAffectedType.Tree
                                            }
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "fireball",
                                    TriggerSound = "fireball",
                                    Radius = 5,
                                    Projectiles = new()
                                    {
                                        new(){ SpriteIndex = 10 },
                                    },
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
                                            EffectType = SpellEffectType.Damage,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer
                                            },
                                            Value = 40
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Burn,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Twig,
                                                SpellAffectedType.Weed,
                                                SpellAffectedType.Grass,
                                                SpellAffectedType.Tree
                                            }
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "fireball",
                                    TriggerSound = "fireball",
                                    Radius = 5,
                                    Projectiles = new()
                                    {
                                        new(){ SpriteIndex = 10, MinAngleOffset = -45, MaxAngleOffset = -45 },
                                        new(){ SpriteIndex = 10 },
                                        new(){ SpriteIndex = 10, MinAngleOffset = 45, MaxAngleOffset = 45 }
                                    },
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
                                            EffectType = SpellEffectType.Damage,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer
                                            },
                                            Value = 60
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Burn,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Twig,
                                                SpellAffectedType.Weed,
                                                SpellAffectedType.Grass,
                                                SpellAffectedType.Tree
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "IceStorm",
                        new()
                        {
                            SetSound = "frozen",
                            DisplayName = "Ice Storm",
                            Sequence = new()
                            {
                                SpellDirection.UpLeft,
                                SpellDirection.UpRight,
                                SpellDirection.DownRight,
                                SpellDirection.DownLeft
                            },
                            SpellLevels = new()
                            {
                                new()
                                {
                                    CastSound = "frozen",
                                    Radius = 3,
                                    Sprites = new()
                                    {
                                        new()
                                        {
                                            Type = SpriteType.Ice
                                        }
                                    },
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Damage,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer
                                            },
                                            Value = 10
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Buff,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Farmer
                                            },
                                            Value = "19"
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Freeze,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster
                                            },
                                            Value = 4000
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "frozen",
                                    Radius = 5,
                                    Charges = 2,
                                    Sprites = new()
                                    {
                                        new()
                                        {
                                            Type = SpriteType.Ice
                                        }
                                    },
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Damage,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer
                                            },
                                            Value = 20
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Buff,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Farmer
                                            },
                                            Value = "19"
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Freeze,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster
                                            },
                                            Value = 8000
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "frozen",
                                    Radius = 10,
                                    Charges = 3,
                                    Sprites = new()
                                    {
                                        new()
                                        {
                                            Type = SpriteType.Ice
                                        }
                                    },
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Damage,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer
                                            },
                                            Value = 40
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Buff,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Farmer
                                            },
                                            Value = "19"
                                        },
                                        new()
                                        {
                                            EffectType = SpellEffectType.Freeze,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster
                                            },
                                            Value = 16000
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "Heal",
                        new()
                        {
                            SetSound = "yoba",
                            DisplayName = "Heal",
                            Sequence = new()
                            {
                                SpellDirection.UpLeft,
                                SpellDirection.UpRight,
                                SpellDirection.DownRight,
                                SpellDirection.UpRight,
                                SpellDirection.DownRight,
                                SpellDirection.DownLeft
                            },
                            SpellLevels = new()
                            {
                                new()
                                {
                                    CastSound = "yoba",
                                    Radius = 0,
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
                                            EffectType = SpellEffectType.Heal,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer
                                            },
                                            Value = 20
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "yoba",
                                    Radius = 2,
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
                                            EffectType = SpellEffectType.Heal,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer
                                            },
                                            Value = 40
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "yoba",
                                    Radius = 4,
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
                                            EffectType = SpellEffectType.Heal,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Monster,
                                                SpellAffectedType.Farmer
                                            },
                                            Value = 80
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "Deluge",
                        new()
                        {
                            SetSound = "thunder",
                            DisplayName = "Deluge",
                            Sequence = new()
                            {
                                SpellDirection.DownLeft,
                                SpellDirection.Down,
                                SpellDirection.Right,
                                SpellDirection.Up,
                                SpellDirection.UpLeft
                            },
                            SpellLevels = new()
                            {
                                new()
                                {
                                    CastSound = "thunder",
                                    Radius = 3,
                                    Sprites = new()
                                    {
                                        new()
                                        {
                                            Type = SpriteType.Fountain,
                                            PerTile = false
                                        }
                                    },
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Water,
                                            Affected = new()
                                            {
                                                SpellAffectedType.HoeDirt
                                            }
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "thunder",
                                    Radius = 5,
                                    Charges = 2,
                                    Sprites = new()
                                    {
                                        new()
                                        {
                                            Type = SpriteType.Fountain,
                                            PerTile = false
                                        }
                                    },
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Water,
                                            Affected = new()
                                            {
                                                SpellAffectedType.HoeDirt
                                            }
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "thunder",
                                    Radius = 10,
                                    Charges = 3,
                                    Sprites = new()
                                    {
                                        new()
                                        {
                                            Type = SpriteType.Fountain,
                                            PerTile = false
                                        }
                                    },
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Water,
                                            Affected = new()
                                            {
                                                SpellAffectedType.HoeDirt
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "EtherealAxe",
                        new()
                        {
                            SetSound = "axchop",
                            DisplayName = "Ethereal Axe",
                            Sequence = new()
                            {
                                SpellDirection.Up,
                                SpellDirection.UpRight,
                                SpellDirection.Down,
                                SpellDirection.UpLeft
                            },
                            SpellLevels = new()
                            {
                                new()
                                {
                                    CastSound = "axchop",
                                    Radius = 3,
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Tool,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Crop,
                                                SpellAffectedType.Object,
                                                SpellAffectedType.ResourceClump,
                                                SpellAffectedType.Tree,
                                            },
                                            Value = "(T)CopperAxe"
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "axchop",
                                    Radius = 5,
                                    Charges = 2,
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Tool,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Crop,
                                                SpellAffectedType.Object,
                                                SpellAffectedType.ResourceClump,
                                                SpellAffectedType.Tree,
                                            },
                                            Value = "(T)GoldAxe"
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "axchop",
                                    Radius = 10,
                                    Charges = 3,
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Tool,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Crop,
                                                SpellAffectedType.Object,
                                                SpellAffectedType.ResourceClump,
                                                SpellAffectedType.Tree,
                                            },
                                            Value = "(T)IridiumAxe"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "EtherealPickaxe",
                        new()
                        {
                            SetSound = "hammer",
                            DisplayName = "Ethereal Pickaxe",
                            Sequence = new()
                            {
                                SpellDirection.Up,
                                SpellDirection.DownLeft,
                                SpellDirection.UpRight,
                                SpellDirection.DownRight
                            },
                            SpellLevels = new()
                            {
                                new()
                                {
                                    CastSound = "hammer",
                                    Radius = 3,
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Tool,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Crop,
                                                SpellAffectedType.Object,
                                                SpellAffectedType.ResourceClump,
                                                SpellAffectedType.Tree,
                                            },
                                            Value = "(T)CopperPickaxe"
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "hammer",
                                    Radius = 5,
                                    Charges = 2,
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Tool,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Crop,
                                                SpellAffectedType.Object,
                                                SpellAffectedType.ResourceClump,
                                                SpellAffectedType.Tree,
                                            },
                                            Value = "(T)GoldPickaxe"
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "hammer",
                                    Radius = 10,
                                    Charges = 3,
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Tool,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Crop,
                                                SpellAffectedType.Object,
                                                SpellAffectedType.ResourceClump,
                                                SpellAffectedType.Tree,
                                            },
                                            Value = "(T)IridiumPickaxe"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "EtherealHoe",
                        new()
                        {
                            SetSound = "hoeHit",
                            DisplayName = "Ethereal Hoe",
                            Sequence = new()
                            {
                                SpellDirection.Up,
                                SpellDirection.UpLeft,
                                SpellDirection.Right,
                                SpellDirection.DownLeft  
                            },
                            SpellLevels = new()
                            {
                                new()
                                {
                                    CastSound = "hoeHit",
                                    Radius = 3,
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Tool,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Crop,
                                                SpellAffectedType.Object,
                                                SpellAffectedType.ResourceClump,
                                                SpellAffectedType.Tree,
                                            },
                                            Value = "(T)CopperHoe"
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "hoeHit",
                                    Radius = 5,
                                    Charges = 2,
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Tool,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Crop,
                                                SpellAffectedType.Object,
                                                SpellAffectedType.ResourceClump,
                                                SpellAffectedType.Tree,
                                            },
                                            Value = "(T)GoldHoe"
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "hoeHit",
                                    Radius = 10,
                                    Charges = 3,
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Tool,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Crop,
                                                SpellAffectedType.Object,
                                                SpellAffectedType.ResourceClump,
                                                SpellAffectedType.Tree,
                                            },
                                            Value = "(T)IridiumHoe"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "Meteor",
                        new()
                        {
                            SetSound = "explosion",
                            DisplayName = "Meteor Strike",
                            Sequence = new()
                            {
                                SpellDirection.Down,
                                SpellDirection.DownLeft,
                                SpellDirection.Right,
                                SpellDirection.UpLeft
                            },
                            SpellLevels = new()
                            {
                                new()
                                {
                                    CastSound = "explosion",
                                    Radius = 3,
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Explode,
                                            PerTile = false,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Farmer,
                                                SpellAffectedType.Object
                                            },
                                            Value = -1
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "explosion",
                                    Radius = 5,
                                    Charges = 2,
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Explode,
                                            PerTile = false,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Farmer,
                                                SpellAffectedType.Object
                                            },
                                            Value = -1
                                        }
                                    }
                                },
                                new()
                                {
                                    CastSound = "explosion",
                                    Radius = 10,
                                    Charges = 3,
                                    Effects = new()
                                    {
                                        new()
                                        {
                                            EffectType = SpellEffectType.Explode,
                                            PerTile = false,
                                            Affected = new()
                                            {
                                                SpellAffectedType.Farmer,
                                                SpellAffectedType.Object
                                            },
                                            Value = -1
                                        }
                                    }
                                }
                            }
                        }
                    }
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
                        SpriteIndex = 0,
                        UpgradeLevel = 3,
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
            if (Helper.ModRegistry.IsLoaded("aedenthorn.Tutorials"))
            {
                CreateTutorials();
            }
            if (Config.Debug)
            {
                var qCPF = Helper.ModRegistry.GetApi<IQCPFAPI>("aedenthorn.QCPF");
                qCPF.StartPack();
                qCPF.AddEditData(spellsPath, new Dictionary<string, object>(SpellDict.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value))));
                //qCPF.WritePack(Path.Combine(SHelper.DirectoryPath, "content.json"));
            }
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