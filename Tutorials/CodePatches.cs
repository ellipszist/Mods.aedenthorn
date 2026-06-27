using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;

namespace Tutorials
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) })]
        public static class InventoryMenu_draw_Patch
        {
            public static void Prefix(InventoryMenu __instance)
            {
                if (!Config.ModEnabled || !Config.OpenTutorialKey.JustPressed())
                {
                    return;
                }
                var mousePos = Game1.getMousePosition(true);
                for (int i = 0; i < __instance.inventory.Count; i++)
                {
                    var cc = __instance.inventory[i];
                    if (cc.containsPoint(mousePos.X, mousePos.Y))
                    {
                        if (__instance.actualInventory.Count > i && __instance.actualInventory[i] is Item obj)
                        {
                            var value = TutorialTriggerDict.FirstOrDefault(p => p.Key == obj.QualifiedItemId).Value;
                            if(value is not null)
                            {
                                if(value.Tutorial is not null)
                                    OpenTutorial(value.Tutorial, null, value.Categories);
                                else if(value.Category is not null)
                                    OpenTutorial(null, value.Category, value.Categories);
                            }
                        }
                        return;
                    }
                }

            }
        }
    }
}