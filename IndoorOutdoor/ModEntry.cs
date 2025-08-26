using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;

namespace IndoorOutdoor
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
        public static string modKey = "aedenthorn.IndoorOutdoor";
        public static string dictPath = "aedenthorn.IndoorOutdoor/dict";
        public static PerScreen<string?> currentIndoors = new(() => null);
        public static Dictionary<string, IndoorData> IndoorDict {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, IndoorData>>(dictPath);
            }
        }
        public static PerScreen<Dictionary<string, List<Rectangle>>> currentLocationIndoorMapDict = new(() => new Dictionary<string, List<Rectangle>>());

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
            helper.Events.Player.Warped += Player_Warped;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            helper.Events.Display.RenderingWorld += Display_RenderingWorld;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
			
            Harmony harmony = new(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        public static bool renderingWorld;

        private void Display_RenderingWorld(object? sender, StardewModdingAPI.Events.RenderingWorldEventArgs e)
        {
            renderingWorld = true;
        }
        private void Display_RenderedWorld(object? sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            renderingWorld = false;
        }


        private void Player_Warped(object? sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (!Config.ModEnabled || !e.IsLocalPlayer)
                return;
            BuildIndoorMap(e.NewLocation);
        }

        private void BuildIndoorMap(GameLocation newLocation)
        {
            currentLocationIndoorMapDict.Value.Clear();
            foreach (var kvp in IndoorDict)
            {
                if(kvp.Value.Location == newLocation.Name)
                {
                    var list = new List<Rectangle>();
                    foreach(var rect in kvp.Value.Areas)
                    {
                        list.Add(new Rectangle(rect.X * 64, rect.Y * 64, rect.Width * 64, rect.Height * 64));
                    }
                    currentLocationIndoorMapDict.Value[kvp.Key] = list;
                }
            }
        }

        private void GameLoop_UpdateTicked(object? sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            renderingWorld = false;

            if (currentLocationIndoorMapDict.Value.Any())
            {
                foreach (var kvp in currentLocationIndoorMapDict.Value)
                {
                    foreach (var area in kvp.Value)
                    {
                        if (area.Contains(Game1.player.GetBoundingBox()))
                        {
                            currentIndoors.Value = kvp.Key;
                            return;
                        }
                    }
                }
            }
            currentIndoors.Value = null;
        }
        private void Input_ButtonPressed(object? sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            return;
            if(e.Button == SButton.O)
            {
                SHelper.GameContent.InvalidateCache(dictPath);
                var cc = Game1.getFarm().characters;
                for (int i = cc.Count - 1; i >= 0; i--)
                {
                    if (cc[i] is Horse horse)
                    {
                        cc.RemoveAt(i);
                    }
                }
            }
        }

        private void GameLoop_SaveLoaded(object? sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            renderingWorld = false;
        }

        private void Content_AssetRequested(object? sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, IndoorData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
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
            }
		}
	}
}
