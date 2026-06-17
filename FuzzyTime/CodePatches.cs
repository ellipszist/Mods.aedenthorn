using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace FuzzyTime
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.draw))]
        public class DayTimeMoneyBox_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Building.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi.Name.EndsWith("drawTextWithShadow"))
                    {
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawTextWithShadow));
                        codes.Insert(i, new(OpCodes.Ldarg_0));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }

    }
}