using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.IO;
using System.Linq;

namespace CustomSplashScreen
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
		public static Texture2D splashBackground;

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
			ReloadTextures();
        }

        private void ReloadTextures()
        {
			splashBackground = null;
            if (Helper.GameContent.DoesAssetExist<Texture2D>(Helper.GameContent.ParseAssetName($"{Helper.ModRegistry.ModID}/splash")))
            {
                splashBackground = Helper.GameContent.Load<Texture2D>(Helper.GameContent.ParseAssetName($"{Helper.ModRegistry.ModID}/splash"));

            }
			else if (Helper.ModContent.DoesAssetExist<Texture2D>("splash.png"))
			{
				splashBackground = Helper.ModContent.Load<Texture2D>("splash.png");
			}
			else if (Directory.Exists(Path.Combine(Helper.DirectoryPath, "Images")))
			{
				var files = Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "Images"), "*.png");
				if (files.Any())
				{
                    splashBackground = Helper.ModContent.Load<Texture2D>(files[Game1.random.Next(0, files.Length)]);
                }
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
			if(Config.ModEnabled && Game1.activeClickableMenu is TitleMenu tm && e.Button == SButton.OemCloseBrackets)
			{
				tm.logoFadeTimer = 5000;
				tm.fadeFromWhiteTimer = 4000;
            }
			ReloadTextures();
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			// Get Generic Mod Config Menu's API
			var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (gmcm is not null)
			{
				// Register mod
				gmcm.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );
                // Main section
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModEnabled,
					setValue: value => Config.ModEnabled = value
				);
			}
		}
	}

}
