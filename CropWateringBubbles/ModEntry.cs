﻿using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace CropWateringBubbles
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;

		internal static bool isEmoting;
		internal static bool emoteFading;
		internal static int currentEmoteFrame;
		internal static float emoteInterval;
		internal static int repeatInterval;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
			Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
			Helper.Events.Player.Warped += Player_Warped;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Crop), nameof(Crop.draw)),
					postfix: new HarmonyMethod(typeof(Crop_draw_Patch), nameof(Crop_draw_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Crop), nameof(Crop.drawWithOffset)),
					postfix: new HarmonyMethod(typeof(Crop_drawWithOffset_Patch), nameof(Crop_drawWithOffset_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
		{
			if (!e.Player.IsLocalPlayer)
				return;
			isEmoting = false;
		}

		private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;
			if (Config.OnlyWhenWatering && Game1.player.CurrentTool is not WateringCan)
			{
				isEmoting = false;
				emoteFading = false;
				currentEmoteFrame = 0;
				emoteInterval = 0;
				repeatInterval = 0;
				return;
			}
			if (isEmoting)
			{
				UpdateEmote();
			}
			else if (!Config.RequireKeyPress)
			{
				if (repeatInterval <= 0)
				{
					repeatInterval = Config.RepeatInterval * 60;
					isEmoting = true;
				}
				repeatInterval--;
			}
		}

		private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
		{
			if (Config.ModEnabled && Context.CanPlayerMove && !isEmoting && Config.RequireKeyPress && Config.PressKeys.JustPressed() && (!Config.OnlyWhenWatering || Game1.player.CurrentTool is WateringCan))
			{
				isEmoting = true;
				Game1.playSound("dwoop");
			}
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

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
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.RepeatInterval.Name"),
				getValue: () => Config.RepeatInterval,
				setValue: value => Config.RepeatInterval = value,
				min: 1,
				max: 100
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.OnlyWhenWatering.Name"),
				getValue: () => Config.OnlyWhenWatering,
				setValue: value => Config.OnlyWhenWatering = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.RequireKeyPress.Name"),
				getValue: () => Config.RequireKeyPress,
				setValue: value => Config.RequireKeyPress = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.IncludeGiantable.Name"),
				getValue: () => Config.IncludeGiantable,
				setValue: value => Config.IncludeGiantable = value
			);
			configMenu.AddKeybindList(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.PressKeys.Name"),
				getValue: () => Config.PressKeys,
				setValue: value => Config.PressKeys = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.OpacityPercent.Name"),
				getValue: () => Config.OpacityPercent,
				setValue: value => Config.OpacityPercent = value,
				min: 1,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.SizePercent.Name"),
				getValue: () => Config.SizePercent,
				setValue: value => Config.SizePercent = value,
				min: 1,
				max: 100
			);
		}
	}
}
