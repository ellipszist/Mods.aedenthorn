using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace Pockets
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick))]
        public static class InventoryPage_receiveLeftClick_Patch
        {
            public static bool Prefix(InventoryPage __instance, int x, int y, bool playSound)
            {
                if (!Config.ModEnabled || !__instance.portrait.containsPoint(x, y))
                    return true;
                if (TryGetPocket(Game1.player, out var data, out IList<Item> inventory, x - __instance.portrait.bounds.Left, y - __instance.portrait.bounds.Top))
                {
                    if (openPocket == data)
                    {
                        openPocket = null;
                        __instance.inventory = new InventoryMenu(__instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth, true, null, null, -1, 3, 0, 0, true);
                        if (playSound)
                        {
                            Game1.playSound("bigDeSelect");
                        }
                    }
                    else
                    {
                        openPocket = data;
                        __instance.inventory = new InventoryMenu(__instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth, false, inventory, null, -1, data.PocketRows, 0, 0, true);
                        if (playSound)
                        {
                            Game1.playSound("bigSelect");
                        }
                    }
                    return false;
                }
                return true;
            }

        }


        [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.draw), new Type[] { typeof(SpriteBatch) })]
        public static class InventoryPage_draw_Patch
        {
            public static void Postfix(InventoryPage __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return;
                var mousePos = Game1.getMousePosition(true);
                if(!__instance.portrait.containsPoint(mousePos.X, mousePos.Y))
                    return;
                if (TryGetPocket(Game1.player, out var data, out IList<Item> inventory, mousePos.X - __instance.portrait.bounds.Left, mousePos.Y - __instance.portrait.bounds.Top))
                {
                    b.Draw(Game1.staminaRect, new Rectangle(__instance.portrait.bounds.Location + new Point(data.StartX, data.StartY), new Point(data.Width, data.Height)), Color.White * Config.Alpha);
                }
            }

        }
    }
}