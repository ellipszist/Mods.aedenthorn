using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SimpleCooking
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Object_draw_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled)
                {
                    return;
                }
                if (TryGetCookingData(__instance, out var data) && CookerDict.TryGetValue(__instance.QualifiedItemId, out var cdata))
                {
                    ParsedItemData idata;
                    var progress = CookProgress(data, Game1.timeOfDay);
                    Color color = Color.White;
                    if (progress < 1)
                    {
                        idata = ItemRegistry.GetDataOrErrorItem(data.InputID);
                    }
                    else if(progress < data.Burned)
                    {
                        color = data.ProductID.StartsWith(grilledPrefix) ? Config.GrilledColor : Color.White;
                        idata = ItemRegistry.GetDataOrErrorItem(data.ProductID);
                    }
                    else
                    {
                        color = Color.DarkSlateGray;
                        idata = ItemRegistry.GetDataOrErrorItem(data.BurntID);
                    }
                    float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                    var offset = new Vector2(8, 4);
                    spriteBatch.Draw(idata.GetTexture(), Game1.GlobalToLocal(__instance.TileLocation * 64) + offset, idata.GetSourceRect(), color, 0, Vector2.Zero, 3f, SpriteEffects.None, draw_layer + 1/100000f);

                    //TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), __instance.TileLocation * 64, false, 0.002f, Color.Gray);
                    //sprite.alpha = 0.5f * progress;
                    //sprite.motion = new Vector2(0f, -0.5f);
                    //sprite.acceleration = new Vector2(0.002f, 0f);
                    //sprite.interval = 99999f;
                    //sprite.layerDepth = 1f;
                    //sprite.scale = 1;
                    //sprite.scaleChange = 0.02f;
                    //sprite.rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f;
                    //__instance.Location.temporarySprites.Add(sprite);
                }
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) })]
        public class Object_drawInMenu_Patch
        {
            public static void Prefix(Object __instance, ref Color color)
            {
                if (!Config.ModEnabled)
                {
                    return;
                }
                if (__instance.ItemId.StartsWith(grilledPrefix))
                {
                    color = Color.DarkGoldenrod;
                }
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawWhenHeld))]
        public class Object_drawWhenHeld_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Object.drawWhenHeld");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi == AccessTools.PropertyGetter(typeof(Color), nameof(Color.White)))
                    {
                        SMonitor.Log($"Adding color switch");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchColor))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performTenMinuteUpdate))]
        public class GameLocation_performTenMinuteUpdate_Patch
        {
            public static void Postfix(GameLocation __instance, int timeOfDay)
            {
                if (!Config.ModEnabled)
                    return;
                foreach(var kvp in __instance.Objects.Pairs)
                {
                    if(TryGetCookingData(kvp.Value, out var data))
                    {
                        if(CookProgress(data, timeOfDay) >= 2 && CookProgress(data, MinutesToTime(TimeToMinutes(timeOfDay) - 10)) < 2)
                        {
                            __instance.playSound(data.BurntSound, kvp.Key);
                        }
                        else if(CookProgress(data, timeOfDay) >= 1 && CookProgress(data, MinutesToTime(TimeToMinutes(timeOfDay) - 10)) < 1)
                        {
                            __instance.playSound(data.CookedSound, kvp.Key);
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                if (!__instance.Objects.TryGetValue(new Vector2(tileLocation.X, tileLocation.Y), out var obj) || !CookerDict.TryGetValue(obj.QualifiedItemId, out var cooker))
                {
                    return true;
                }
                var cookable = IsCookable(who.ActiveObject, out var data);
                if (obj.modData.TryGetValue(cookingKey, out var str))
                {
                    var cdata = GetCookingData(str);
                    var progress = CookProgress(cdata, Game1.timeOfDay);
                    if (progress == 1)
                    {
                        __instance.playSound("dwoop", obj.TileLocation);
                        TryReturnObject(ItemRegistry.Create(cdata.ProductID, 1, cdata.Quality), who);
                        obj.modData.Remove(cookingKey);
                    }
                    else if (progress == 2)
                    {
                        __instance.playSound("slimehit", obj.TileLocation);
                        TryReturnObject(ItemRegistry.Create(cdata.BurntID, 1, cdata.Quality), who);
                        obj.modData.Remove(cookingKey);
                    }
                    else
                    {
                        who.ignoreItemConsumptionThisFrame = false;
                        Game1.showRedMessage(SHelper.Translation.Get("cooker-busy"));
                        __result = true;
                        return false;
                    }
                    if (!cookable)
                    {
                        __result = true;
                        return false;
                    }
                }
                if (cookable)
                {
                    __instance.playSound("grassyStep", obj.TileLocation);
                    who.ignoreItemConsumptionThisFrame = false;
                    obj.modData[cookingKey] = JsonConvert.SerializeObject(data);
                    who.reduceActiveItemByOne();
                    __result = true;
                    return false;
                }
                return true;
            }


        }
    }
}