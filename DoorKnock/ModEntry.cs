using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using xTile.Dimensions;

namespace DoorKnock
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string answerPointKey = "aedenthorn.DoorKnock/answer";
        public const string returnPointKey = "aedenthorn.DoorKnock/return";
        public const string openKey = "aedenthorn.DoorFurniture/open";
        public static Dictionary<string, int> delayDict = new Dictionary<string, int>();
        public static Dictionary<string, int> returnDict = new Dictionary<string, int>();
        public static PerScreen<Vector2?> farmerController = new PerScreen<Vector2?>();
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            returnDict.Clear();
            delayDict.Clear();
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            foreach(var key in delayDict.Keys.ToArray())
            {
                delayDict[key]--;
                if (delayDict[key] < 0)
                {
                    delayDict.Remove(key);
                    var npc = Game1.getCharacterFromName(key, true);
                    if(npc != null)
                        DoneDelaying(npc);
                }
            }
            foreach(var key in returnDict.Keys.ToArray())
            {
                returnDict[key]--;
                if (returnDict[key] < 0)
                {
                    returnDict.Remove(key);
                    var npc = Game1.getCharacterFromName(key, true);
                    if(npc != null)
                        DoneWaiting(npc);
                }
            }
        }

        public static void updateMovementAnimation(GameTime time)
        {
            bool carrying = Game1.player.IsCarrying();
            if (!Game1.player.isRidingHorse())
            {
                Game1.player.xOffset = 0f;
            }
            FishingRod rod = Game1.player.CurrentTool as FishingRod;
            if (rod != null && (rod.isTimingCast || rod.isCasting))
            {
                rod.setTimingCastAnimation(Game1.player);
                return;
            }
            if (Game1.player.FarmerSprite.PauseForSingleAnimation || Game1.player.UsingTool)
            {
                if (Game1.player.UsingTool && Game1.player.canStrafeForToolUse() && (Game1.player.movementDirections.Count > 0 || (!Game1.player.IsLocalPlayer && Game1.player.IsRemoteMoving())) && Game1.player.yJumpOffset == 0)
                {
                    Game1.player.jumpWithoutSound(2.5f);
                }
                return;
            }
            if (Game1.player.IsSitting())
            {
                Game1.player.ShowSitting();
                return;
            }
            if (Game1.player.IsLocalPlayer && !Game1.player.CanMove && !Game1.eventUp)
            {
                if (Game1.player.isRidingHorse() && Game1.player.mount != null && !Game1.player.isAnimatingMount)
                {
                    Game1.player.showRiding();
                    return;
                }
                if (carrying)
                {
                    Game1.player.showCarrying();
                }
                return;
            }
            else
            {
                if (Game1.player.IsLocalPlayer || Game1.player.isFakeEventActor)
                {
                    AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveUp") = Game1.player.movementDirections.Contains(0);
                    AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveRight") = Game1.player.movementDirections.Contains(1);
                    AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveDown") = Game1.player.movementDirections.Contains(2);
                    AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveLeft") = Game1.player.movementDirections.Contains(3);
                    if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveLeft"))
                    {
                        Game1.player.FacingDirection = 3;
                    }
                    else if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveRight"))
                    {
                        Game1.player.FacingDirection = 1;
                    }
                    else if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveUp"))
                    {
                        Game1.player.FacingDirection = 0;
                    }
                    else if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveDown"))
                    {
                        Game1.player.FacingDirection = 2;
                    }
                    if (Game1.player.isRidingHorse() && !Game1.player.mount.dismounting.Value)
                    {
                    }
                }
                if (Game1.player.hasBuff("19"))
                {
                    Game1.player.running = false;
                    AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveUp") = false;
                    AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveDown") = false;
                    AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveLeft") = false;
                    AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveRight") = false;
                }
                if (Game1.player.FarmerSprite.PauseForSingleAnimation || Game1.player.UsingTool)
                {
                    return;
                }
                if (Game1.player.isRidingHorse() && !Game1.player.mount.dismounting.Value)
                {
                    Game1.player.showRiding();
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveLeft") && Game1.player.running && !carrying)
                {
                    Game1.player.FarmerSprite.animate(56, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveRight") && Game1.player.running && !carrying)
                {
                    Game1.player.FarmerSprite.animate(40, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveUp") && Game1.player.running && !carrying)
                {
                    Game1.player.FarmerSprite.animate(48, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveDown") && Game1.player.running && !carrying)
                {
                    Game1.player.FarmerSprite.animate(32, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveLeft") && Game1.player.running)
                {
                    Game1.player.FarmerSprite.animate(152, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveRight") && Game1.player.running)
                {
                    Game1.player.FarmerSprite.animate(136, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveUp") && Game1.player.running)
                {
                    Game1.player.FarmerSprite.animate(144, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveDown") && Game1.player.running)
                {
                    Game1.player.FarmerSprite.animate(128, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveLeft") && !carrying)
                {
                    Game1.player.FarmerSprite.animate(24, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveRight") && !carrying)
                {
                    Game1.player.FarmerSprite.animate(8, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveUp") && !carrying)
                {
                    Game1.player.FarmerSprite.animate(16, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveDown") && !carrying)
                {
                    Game1.player.FarmerSprite.animate(0, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveLeft"))
                {
                    Game1.player.FarmerSprite.animate(120, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveRight"))
                {
                    Game1.player.FarmerSprite.animate(104, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveUp"))
                {
                    Game1.player.FarmerSprite.animate(112, time);
                    return;
                }
                if (AccessTools.FieldRefAccess<Farmer, bool>(Game1.player, "moveDown"))
                {
                    Game1.player.FarmerSprite.animate(96, time);
                    return;
                }
                if (carrying)
                {
                    Game1.player.showCarrying();
                    return;
                }
                Game1.player.showNotCarrying();
                return;
            }
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree || !Config.KnockButton.JustPressed())
                return;
            if (Config.Debug)
            {
                //farmerController.Value = Game1.player.Position;
                //Game1.playSound("axchop");
            }
            var doorTile = Game1.player.GetGrabTile();
            Furniture f = Game1.currentLocation.GetFurnitureAt(doorTile);
            if (f?.modData.TryGetValue(openKey, out var open) == true && open == "closed")
            {
                PlayKnockSound(doorTile);
                return;
            }
            var action = Game1.currentLocation.GetTilePropertySplitBySpaces("Action", "Buildings", (int)doorTile.X, (int)doorTile.Y);
            if (action.Length > 1 && action[0] == "Door")
            {
                KnockInteriorDoor(doorTile, action);
                return;
            }
            var door = doorTile.ToPoint();
            if (Game1.currentLocation.doors.TryGetValue(door, out var target))
            {
                KnockExteriorDoor(doorTile, action);
            }
        }


        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
        
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("ModEnabled"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
                var props = typeof(ModConfig).GetProperties().ToArray();
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");

                foreach (var p in props)
                {
                    if (p.Name == nameof(Config.ModEnabled) || p.Name == nameof(Config.Debug))
                        continue;
                    if (p.PropertyType == typeof(bool))
                    {
                        configMenu.AddBoolOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (bool)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (int)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) { p.SetValue(Config, f); } }
                        );
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) { p.SetValue(Config, d); } }
                        );
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (string)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(KeybindList))
                    {
                        configMenu.AddKeybindList(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (KeybindList)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(SButton))
                    {
                        configMenu.AddKeybind(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (SButton)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(Color) && configMenuExt is not null)
                    {
                        configMenuExt.AddColorOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (Color)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                }
            }
        }
    }
}