using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace CustomAchievements
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModConfig Config;
        
        public static readonly string dictPath = "custom_achievements_dictionary";

        public static Dictionary<string, CustomAcheivementData> currentAchievements = new Dictionary<string, CustomAcheivementData>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            MyPatches.Initialize(Monitor, Helper, Config);

            MyPatches.MakePatches(ModManifest.UniqueID);

            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            Helper.Events.Player.Warped += Player_Warped;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {

            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, CustomAcheivementData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }
        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            CheckForAchievements();
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            CheckForAchievements();

        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            CheckForAchievements();
        }

        public static void CheckForAchievements()
        {
            var sound = false;
            using (var dict = Game1.content.Load<Dictionary<string, CustomAcheivementData>>(dictPath).GetEnumerator())
            {
                while (dict.MoveNext())
                {
                    var a = dict.Current.Value;
                    if (currentAchievements.TryGetValue(a.ID, out var a1) && !a1.achieved && a.achieved)
                    {
                        PMonitor.Log($"Achievement {a.name} achieved!", LogLevel.Debug);
                        currentAchievements[a.ID].achieved = true;
                        if (!sound)
                        {
                            Game1.playSound("achievement");
                            sound = true;
                        }
                        Game1.addHUDMessage(new HUDMessage(a.name));
                    }
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            using (var dict = Helper.GameContent.Load<Dictionary<string, CustomAcheivementData>>(dictPath).GetEnumerator())
            {
                while (dict.MoveNext())
                {
                    var a = dict.Current.Value;
                    currentAchievements[a.ID] = a;
                }
            }
        }
    }
}