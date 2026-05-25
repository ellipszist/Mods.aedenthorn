using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace DoorFurniture
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch),typeof(int),typeof(int),typeof(float) })]
        public static class Furniture_draw_Patch
        {
            public static void Prefix(Furniture __instance, int x, int y, NetInt ___sourceIndexOffset)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(openKey, out var str))
                {
                    return;
                }
                bool flipped = false;
                if (__instance.modData.TryGetValue(flipKey, out var fs))
                {
                    _ = bool.TryParse(fs, out flipped);
                }
                if (str == "closed")
                {
                    switch (__instance.currentRotation.Value)
                    {
                        case 0:
                            ___sourceIndexOffset.Value = 0;
                            __instance.Flipped = flipped;
                            break;
                        case 1:
                            ___sourceIndexOffset.Value = 0;
                            __instance.Flipped = flipped;
                            break;
                        case 2:
                            ___sourceIndexOffset.Value = 0;
                            __instance.Flipped = flipped;
                            break;
                        case 3:
                            ___sourceIndexOffset.Value = 0;
                            __instance.Flipped = !flipped;
                            break;
                    }
                    return;
                }

                if (str == "open")
                {
                    switch (__instance.currentRotation.Value)
                    {
                        case 0:
                            ___sourceIndexOffset.Value = 1;
                            __instance.Flipped = !flipped;
                            break;
                        case 1:
                            ___sourceIndexOffset.Value = -1;
                            __instance.Flipped = !flipped;
                            break;
                        case 2:
                            ___sourceIndexOffset.Value = 5;
                            __instance.Flipped = flipped;
                            break;
                        case 3:
                            ___sourceIndexOffset.Value = -1;
                            __instance.Flipped = flipped;
                            break;
                    }
                    DoorData data = null;
                    if (__instance.modData.TryGetValue(closeKey, out var cs) || Config.AutoCloseDelay > -1 || (TryGetDoorData(__instance, out data) && data.AutoCloseDelay > -1))
                    {
                        bool can = true;
                        var box = __instance.GetBoundingBox();
                        box.Inflate(Config.PreventCloseBuffer, Config.PreventCloseBuffer);
                        foreach (var f in __instance.Location.farmers)
                        {
                            if (box.Intersects(f.GetBoundingBox()))
                            {
                                can = false;
                                break;
                            }
                        }
                        if (can)
                        {
                            if(cs == null)
                            {
                                __instance.modData[closeKey] = (data != null && data.AutoCloseDelay > -1 ? data.AutoCloseDelay : Config.AutoCloseDelay).ToString();
                            }
                            else if (cs == "0")
                            {
                                __instance.modData.Remove(closeKey);
                                str = "-1";
                            }
                            else
                            {
                                __instance.modData[closeKey] = (int.Parse(cs) - 1).ToString();
                            }
                        }
                    }
                }
                if(str != "open")
                {
                    int open = int.Parse(str);
                    switch (__instance.currentRotation.Value)
                    {
                        case 0:
                            __instance.Flipped = flipped;
                            if (open > 0)
                                ___sourceIndexOffset.Value = open < 3 ? 3 : 4;
                            else
                                ___sourceIndexOffset.Value = open > -3 ? 4 : 3;
                            break;
                        case 1:
                            __instance.Flipped = !flipped;
                            if (open > 0)
                                ___sourceIndexOffset.Value = open < 3 ? 3 : 2;
                            else
                                ___sourceIndexOffset.Value = open > -3 ? 2 : 3;
                            break;
                        case 2:
                            __instance.Flipped = flipped;
                            if (open > 0)
                                ___sourceIndexOffset.Value = open < 3 ? 3 : 4;
                            else
                                ___sourceIndexOffset.Value = open > -3 ? 4 : 3;
                            break;
                        case 3:
                            __instance.Flipped = flipped;
                            if (open > 0)
                                ___sourceIndexOffset.Value = open < 3 ? 3 : 2;
                            else
                                ___sourceIndexOffset.Value = open > -3 ? 2 : 3;
                            break;
                    }
                    if (open < 0)
                        open--;
                    else
                        open++;
                    if (open < -4)
                    {
                        __instance.modData[openKey] = "closed";
                    }
                    else if (open > 4)
                    {
                        __instance.modData[openKey] = "open";
                    }
                    else
                    {
                        __instance.modData[openKey] = open.ToString();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.checkForAction))]
        public static class Furniture_checkForAction_Patch
        {
            public static bool Prefix(Furniture __instance, bool justCheckingForActivity, ref bool __result)
            {
                if (!Config.ModEnabled || !IsDoor(__instance))
                {
                    return true;
                }
                if (!__instance.modData.TryGetValue(openKey, out var open))
                {
                    open = "closed";
                    __instance.modData[openKey] = open;
                }
                if (open != "closed" && open != "open")
                    return false;
                if (justCheckingForActivity)
                {
                    __result = true;
                    return false;
                }
                if(open == "closed")
                {
                    __instance.Location.playSound("doorOpen");
                    open = "1";
                }
                else
                {
                    __instance.Location.playSound("doorClose");
                    open = "-1";
                }
                __instance.modData[openKey] = open;
                return false;
            }

        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.IntersectsForCollision))]
        public static class Furniture_IntersectsForCollision_Patch
        {
            public static bool Prefix(Furniture __instance, Rectangle rect, ref bool __result)
            {
                if (!Config.ModEnabled || !TryGetDoorData(__instance, out var data) || data.Bounds?.Length < 4)
                    return true;
                if (!__instance.modData.TryGetValue(openKey, out var open))
                {
                    open = "closed";
                    __instance.modData[openKey] = open;
                }
                else if (open != "closed")
                {
                    return false;
                }
                var rot = __instance.currentRotation.Value;
                var loc = __instance.GetBoundingBox().Location + data.Bounds[rot].Location;
                var bounds = new Rectangle(loc, data.Bounds[rot].Size);
                __result = bounds.Intersects(rect);
                if(__result && (data.AutoOpen || Config.AutoOpen))
                {
                    __instance.modData[openKey] = "1";
                    __instance.Location.playSound("doorOpen");
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.placementAction))]
        public static class Furniture_placementAction_Patch
        {
            public static void Postfix(Furniture __instance)
            {
                if (!Config.ModEnabled || !IsDoor(__instance))
                    return;
                __instance.modData[openKey] = "closed";
            }
        }
    }
}