using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace InventoryIndicators
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
		public static Dictionary<string, List<string>> favoriteThings = new Dictionary<string, List<string>>();

		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			var drawPrefix = new HarmonyMethod(typeof(ModEntry), nameof(drawInMenu_Prefix));
			var drawPostfix = new HarmonyMethod(typeof(ModEntry), nameof(drawInMenu_Postfix));
			var descPostfix = new HarmonyMethod(typeof(ModEntry), nameof(getDescription_Postfix));

			foreach(var t in new Type[] { typeof(Object), typeof(ColoredObject), typeof(Slingshot), typeof(Tool), typeof(MeleeWeapon), typeof(Boots), typeof(Clothing), typeof(Hat), typeof(Furniture), typeof(PetLicense), typeof(Ring), typeof(Wallpaper), typeof(RandomizedPlantFurniture), typeof(Fence), typeof(Trinket), typeof(WateringCan), typeof(Mannequin) })
			{
				try
				{
                    harmony.Patch
                    (
                        original: AccessTools.Method(t, "drawInMenu", new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
                        prefix: drawPrefix,
                        postfix: drawPostfix
                    );
                }
				catch { }
				try
				{
                    harmony.Patch
                    (
                        original: AccessTools.Method(t, "getDescription", new Type[0]),
                        postfix: descPostfix
                    );
                }
				catch 
				{
				}
            }

        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
			favoriteThings.Clear();
            foreach (var npc in Game1.NPCGiftTastes)
            {
                try
                {
                    var favs = ArgUtility.SplitBySpace(npc.Value.Split('/', StringSplitOptions.None)[1]);
                    foreach (var fav in favs)
                    {
						if(!favoriteThings.TryGetValue(fav, out var list))
						{
							list = new List<string>();
							favoriteThings[fav] = list;
							list.Add(npc.Key);
						}
                    }
                }
                catch
                {

                }
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
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ShowGiftedFavorites.Name"),
					getValue: () => Config.ShowGiftedFavorites,
					setValue: value => Config.ShowGiftedFavorites = value
				);
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ShowGiftedFavorites.Name"),
					getValue: () => Config.ShowUngiftedFavorites,
					setValue: value => Config.ShowUngiftedFavorites = value
				);
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ShowBundleItems.Name"),
					getValue: () => Config.ShowBundleItems,
					setValue: value => Config.ShowBundleItems = value
				);
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ShowPlantableSeeds.Name"),
					getValue: () => Config.ShowPlantableSeeds,
					setValue: value => Config.ShowPlantableSeeds = value
				);
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ShowUngiftedFavorites.Name"),
					getValue: () => Config.ShowUngiftedFavorites,
					setValue: value => Config.ShowUngiftedFavorites = value
				);
                gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.OutlineWidth.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.OutlineWidth.Desc"),
					getValue: () => Config.OutlineWidth,
					setValue: value => Config.OutlineWidth = value
				);
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");
                if (configMenuExt is not null)
				{
                    configMenuExt.AddColorOption(
						mod: ModManifest,
						getValue: () => Config.JunimoColor,
						setValue: (c) => Config.JunimoColor = c,
						name: () => SHelper.Translation.Get("GMCM.JunimoColor.Name")
					);
                    bool changed = false;
                    foreach (var f in GetFieldInfos())
                    {
                        var name = f.Name;

                        if (!Config.Colors.TryGetValue(name, out var color))
                        {
                            color = Object.GetCategoryColor(GetCategory(f.Name));
                            if (color != Color.Black)
                            {
                                Config.Colors[name] = color;

                            }
                            else
                            {
                                color = new Color(0, 0, 0, 0);
                                Config.Colors[name] = color;
                            }
                            changed = true;
                        }
                        var dname = name.Replace("Category", " Category");
                        configMenuExt.AddColorOption(
                            mod: ModManifest,
                            getValue: () => color,
                            setValue: (c) => Config.Colors[name] = c,
                            name: () => dname,
                            tooltip: () => string.Format(SHelper.Translation.Get("x_color"), dname));
                    }
                    if (changed)
                        SHelper.WriteConfig(Config);
                }
			}
		}
	}
}
