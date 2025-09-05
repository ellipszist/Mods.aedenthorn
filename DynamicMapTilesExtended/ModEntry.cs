global using static DMT.ModEntry;
global using static DMT.Utils;
global using LogLevel = StardewModdingAPI.LogLevel;
global using SContext = StardewModdingAPI.Context;
global using xLocation = xTile.Dimensions.Location;
global using xRectangle = xTile.Dimensions.Rectangle;
global using xSize = xTile.Dimensions.Size;
using DMT.APIs;
using DMT.Data;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Diagnostics;

namespace DMT
{
    public class ModEntry : Mod
    {
        public static string ModPrefix => "DMT/";
        public string TileDataDictPath => "DMT/Tiles";
        public string AnimationDataDictPath => "DMT/Animations";

        public enum Invalidate
        {
            None,
            OnTimeChanged,
            OnLocationChanged,
            OnNewDay
        }
        public Dictionary<GameLocation, List<PushedTile>> PushTileDict { get; } = [];

        public static ModEntry context;

        public Config Config { get; private set; }

        public static int animationCounter = 0;

        public static Dictionary<GameLocation, int> InvalidateOnTimeChanged = new();
        public static List<GameLocation> InvalidateOnLocationChanged = new();
        public static List<GameLocation> InvalidateOnNewDay = new();

        public Dictionary<string, List<Animation>> animationsDict;
        public Dictionary<string, List<Animation>> AnimationsDict {
            get
            {
                if(animationsDict is null)
                {
                    animationsDict = Helper.GameContent.Load<Dictionary<string, List<Animation>>>(AnimationDataDictPath);
                }
                return animationsDict; 
            }
            private set
            {
                animationsDict = value;
            } 
        }

        public Dictionary<string, DynamicTile> DynamicTiles { get; private set; } = [];

        internal PerScreen<List<SecondUpdateData>> SecondUpdateFiredLoops = new(() => new());
        internal PerScreen<List<SecondUpdateData>> SecondUpdateContinuousLoops = new(() => new());
        internal PerScreen<long> UpdateTicks = new(() => new());

        public override void Entry(IModHelper helper)
        {
            context = this;

            Config = Helper.ReadConfig<Config>();

            Helper.Events.GameLoop.GameLaunched += onGameLaunched;
            Helper.Events.Player.Warped += onWarped;
            Helper.Events.Content.AssetRequested += onAssetRequested;
            Helper.Events.Content.AssetsInvalidated += onAssetInvalidated;
            Helper.Events.GameLoop.UpdateTicked += onUpdateTicked;
            Helper.Events.GameLoop.SaveLoaded += onSaveLoad;
            Helper.Events.GameLoop.TimeChanged += onTimeChanged;
            Helper.Events.GameLoop.DayStarted += onDayStarted;

            Helper.ConsoleCommands.Add("dmt", "DMT test commands", onConsoleCommand);
        }

        private void onDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (!Config.Enabled)
                return;
            if (InvalidateOnNewDay.Any())
            {
                foreach (var l in InvalidateOnNewDay)
                {
                    Helper.GameContent.InvalidateCache(l.mapPath.Value);
                }
                InvalidateOnNewDay.Clear();
            }
            if (InvalidateOnTimeChanged.Any()) // invalidate on new day as well, safer this way
            {
                foreach (var l in InvalidateOnTimeChanged.Keys)
                {
                    Helper.GameContent.InvalidateCache(l.mapPath.Value);
                }
                InvalidateOnTimeChanged.Clear();
            }
        }

        private void onTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if(!Config.Enabled)  
                return;
            if (Context.IsWorldReady && InvalidateOnTimeChanged.Any())
            {
                foreach(var key in InvalidateOnTimeChanged.Keys.ToArray()) 
                {
                    // amount is how much time left
                    int amount = InvalidateOnTimeChanged[key];
                    int h = amount / 100;
                    int m = amount % 100;
                    int dh = e.NewTime / 100 - e.OldTime / 100;
                    int dm = e.NewTime % 100 - e.OldTime % 100;
                    
                    int newAmount = h - dh * 100 + m - dm * 100;
                    if(newAmount <= 0)
                    {
                        Helper.GameContent.InvalidateCache(key.mapPath.Value);
                        InvalidateOnTimeChanged.Remove(key);
                    }
                }
            }
        }

        private void onSaveLoad(object? sender, SaveLoadedEventArgs e)
        {
            var passOutEvent = Helper.Reflection.GetField<NetEvent0>(Game1.player, "passOutEvent", false);
            passOutEvent.GetValue().onEvent += onFarmerPassOut;
            PushTileDict.Clear();
            UpdateTicks.Value = 0;
        }

        private void onUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Config.Enabled || !SContext.IsPlayerFree)
                return;
            if (SecondUpdateFiredLoops.Value.Count == 0 && SecondUpdateContinuousLoops.Value.Count == 0)
            {
                UpdateTicks.Value = 0;
            }
            else
            {
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
            if(PushTileDict.Count > 0)
            {
                foreach(var kvp in PushTileDict)
                {
                    for(int i = kvp.Value.Count - 1; i >= 0; i--)
                    {
                        var tile = kvp.Value[i];
                        tile.Position += GetNextTile(tile.Direction);
                        if(tile.Position == new Point(tile.Destination.X * 64, tile.Destination.Y * 64))
                        {
                            kvp.Key.Map.GetLayer("Buildings").Tiles[tile.Destination.X, tile.Destination.Y] = tile.Tile;
                            kvp.Value.RemoveAt(i);
                            TriggerActions([kvp.Key.Map.GetLayer("Back")], tile.Farmer, kvp.Key, tile.Origin, ["Pushed"]);
                        }
                    }
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
            if (InvalidateOnLocationChanged.Contains(e.OldLocation))
            {
                if (Game1.getAllFarmers().Where(f => f.currentLocation == e.OldLocation).Count() == 0)
                {
                    Helper.GameContent.InvalidateCache(e.OldLocation.mapPath.Value);
                    InvalidateOnLocationChanged.Remove(e.OldLocation);
                }
            }
            GameLocation l = e.NewLocation;
            LoadLocation(l);
            TriggerActions([.. l.Map.Layers], e.Player, e.NewLocation, e.Player.TilePoint, ["Enter"]);
        }

        private void onGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            Patches.Patch(this);

            var configMenu = Helper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
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
        }

        private void onConsoleCommand(string cmd, string[] args)
        {
            if (args.Length == 0 || args.Length < 2 || !SContext.IsPlayerFree)
                return;
            var who = Game1.player;
            var l = who.currentLocation;
            l.setTileProperty(who.TilePoint.X, who.TilePoint.Y, "Back", args[0] + "_Once_On", args[1]);
            TriggerActions([l.Map.GetLayer("Back")], who, l, who.TilePoint, ["On"]);
        }

        private void onFarmerPassOut()
        {
            SecondUpdateFiredLoops.Value.Clear();
        }
    }
}
