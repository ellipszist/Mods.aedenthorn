using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;
using System.Text;

namespace LauncherDrawer
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.drawMoneyBox))]
        public static class DayTimeMoneyBox_drawMoneyBox_Patch
        {
            public static void Prefix(DayTimeMoneyBox __instance, SpriteBatch b, int overrideX, ref int overrideY)
            {
                if (!Config.ModEnabled || !drawerOpen.Value)
                    return;
                var height = MenuHeight;
                var per = height / LauncherDict.Count;
                height += 32;
                overrideY = height + 180;
                int xoff = (int)__instance.position.X + 48;
                int yoff = (int)__instance.position.Y + 172;
                int width = 224;
                IClickableMenu.drawTextureBox(b, xoff, yoff, width, height, Color.White);
                foreach(var kvp in LauncherDict)
                {
                    try
                    {
                        string name = (kvp.Value.TryGetValue("DisplayName", out var str) && str is string s) ? s : kvp.Key;
                        b.DrawString(Game1.smallFont, name, new Vector2(xoff + 18, yoff + 18), Game1.textColor);
                        yoff += per;
                    }
                    catch { }
                }
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.receiveLeftClick))]
        public static class DayTimeMoneyBox_receiveLeftClick_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, int x, int y)
            {
                if (!Config.ModEnabled)
                    return true;
                if(new Rectangle(__instance.xPositionOnScreen + 116 + 48 + 4, __instance.yPositionOnScreen + 68, 40, 36).Contains(x, y))
                {
                    ToggleMenu();
                    return false;
                }
                else if(new Rectangle(__instance.xPositionOnScreen + 116 + 48 + 4, __instance.yPositionOnScreen + 68, 40, 36).Contains(x, y))
                {
                    ToggleMenu();
                    return false;
                }
                return true;
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
        
    }
}