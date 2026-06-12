using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

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
                    Color color = Color.White;
                    if (data.Burned)
                    {
                        color = Color.DarkSlateGray;
                        idata = ItemRegistry.GetDataOrErrorItem(data.BurntID);
                    }
                    else if(data.Progress >= 1)
                    {
                        color = data.ProductID.StartsWith(grilledPrefix) ? Config.GrilledColor : Color.White;
                        idata = ItemRegistry.GetDataOrErrorItem(data.ProductID);
                    }
                    else
                    {
                        idata = ItemRegistry.GetDataOrErrorItem(data.InputID);
                    }
                    float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                    spriteBatch.Draw(idata.GetTexture(), Game1.GlobalToLocal(__instance.TileLocation * 64) + new Vector2(cdata.CookOffset.X, cdata.CookOffset.Y), idata.GetSourceRect(), color, 0, Vector2.Zero, 3f, SpriteEffects.None, draw_layer + cdata.CookOffset.Z);

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
                    color = Config.GrilledColor;
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
                        if(kvp.Value is Torch t && !t.IsOn)
                        {
                            continue;
                        }
                        data.Update(kvp.Value, timeOfDay);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        public class Game1_pressUseToolButton_Patch
        {
            public static bool Prefix()
            {
                if (!Config.ModEnabled)
                    return true;
                Farmer f = Game1.player;
                Vector2 c = f.GetToolLocation(false) / 64f;
                c.X = (float)((int)c.X);
                c.Y = (float)((int)c.Y);
                GameLocation l = Game1.currentLocation;
                if (!l.Objects.TryGetValue(c, out var obj) || !CookerDict.TryGetValue(obj.QualifiedItemId, out var cooker))
                {
                    return true;
                }
                var cookable = IsCookable(f.ActiveObject, out var data);
                if (obj.modData.TryGetValue(cookingKey, out var str))
                {
                    var cdata = GetCookingData(str);
                    var progress = cdata.Progress;
                    if (progress < 1)
                    {
                        Game1.showRedMessage(SHelper.Translation.Get("cooker-busy"));
                        return false;
                    }
                    l.playSound("coin", obj.TileLocation);
                    TryReturnObject(cdata.Burned ? ItemRegistry.Create(cdata.BurntID, 1) : ItemRegistry.Create(cdata.ProductID, 1, cdata.Quality), f);
                    obj.modData.Remove(cookingKey);
                    if (!cookable)
                        return false;
                }
                if (cookable)
                {
                    l.playSound("cut", obj.TileLocation);
                    f.ignoreItemConsumptionThisFrame = false;
                    obj.modData[cookingKey] = JsonConvert.SerializeObject(data);
                    f.reduceActiveItemByOne();
                    return false;
                }
                return true;
            }


        }
    }
}