using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ShowMissingCollectionEntries
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(CollectionsPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class CollectionsPage_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling CollectionsPage.ctor");

                bool start = false;
                bool found = false;
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!start && codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo fi && fi == AccessTools.Field(typeof(Farmer), nameof(Farmer.achievements)))
                    {
                        start = true;
                    }
                    else if (start && codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(String), nameof(String.Equals), new Type[] { typeof(string) }))
                    {
                        SMonitor.Log("Adding achievement achieved");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.AchievementAchieved))));
                        if (found)
                        {
                            break;
                        }
                        found = true;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(CollectionsPage), nameof(CollectionsPage.draw), new Type[] { typeof(SpriteBatch) })]
        public class CollectionsPage_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling CollectionsPage.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi == AccessTools.PropertyGetter(typeof(Color), nameof(Color.Black)))
                    {
                        SMonitor.Log("Adding color replace");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ModifyColor))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(CollectionsPage), nameof(CollectionsPage.performHoverAction))]
        public class CollectionsPage_performHoverAction_Patch
        {

            public static bool Prefix(CollectionsPage __instance, int x, int y, ref string ___hoverText)
            {
                if (!Config.ModEnabled || (__instance.currentTab == 5 && Config.ShowMissingAchievementDetails) || !Config.ShowMissingDetails)
                    return true;
                foreach (ClickableTextureComponent c2 in __instance.collections[__instance.currentTab][__instance.currentPage])
                {
                    if (c2.containsPoint(x, y, 2))
                    {
                        c2.scale = Math.Min(c2.scale + 0.02f, c2.baseScale + 0.1f);
                        string[] data_split = ArgUtility.SplitBySpace(c2.name);
                        if ((data_split.Length > 1 && Convert.ToBoolean(data_split[1])) || (data_split.Length > 2 && Convert.ToBoolean(data_split[2])))
                        {
                            return true;
                        }
                        else
                        {
                            if (__instance.currentTab == 7)
                            {
                                ___hoverText = Game1.parseText(c2.name.Substring(c2.name.IndexOf(' ', c2.name.IndexOf(' ') + 1) + 1), Game1.smallFont, 256);
                            }
                            else
                            {
                                ___hoverText = __instance.createDescription(data_split[0]);
                            }
                        }
                        return false;
                    }
                }
                return true;
            }
        }
    }
}