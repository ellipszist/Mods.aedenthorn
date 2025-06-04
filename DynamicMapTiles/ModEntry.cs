global using xRectangle = xTile.Dimensions.Rectangle;
global using xLocation = xTile.Dimensions.Location;
global using xSize = xTile.Dimensions.Size;
global using SContext = StardewModdingAPI.Context;
global using LogLevel = StardewModdingAPI.LogLevel;
global using static DMT.ModEntry;
global using static DMT.Utils;
using DMT.Data;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using DMT.APIs;
using StardewModdingAPI.Utilities;
using StardewValley;
using Microsoft.Xna.Framework;
using Netcode;

namespace DMT
{
    internal class ModEntry : Mod
    {
        public string TileDataDictPath => $"DMT/Tiles";
        public string AnimationDataDictPath => $"DMT/Animations";

        internal static ModEntry Context { get; private set; }

        public Config Config { get; private set; }

        public Dictionary<string, List<PushedTile>> PushTileDict { get; } = [];

        public Dictionary<string, List<Animation>> AnimationsDict { get; private set; } = [];

        public Dictionary<string, DynamicTile> DynamicTiles { get; private set; } = [];

        internal PerScreen<Dictionary<string, DynamicTileProperty>> InternalProperties = new(() => []);

        internal PerScreen<List<SecondUpdateData>> SecondUpdateFiredLoops = new(() => new());
        internal PerScreen<List<SecondUpdateData>> SecondUpdateContinuousLoops = new(() => new());
        internal PerScreen<long> UpdateTicks = new(() => new());

        public override void Entry(IModHelper helper)
        {
            Context = this;

            Config = Helper.ReadConfig<Config>();

            Helper.Events.GameLoop.GameLaunched += onGameLaunched;
            Helper.Events.Player.Warped += onWarped;
            Helper.Events.Content.AssetRequested += onAssetRequested;
            Helper.Events.Content.AssetsInvalidated += onAssetInvalidated;
            Helper.Events.GameLoop.UpdateTicked += onUpdateTicked;
            Helper.Events.GameLoop.SaveLoaded += onSaveLoad;

            Helper.ConsoleCommands.Add("dmt", "DMT test commands", onConsoleCommand);
        }

        private void onSaveLoad(object? sender, SaveLoadedEventArgs e)
        {
            var passOutEvent = Helper.Reflection.GetField<NetEvent0>(Game1.player, "passOutEvent", false);
            passOutEvent.GetValue().onEvent += onFarmerPassOut;
            UpdateTicks.Value = 0;
        }

        private void onUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Config.Enabled || !SContext.IsPlayerFree)
                return;
            if (SecondUpdateFiredLoops.Value.Count == 0 && SecondUpdateContinuousLoops.Value.Count == 0)
            {
                UpdateTicks.Value = 0;
                return;
            }
            UpdateTicks.Value++;
            if (SecondUpdateFiredLoops.Value.Count > 0)
            {
                for (int i = SecondUpdateFiredLoops.Value.Count - 1; i >= 0; i--)
                {
                    var su = SecondUpdateFiredLoops.Value[i];
                    if (su.Loops <= 0)
                    {
                        SecondUpdateFiredLoops.Value.RemoveAt(i);
                        continue;
                    }
                    if (UpdateTicks.Value - su.LastTick < 60)
                        continue;
                    su.LastTick = UpdateTicks.Value;
                    --SecondUpdateFiredLoops.Value[i].Loops;
                    FireTileEvent(su);
                }
            }
            if (SecondUpdateContinuousLoops.Value.Count > 0)
            {
                for (int i = SecondUpdateContinuousLoops.Value.Count - 1; i >= 0; i--)
                {
                    var su = SecondUpdateContinuousLoops.Value[i];
                    if (su.Who.currentLocation != su.Location || su.Who.Tile != su.Tile)
                    {
                        SecondUpdateContinuousLoops.Value.RemoveAt(i);
                        continue;
                    }
                    if (UpdateTicks.Value - su.LastTick < 60)
                        continue;
                    su.LastTick = UpdateTicks.Value;
                    FireTileEvent(su);
                }
            }
        }

        private void FireTileEvent(SecondUpdateData su)
        {
            var who = su.Who;
            var value = su.Value;
            switch (su.type)
            {
                case SecondUpdateData.SecondUpdateType.Health:
                    if (value > 0)
                    {
                        who.health = (int)Math.Min(who.health + value, who.maxHealth);
                        who.currentLocation.debris.Add(new((int)value, new(who.getStandingPosition().X + 8, who.getStandingPosition().Y), Color.LimeGreen, 1f, who));
                    }
                    else
                        who.takeDamage(Math.Abs((int)value), false, null);
                    who.currentTemporaryInvincibilityDuration = 500;
                    return;
                case SecondUpdateData.SecondUpdateType.Stamina:
                    who.Stamina += value;
                    return;
                default:
                    return;
            }
        }

        private void onAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(x => x.IsEquivalentTo(AnimationDataDictPath)))
            {
                AnimationsDict = [];
                AnimationsDict = Helper.GameContent.Load<Dictionary<string, List<Animation>>>(AnimationDataDictPath);
            }

            if (e.NamesWithoutLocale.Any(x => x.IsEquivalentTo(TileDataDictPath)) && SContext.IsWorldReady)
                LoadLocation(Game1.player.currentLocation);
        }

        private void onAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(TileDataDictPath))
                e.LoadFrom(() => new Dictionary<string, DynamicTile>(), AssetLoadPriority.Exclusive);
            if (e.NameWithoutLocale.IsEquivalentTo(AnimationDataDictPath))
                e.LoadFrom(() => new Dictionary<string, List<Animation>>(), AssetLoadPriority.Exclusive);
        }

        private void onWarped(object? sender, WarpedEventArgs e)
        {
            GameLocation l = e.NewLocation;
            LoadLocation(l);
            TriggerActions([.. l.Map.Layers], e.Player, e.Player.TilePoint, ["Enter"]);
        }

        private void onGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            Patches.Patch(this);

            var configMenu = Helper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new(),
                save: () => Helper.WriteConfig(Config)
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.Enabled,
                setValue: value => Config.Enabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Trigger During Events",
                getValue: () => Config.TriggerDuringEvents,
                setValue: value => Config.TriggerDuringEvents = value
            );
        }

        private void onConsoleCommand(string cmd, string[] args)
        {
            if (args.Length == 0 || args.Length < 2 || !SContext.IsPlayerFree)
                return;
            var who = Game1.player;
            var l = who.currentLocation;
            l.setTileProperty(who.TilePoint.X, who.TilePoint.Y, "Back", args[0] + "_Once_On", args[1]);
            TriggerActions([l.Map.GetLayer("Back")], who, who.TilePoint, ["On"]);
        }

        private void onFarmerPassOut()
        {
            SecondUpdateFiredLoops.Value.Clear();
        }
    }
}
