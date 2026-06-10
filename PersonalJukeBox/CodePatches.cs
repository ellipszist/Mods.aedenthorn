using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;
using System.Text;

namespace PersonalJukeBox
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.receiveLeftClick))]
        public static class DayTimeMoneyBox_receiveLeftClick_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, int x, int y)
            {
                if (!Config.ModEnabled || !new Rectangle(__instance.xPositionOnScreen + 116 + 48 + 4, __instance.yPositionOnScreen + 68, 40, 36).Contains(x, y))
                    return true;
                ToggleMenu();
                return false;
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.isWithinBounds))]
        public static class DayTimeMoneyBox_isWithinBounds_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, int x, int y,ref bool __result)
            {
                if (!Config.ModEnabled || !new Rectangle(__instance.xPositionOnScreen + 116 + 48 + 4, __instance.yPositionOnScreen + 68, 40, 36).Contains(x, y))
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.performHoverAction))]
        public static class DayTimeMoneyBox_performHoverAction_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, int x, int y, StringBuilder ____hoverText)
            {
                if (!Config.ModEnabled || !new Rectangle(__instance.xPositionOnScreen + 116 + 48 + 4, __instance.yPositionOnScreen + 68, 40, 36).Contains(x, y))
                    return true;
                ____hoverText.Clear();
                ____hoverText.Append(SHelper.Translation.Get("jukebox"));
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.HandleMusicChange))]
        public static class GameLocation_HandleMusicChange_Patch
        {
            public static bool Prefix(GameLocation __instance)
            {
                if (!Config.ModEnabled || !Context.IsWorldReady || PlayerSong == null)
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkForMusic))]
        public static class GameLocation_checkForMusic_Patch
        {
            public static bool Prefix(GameLocation __instance)
            {
                if (!Config.ModEnabled || !Context.IsWorldReady || PlayerSong == null)
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.updateMusic))]
        public static class Game1_updateMusic_Patch
        {
            public static bool Prefix()
            {
                if (!Config.ModEnabled)
                    return true;
                return true;
            }
        }
    }
}