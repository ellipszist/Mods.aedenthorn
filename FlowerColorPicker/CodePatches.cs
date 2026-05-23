using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace FlowerColorPicker
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) })]
        public static class InventoryMenu_draw_Patch
        {
            public static void Prefix(InventoryMenu __instance)
            {
                if (!Config.ModEnabled || Game1.input.GetMouseState().ScrollWheelValue == Game1.oldMouseState.ScrollWheelValue)
                    return;
                var mousePos = Game1.getMousePosition(true);
                for(int i = 0; i < __instance.inventory.Count; i++)
                {
                    var cc = __instance.inventory[i];
                    if(cc.containsPoint(mousePos.X, mousePos.Y) && __instance.actualInventory.Count > i && __instance.actualInventory[i] is ColoredObject co && co.Category == Object.flowersCategory)
                    {
                        var data = Game1.cropData.Values.FirstOrDefault(d => d.HarvestItemId == co.ItemId);
                        if (data == null)
                            return;
                        var newColor = GetNewColor(data.TintColors, co.color.Value, Game1.input.GetMouseState().ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue);
                        if (newColor is not null)
                            co.color.Value = newColor.Value;
                        Game1.playSound("shiny4");
                        SHelper.Input.SuppressScrollWheel();
                        return;
                    }
                }

            }

        }

        [HarmonyPatch(typeof(Item), nameof(Item.canStackWith))]
        public static class Item_canStackWith_Patch
        {
            public static void Postfix(Item __instance, ISalable other, ref bool __result)
            {
                if (!Config.ModEnabled || !Config.CombineStacks || __result || __instance is not ColoredObject co || other is not ColoredObject coo || co.Category != Object.flowersCategory || co.ItemId != coo.ItemId || co.Quality != coo.Quality)
                    return;
                bool prismatic1 = co.modData.ContainsKey(prismaticKey);
                bool prismatic2 = coo.modData.ContainsKey(prismaticKey);
                if (prismatic1 || prismatic2)
                    return;
                __result = true;
            }
        }
    }
}