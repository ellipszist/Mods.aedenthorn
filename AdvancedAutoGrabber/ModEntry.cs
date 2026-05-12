using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace AdvancedAutoGrabber
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;
		internal static int seconds;
		public const string limitKey = "aedenthorn.AdvancedAutoGrabber/limit";

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if(Game1.IsMasterGame && Context.IsPlayerFree && Config.ModEnabled && Config.GrabAfterSeconds > 0 && ++seconds >= Config.GrabAfterSeconds)
            {
                seconds = 0;
                TriggerAutoGrabbers();
            }
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            if (Game1.IsMasterGame && Config.ModEnabled && Config.GrabOnTimeChange)
            {
                TriggerAutoGrabbers();
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                    name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.GrabOnDugUp.Name"),
                    getValue: () => Config.GrabOnDugUp,
                    setValue: value => Config.GrabOnDugUp = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.GrabOnTimeChange.Name"),
                    getValue: () => Config.GrabOnTimeChange,
                    setValue: value => Config.GrabOnTimeChange = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.GrabAfterSeconds.Name"),
                    getValue: () => Config.GrabAfterSeconds,
                    setValue: value => Config.GrabAfterSeconds = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.SendToChests.Name"),
                    getValue: () => Config.SendToChests,
                    setValue: value => Config.SendToChests = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.OpacityPercent.Name"),
                    getValue: () => Config.OpacityPercent,
                    setValue: value => Config.OpacityPercent = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.GrabRange.Name"),
                    getValue: () => Config.GrabRange,
                    setValue: value => Config.GrabRange = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.ChestRange.Name"),
                    getValue: () => Config.ChestRange,
                    setValue: value => Config.ChestRange = value
                );

            }

		}
	}
}
