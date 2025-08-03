using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using System.Reflection;

namespace RoastingMarshmallows
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
        public static string modKey = "aedenthorn.RoastingMarshmallows/roastProgress";
        public static string texturePath = "aedenthorn.RoastingMarshmallows/texture";
        public static string rawItem = "aedenthorn.RoastingMarshmallows_RawMarshmallow";
        public static string cookedItem = "aedenthorn.RoastingMarshmallows_CookedMarshmallow";
        public static string burntItem = "aedenthorn.RoastingMarshmallows_BurntMarshmallow";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
			
            Harmony harmony = new(ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }

        private void GameLoop_UpdateTicked(object? sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || !Game1.shouldTimePass())
                return;
            GetRoastProgress(Game1.player, true, false);
        }

        private void Input_ButtonsChanged(object? sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Config.RoastKey.JustPressed())
            {
                int progress = GetRoastProgress(Game1.player, false, true);
                if(progress > 1) // remove
                {
                    var texture = SHelper.GameContent.Load<Texture2D>(texturePath);
                    int frames = (texture.Width / texture.Height);
                    int intervalLength = Config.RoastFrames / frames;
                    Game1.player.currentLocation.playSound("dwoop");
                    if (progress > intervalLength * (frames - 1))
                    {
                        Game1.createObjectDebris(burntItem, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.currentLocation);
                    }
                    else if (progress > intervalLength * (frames - 2))
                    {
                        Game1.createObjectDebris(cookedItem, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.currentLocation);
                    }
                    else
                    {
                        Game1.createObjectDebris(rawItem, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.currentLocation);
                    }
                    Game1.player.modData.Remove(modKey);
                }
            }
        }

        private void GameLoop_SaveLoaded(object? sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            var dict = SHelper.GameContent.Load<Dictionary<string, string>>(modKey + "/items");
            rawItem = dict["raw"];
            cookedItem = dict["cooked"];
            burntItem = dict["burnt"];
        }

        private void Content_AssetRequested(object? sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/CookingRecipes"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.GetData<Dictionary<string, string>>();
                    dict["Raw Marshmallow"] = $"766 1 245 1/1 10/{rawItem}/default/";
                });
            }
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.GetData<Dictionary<string, ObjectData>>();
                    dict[rawItem] = new()
                    {
                        Name = "Raw Marshmallow",
                        DisplayName = SHelper.Translation.Get("RawMarshmallow.Name"),
                        Description = SHelper.Translation.Get("RawMarshmallow.Description"),
                        Price = 20,
                        Edibility = 10,
                        Type = "Cooking",
                        Category = -7,
                        Texture = rawItem + "/texture",
                        ContextTags = ["color_white", "food_sweet"]
                    };
                    dict[cookedItem] = new()
                    {
                        Name = "Cooked Marshmallow",
                        DisplayName = SHelper.Translation.Get("CookedMarshmallow.Name"),
                        Description = SHelper.Translation.Get("CookedMarshmallow.Description"),
                        Price = 50,
                        Edibility = 50,
                        Type = "Cooking",
                        Category = -7,
                        Texture = cookedItem + "/texture",
                        ContextTags = ["color_white", "food_sweet"]
                    };
                    dict[burntItem] = new()
                    {
                        Name = "Burnt Marshmallow",
                        DisplayName = SHelper.Translation.Get("BurntMarshmallow.Name"),
                        Description = SHelper.Translation.Get("BurntMarshmallow.Description"),
                        Price = 10,
                        Edibility = 20,
                        Type = "Cooking",
                        Category = -7,
                        Texture = burntItem + "/texture",
                        ContextTags = ["color_white", "food_sweet"]
                    };
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(texturePath))
            {
                e.LoadFromModFile<Texture2D>("assets/stick.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(rawItem + "/texture"))
            {
                e.LoadFromModFile<Texture2D>("assets/raw_marshmallow.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(cookedItem + "/texture"))
            {
                e.LoadFromModFile<Texture2D>("assets/cooked_marshmallow.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(burntItem + "/texture"))
            {
                e.LoadFromModFile<Texture2D>("assets/burnt_marshmallow.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(modKey + "/items"))
            {
                e.LoadFrom(() => new Dictionary<string, string>()
                {
                    {"raw", "aedenthorn.RoastingMarshmallows_RawMarshmallow" },
                    {"cooked", "aedenthorn.RoastingMarshmallows_CookedMarshmallow" },
                    {"burnt", "aedenthorn.RoastingMarshmallows_BurntMarshmallow" }
                }, StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_GameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                    name: () => ModEntry.SHelper.Translation.Get("GMCM.ModEnabled.Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
                configMenu.AddKeybindList(
                    mod: ModManifest,
                    name: () => ModEntry.SHelper.Translation.Get("GMCM.RoastKey.Name"),
                    getValue: () => Config.RoastKey,
                    setValue: value => Config.RoastKey = value
                );
            }
		}
	}
}
