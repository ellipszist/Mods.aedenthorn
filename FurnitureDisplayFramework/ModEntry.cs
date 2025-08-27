﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Object = StardewValley.Object;

namespace FurnitureDisplayFramework
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static Dictionary<string, FurnitureDisplayData> furnitureDisplayDict = new Dictionary<string, FurnitureDisplayData>();
        public static readonly string frameworkPath = "Mods/aedenthorn.FurnitureDisplayFramework/dictionary";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.draw), new Type[] {typeof(SpriteBatch)}),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(GameLocation_draw_Postfix))
            );
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo(frameworkPath))
            {
                e.LoadFrom(() => new Dictionary<string, FurnitureDisplayData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        public override object GetApi()
        {
            return new FurnitureDisplayFrameworkAPI();
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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Place Key",
                getValue: () => Config.PlaceKey,
                setValue: value => Config.PlaceKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Take Key",
                getValue: () => Config.TakeKey,
                setValue: value => Config.TakeKey = value
            );

        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            furnitureDisplayDict.Clear();
            var tempDisplayDict = Helper.GameContent.Load<Dictionary<string, FurnitureDisplayData>>(frameworkPath);
            foreach(var kvp in tempDisplayDict)
            {
                if (kvp.Key.Contains(","))
                {
                    var names = kvp.Key.Split(',');
                    foreach(var name in names)
                    {
                        if (name.Contains(":"))
                        {
                            var parts = name.Split(':');
                            var rots = parts[1].Split(',');
                            foreach(var rot in rots)
                            {
                                furnitureDisplayDict.TryAdd(parts[0]+":"+rot, kvp.Value);
                            }
                        }
                        else
                        {
                            furnitureDisplayDict.TryAdd(name, kvp.Value);
                        }
                        Monitor.Log($"Loaded furniture display template for {name}");
                    }
                }
                else
                {
                    if (kvp.Key.Contains(":"))
                    {
                        var parts = kvp.Key.Split(':');
                        var rots = parts[1].Split(',');
                        foreach (var rot in rots)
                        {
                            furnitureDisplayDict.TryAdd(parts[0] + ":" + rot, kvp.Value);
                        }
                    }
                    else
                    {
                        furnitureDisplayDict.TryAdd(kvp.Key, kvp.Value);
                    }
                    Monitor.Log($"Loaded furniture display template for {kvp.Key}");
                }
            }
            Monitor.Log($"Loaded {furnitureDisplayDict.Count} furniture display templates");
        }
        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || Game1.activeClickableMenu != null || !Context.IsWorldReady || furnitureDisplayDict.Count == 0)
                return;
            if(e.Button == Config.PlaceKey && Game1.player.ActiveObject is not null && !Game1.player.ActiveObject.bigCraftable.Value && Game1.player.ActiveObject.GetType().BaseType == typeof(Item))
            {
                var aObj = (Object)Game1.player.ActiveObject.getOne();
                var mx = Game1.viewport.X + Game1.getOldMouseX();
                var my = Game1.viewport.Y + Game1.getOldMouseY();
                foreach (var f in Game1.currentLocation.furniture)
                {
                    var name = f.rotations.Value > 1 ? f.Name + ":" + f.currentRotation.Value : f.Name;
                    if (furnitureDisplayDict.ContainsKey(name))
                    {
                        FurnitureDisplayData data = furnitureDisplayDict[name];
                        for(int i = 0; i < data.slots.Length; i++)
                        {
                            Rectangle slotRect = new Rectangle((int)(f.boundingBox.X + data.slots[i].slotRect.X * 4), (int)(f.boundingBox.Y + data.slots[i].slotRect.Y * 4), (int)(data.slots[i].slotRect.Width * 4), (int)(data.slots[i].slotRect.Height * 4));
                            //Monitor.Log($"Checking if {slotRect} contains {Game1.viewport.X + Game1.getOldMouseX()},{Game1.viewport.Y + Game1.getOldMouseY()}");
                            if (slotRect.Contains(mx, my))
                            {
                                Object obj = null;
                                var amount = Config.PlaceAllByDefault ? Game1.player.ActiveObject.Stack : 1;
                                aObj.Stack = amount;
                                //Monitor.Log($"Clicked on furniture display {name} slot {i}");
                                Monitor.Log($"Placing {aObj.Name} x{amount} in slot {i}");
                                Game1.player.currentLocation.playSound("dwop");
                                if (f.modData.TryGetValue("aedenthorn.FurnitureDisplayFramework/" + i, out string slotString))
                                {
                                    var currentItem = f.modData["aedenthorn.FurnitureDisplayFramework/" + i];
                                    obj = GetObjectFromSlot(currentItem);
                                    if (obj is not null)
                                    {
                                        Monitor.Log($"Slot has {obj.Name}x{obj.Stack}");
                                        if (AreNotSame(obj, aObj))
                                        {
                                            if (Game1.player.addItemToInventoryBool(obj, true))
                                            {
                                                if (obj.Stack <= 0)
                                                {

                                                    if (amount >= Game1.player.ActiveObject.Stack)
                                                        Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                                                    else
                                                        Game1.player.ActiveObject.Stack -= amount;
                                                    f.modData["aedenthorn.FurnitureDisplayFramework/" + i] = aObj.ToXmlString();
                                                }
                                                Monitor.Log($"Switching with {Game1.player.ActiveObject.Name} x{amount}");

                                            }
                                        }
                                        else
                                        {
                                            int slotAmount = obj.Stack;
                                            int newSlotAmount = Math.Min(aObj.maximumStackSize(), slotAmount + amount);
                                            obj.Stack = newSlotAmount;
                                            Monitor.Log($"Adding {aObj.Name} x{newSlotAmount}");
                                            f.modData["aedenthorn.FurnitureDisplayFramework/" + i] = obj.ToXmlString();
                                            Game1.player.ActiveObject.Stack -= newSlotAmount - slotAmount;
                                            if (Game1.player.ActiveObject.Stack <= 0)
                                                Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                                        }
                                        Helper.Input.Suppress(e.Button);
                                        return;
                                    }

                                }
                                Monitor.Log($"Adding {Game1.player.ActiveObject.Name} x{amount}");

                                f.modData["aedenthorn.FurnitureDisplayFramework/" + i] = aObj.ToXmlString();
                                if (amount >= Game1.player.ActiveObject.Stack)
                                    Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                                else
                                    Game1.player.ActiveObject.Stack -= amount;
                                Helper.Input.Suppress(e.Button);
                                return;
                            }
                        }
                    }
                }
            }
            if(e.Button == Config.TakeKey)
            {
                foreach (var f in Game1.currentLocation.furniture)
                {
                    var name = f.rotations.Value > 1 ? f.Name + ":" + f.currentRotation.Value : f.Name;

                    if (furnitureDisplayDict.ContainsKey(name))
                    {
                        FurnitureDisplayData data = furnitureDisplayDict[name];
                        for (int i = 0; i < data.slots.Length; i++)
                        {
                            Rectangle slotRect = new Rectangle((int)(f.boundingBox.X + data.slots[i].slotRect.X * 4), (int)(f.boundingBox.Y + data.slots[i].slotRect.Y * 4), (int)(data.slots[i].slotRect.Width * 4), (int)(data.slots[i].slotRect.Height * 4));
                            if (slotRect.Contains(Game1.viewport.X + Game1.getOldMouseX(), Game1.viewport.Y + Game1.getOldMouseY()) && furnitureDisplayDict.ContainsKey(name) && f.modData.TryGetValue("aedenthorn.FurnitureDisplayFramework/" + i, out string slotString))
                            {
                                Object obj = GetObjectFromSlot(slotString);

                                if (obj != null)
                                {
                                    Monitor.Log($"Slot has {obj.Name}x{obj.Stack}");
                                    Game1.player.currentLocation.playSound("dwop");
                                    Monitor.Log($"Taking {obj.Name}x{obj.Stack}");
                                    f.modData.Remove("aedenthorn.FurnitureDisplayFramework/" + i);
                                    Game1.player.addItemToInventoryBool(obj, true);
                                    Helper.Input.Suppress(e.Button);
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }

        private bool AreNotSame(Object obj1, Object obj2)
        {
            return obj1.Name != obj2.Name || obj1.QualifiedItemId != obj2.QualifiedItemId || obj1.Quality != obj2.Quality || obj1.preservedParentSheetIndex != obj2.preservedParentSheetIndex;
        }

        public static Object GetObjectFromSlot(string slotString)
        {
            if (string.IsNullOrEmpty(slotString))
                return null;
            return slotString.FromXmlString<Object>();
        }
    }
    

    public static class XmlTools
    {
        public static string ToXmlString<T>(this T input)
        {
            using (var writer = new StringWriter())
            {
                input.ToXml(writer);
                return writer.ToString();
            }
        }
        public static T FromXmlString<T>(this string input)
        {
            using (TextReader reader = new StringReader(input))
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(reader);
            }
        }
        public static void ToXml<T>(this T objectToSerialize, Stream stream)
        {
            new XmlSerializer(typeof(T)).Serialize(stream, objectToSerialize);
        }

        public static void ToXml<T>(this T objectToSerialize, StringWriter writer)
        {
            new XmlSerializer(typeof(T)).Serialize(writer, objectToSerialize);
        }
    }


}