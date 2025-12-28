using HarmonyLib;
using StardewModdingAPI;
using System.Collections.Generic;

namespace AdvancedGiftReactions
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static string giftsKey = "aedenthorn.AdvancedGiftReactions/";

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();


            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
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
                name: () => SHelper.Translation.Get("Config.EnableMod"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.RevealAllTastes"),
                getValue: () => Config.RevealAllTastes,
                setValue: value => Config.RevealAllTastes = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.IncreaseForFirst"),
                getValue: () => Config.IncreaseForFirst,
                setValue: value => Config.IncreaseForFirst = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Config.IncreaseForBirthday"),
                getValue: () => Config.IncreaseForBirthday,
                setValue: value => Config.IncreaseForBirthday = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Config.LovedToLiked",
                getValue: () => Config.LovedToLiked,
                setValue: value => Config.LovedToLiked = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Config.LikedToNeutral",
                getValue: () => Config.LikedToNeutral,
                setValue: value => Config.LikedToNeutral = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Config.NeutralToDisliked",
                getValue: () => Config.NeutralToDisliked,
                setValue: value => Config.NeutralToDisliked = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Config.DislikedToHated",
                getValue: () => Config.DislikedToHated,
                setValue: value => Config.DislikedToHated = value
            );
        }
    }
}
