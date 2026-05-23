using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Linq;
using Object = StardewValley.Object;

namespace FlowerColors
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) })]
        public static class InventoryMenu_draw_Patch
        {
            public static void Prefix(InventoryMenu __instance)
            {
                if (!Config.ModEnabled || (lastScrollDelta.Value == 0 && !Config.CopyButton.JustPressed() && !Config.PasteButton.JustPressed() && !Config.PickButton.JustPressed()))
                {
                    return;
                }
                var mousePos = Game1.getMousePosition(true);
                for(int i = 0; i < __instance.inventory.Count; i++)
                {
                    var cc = __instance.inventory[i];
                    if(cc.containsPoint(mousePos.X, mousePos.Y))
                    {
                        if(__instance.actualInventory.Count > i && __instance.actualInventory[i] is ColoredObject co && co.Category == Object.flowersCategory)
                        {
                            var data = Game1.cropData.Values.FirstOrDefault(d => d.HarvestItemId == co.ItemId);
                            if (data == null)
                                return;
                            if (lastScrollDelta.Value != 0)
                            {
                                if (SHelper.Input.IsDown(Config.PrismaticModKey))
                                {
                                    if (co.modData.TryGetValue(prismaticKey, out var prismatic))
                                    {
                                        co.modData[prismaticKeyDisabled] = prismatic;
                                        co.modData.Remove(prismaticKey);
                                        Game1.playSound("yoba");
                                        SHelper.Input.SuppressScrollWheel();
                                    }
                                    else if (co.modData.TryGetValue(prismaticKeyDisabled, out var prismatic2))
                                    {
                                        co.modData[prismaticKey] = prismatic2;
                                        co.modData.Remove(prismaticKeyDisabled);
                                        Game1.playSound("yoba");
                                        SHelper.Input.SuppressScrollWheel();
                                    }
                                    else if (prismaticFlowersAPI?.MakePrismatic(co) == true)
                                    {
                                        Game1.playSound("yoba");
                                        SHelper.Input.SuppressScrollWheel();
                                    }
                                }
                                else
                                {
                                    var newColor = GetNewColor(data.TintColors, co.color.Value, lastScrollDelta.Value);
                                    if (newColor is not null)
                                        co.color.Value = newColor.Value;
                                    Game1.playSound("shiny4");
                                    SHelper.Input.SuppressScrollWheel();
                                }
                                lastScrollDelta.Value = 0;
                            }
                            else if (Config.CopyButton.JustPressed())
                            {
                                if (co.modData.TryGetValue(prismaticKey, out var str))
                                {
                                    pastePrismatic.Value = str;
                                    co.modData[prismaticKey] = str;
                                }
                                else
                                {
                                    pastePrismatic.Value = null;
                                }
                                pasteColor.Value = co.color.Value;
                                Game1.hudMessages.Clear();
                                Game1.player.ShowItemReceivedHudMessage(co, 1);
                                foreach (var k in Config.CopyButton.Keybinds)
                                {
                                    foreach (var b in k.Buttons)
                                    {
                                        if (!new SButton[] { SButton.LeftControl, SButton.LeftShift, SButton.LeftAlt, SButton.RightShift, SButton.RightControl, SButton.RightAlt, }.Contains(b))
                                            SHelper.Input.Suppress(b);
                                    }
                                }
                            }
                            else if (Config.PasteButton.JustPressed())
                            {
                                if (pasteColor.Value == Color.White)
                                    return;
                                if (pastePrismatic.Value != null)
                                {
                                    co.modData[prismaticKey] = pastePrismatic.Value;
                                }
                                else
                                {
                                    co.modData.Remove(prismaticKey);
                                    co.color.Value = pasteColor.Value;
                                }
                                Game1.playSound("shiny4");
                                foreach (var k in Config.PasteButton.Keybinds)
                                {
                                    foreach (var b in k.Buttons)
                                    {
                                        if (!new SButton[] { SButton.LeftControl, SButton.LeftShift, SButton.LeftAlt, SButton.RightShift, SButton.RightControl, SButton.RightAlt, }.Contains(b))
                                            SHelper.Input.Suppress(b);
                                    }
                                }
                            }
                            else if (Config.PickButton.JustPressed())
                            {
                                Game1.activeClickableMenu = new ColorPickMenu(co, Game1.activeClickableMenu);
                                foreach (var k in Config.PickButton.Keybinds)
                                {
                                    foreach (var b in k.Buttons)
                                    {
                                        SHelper.Input.Suppress(b);
                                    }
                                }
                            }
                        }
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