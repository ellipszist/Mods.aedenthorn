using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MineShipping
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Display.RenderedStep += Display_RenderedStep;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Display_RenderedStep(object sender, RenderedStepEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || Game1.currentLocation is not MineShaft shaft || e.Step != StardewValley.Mods.RenderSteps.World_Sorted || Game1.getFarm()?.buildings.FirstOrDefault(b => b is ShippingBin bin && bin.daysOfConstructionLeft.Value <= 0) is not ShippingBin bin)
                return; 
            Vector2 binTile = shaft.tileBeneathLadder + new Vector2(1, -1);
            if (binTile.X < 0)
                return;

            BuildingData data = bin.GetData();
            float baseSortY = (binTile.Y + bin.tilesHigh.Value) * 64;
            float sortY = baseSortY;
            if (data != null)
            {
                sortY -= data.SortTileOffset * 64f;
            }
            sortY /= 10000f;
            Vector2 drawPosition = new Vector2((float)(binTile.X * 64), (float)(binTile.Y * 64 + bin.tilesHigh.Value * 64));
            Vector2 drawOffset = Vector2.Zero;
            if (data != null)
            {
                drawOffset = data.DrawOffset * 4f;
            }
            Rectangle mainSourceRect = bin.getSourceRect();
            Vector2 drawOrigin = new Vector2(0f, mainSourceRect.Height);
            e.SpriteBatch.Draw(bin.texture.Value, Game1.GlobalToLocal(Game1.viewport, drawPosition + drawOffset), new Rectangle?(mainSourceRect), bin.color * bin.alpha * 0.75f, 0f, drawOrigin, 4f, SpriteEffects.None, sortY);
            var lid = AccessTools.FieldRefAccess<ShippingBin, TemporaryAnimatedSprite>(bin, "shippingBinLid");
            if (lid == null)
            {
                bin.initLid();
            }
            else
            {
                Rectangle area = new((int)binTile.X * 64, (int)binTile.Y * 64 + 64, 128, 64);
                bool opening = false;
                using (FarmerCollection.Enumerator enumerator = shaft.farmers.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.GetBoundingBox().Intersects(area))
                        {
                            if (lid.pingPongMotion != 1)
                            {
                                Game1.currentLocation.localSound("doorCreak", null, null, SoundContext.Default);
                            }
                            AccessTools.Method(typeof(ShippingBin),"openShippingBinLid").Invoke(bin, Array.Empty<object>());

                            opening = true;
                        }
                    }
                }
                if (!opening)
                {
                    if (lid.pingPongMotion != -1)
                    {
                        Game1.currentLocation.localSound("doorCreakReverse", null, null, SoundContext.Default);
                    }
                    AccessTools.Method(typeof(ShippingBin), "closeShippingBinLid").Invoke(bin, Array.Empty<object>());

                }
                AccessTools.Method(typeof(ShippingBin), "updateShippingBinLid").Invoke(bin, new object[] { Game1.currentGameTime });
                var offset = (binTile) * 64 + new Vector2(4, -88) - lid.Position;
                lid.draw(e.SpriteBatch, false, (int)offset.X, (int)offset.Y, bin.alpha * 0.75f);
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
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (float)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
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