using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomMonsters
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;

		public static Dictionary<string, MonsterData> Monsters
		{
			get
			{
				return SHelper.GameContent.Load<Dictionary<string, MonsterData>>(dictPath);
			}
		}

        public const string dictPath = "aedenthorn.CustomMonsters/dict";
        public const string monsterKey = "aedenthorn.CustomMonsters/monster";

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
			foreach (var t in typeof(Monster).Assembly.GetTypes().Where(t => typeof(Monster).IsAssignableFrom(t)))
			{
				var mi = t.GetMethod("getExtraDropItems");
                if (mi != null && mi.DeclaringType == t)
				{
					harmony.Patch(mi, postfix: new HarmonyMethod(typeof(ModEntry), nameof(GetExtraDropItemsPostfix)));
                }
				mi = t.GetMethod("reloadSprite");
                if (mi != null && mi.DeclaringType == t)
				{
					harmony.Patch(mi, prefix: new HarmonyMethod(typeof(ModEntry), nameof(ReloadSpritePrefix)));
                }
				mi = t.GetMethod("takeDamage", new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) });
                if (mi != null && mi.DeclaringType == t)
				{
					harmony.Patch(mi, transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ChangeDamageSoundTranspiler)));
                }
				mi = t.GetMethod("localDeathAnimation");
                if (mi != null && mi.DeclaringType == t)
				{
					harmony.Patch(mi, transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ChangeDeathSoundTranspiler)));
                }
				mi = t.GetMethod("sharedDeathAnimation");
                if (mi != null && mi.DeclaringType == t)
				{
					harmony.Patch(mi, transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ChangeDeathSoundTranspiler)));
                }
				mi = t.GetMethod("behaviorAtGameTick");
                if (mi != null && mi.DeclaringType == t)
				{
					harmony.Patch(mi, transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ChangeMoveSoundTranspiler)));
                }
				mi = t.GetMethod("updateAnimation");
                if (mi != null && mi.DeclaringType == t)
				{
					harmony.Patch(mi, transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ChangeMoveSoundTranspiler)));
                }
				mi = t.GetMethod("onDealContactDamage");
                if (mi != null && mi.DeclaringType == t)
				{
					harmony.Patch(mi, transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ChangeContactSoundTranspiler)));
                }
            }
        }


        public override object GetApi()
        {
            return new CustomMonstersAPI();
        }


        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
		{
            if(!Config.ModEnabled) 
                return;
			if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
			{
                e.LoadFrom(() => new Dictionary<string, MonsterData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
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
