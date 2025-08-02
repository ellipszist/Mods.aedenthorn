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
                var texture = SHelper.GameContent.Load<Texture2D>(texturePath);
                int frames = (texture.Width / texture.Height);
                int intervalLength = Config.RoastFrames / frames;
                if(progress > 1)
                {
                    if (progress > intervalLength * (frames - 1))
                    {
                        Game1.player.currentLocation.playSound("dwoop");
                        Game1.createObjectDebris(Config.BurntProduct, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.currentLocation);
                    }
                    else if (progress > intervalLength * (frames - 2))
                    {
                        Game1.player.currentLocation.playSound("dwoop");
                        Game1.createObjectDebris(Config.Product, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.currentLocation);
                    }

                    Game1.player.modData.Remove(modKey);
                }
            }
        }

        private void GameLoop_SaveLoaded(object? sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
        }

        private void Content_AssetRequested(object? sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.GetData<Dictionary<string, ObjectData>>();
                    dict["aedenthorn.RoastingMarshmallows_Marshmallow"] = new()
                    {
                        Name = "Marshmallow",
                        DisplayName = SHelper.Translation.Get("Marshmallow.Name"),
                        Description = SHelper.Translation.Get("Marshmallow.Description"),
                        Price = 50,
                        Edibility = 40,
                        Type = "Cooking",
                        Category = -7,
                        Texture = SHelper.ModContent.GetInternalAssetName("assets/marshmallow.png").ToString(),
                        ContextTags = ["color_white", "food_sweet"]
                    };
                    dict["aedenthorn.RoastingMarshmallows_BurntMarshmallow"] = new()
                    {
                        Name = "BurntMarshmallow",
                        DisplayName = SHelper.Translation.Get("BurntMarshmallow.Name"),
                        Description = SHelper.Translation.Get("BurntMarshmallow.Description"),
                        Price = 10,
                        Edibility = 20,
                        Type = "Cooking",
                        Category = -7,
                        Texture = SHelper.ModContent.GetInternalAssetName("assets/burnt_marshmallow.png").ToString(),
                        ContextTags = ["color_white", "food_sweet"]
                    };
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(texturePath))
            {
                e.LoadFromModFile<Texture2D>("assets/stick.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
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
                    name: () => ModEntry.SHelper.Translation.Get("GMCM.TakeKey.Name"),
                    getValue: () => Config.RoastKey,
                    setValue: value => Config.RoastKey = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => ModEntry.SHelper.Translation.Get("GMCM.Product.Name"),
                    getValue: () => Config.Product,
                    setValue: value => Config.Product = value
                );
            }
		}
	}
}
