using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace CustomBouquets
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Object), nameof(Object.drawInMenu), new Type[] {typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) })]
        public class Object_drawInMenu_Patch
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.EnableMod || !__instance.modData.TryGetValue(flowerPath1, out var f1) || !__instance.modData.TryGetValue(flowerPath2, out var f2) || !__instance.modData.TryGetValue(flowerPath3, out var f3))
                    return true;
                __instance.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
                if (drawShadow)
                {
                    __instance.DrawShadow(spriteBatch, location, color, layerDepth);
                }
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(__instance.ItemId);
                Texture2D texture = itemData.GetTexture();
                Vector2 origin = new Vector2(8f, 8f);
                float scale = 4f * scaleSize;
                
                spriteBatch.Draw(texture, location + new Vector2(32f, 32f) * scaleSize, new Rectangle?(itemData.GetSourceRect(0, new int?(__instance.ParentSheetIndex))), Color.White * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, layerDepth);
                if (flower1 == null)
                {
                    flower1 = SHelper.GameContent.Load<Texture2D>(flowerPath1);
                    flower2 = SHelper.GameContent.Load<Texture2D>(flowerPath2);
                    flower3 = SHelper.GameContent.Load<Texture2D>(flowerPath3);
                }
                spriteBatch.Draw(flower1, location + new Vector2(32f, 32f) * scaleSize, null, GetColor(f1) * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
                spriteBatch.Draw(flower2, location + new Vector2(32f, 32f) * scaleSize, null, GetColor(f2) * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
                spriteBatch.Draw(flower3, location + new Vector2(32f, 32f) * scaleSize, null, GetColor(f3) * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));

                __instance.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth + 3E-05f, drawStackNumber, color);
                return false;
            }

        }
        public static bool showingBouquets;
        public static List<List<ColoredObject>> bouquetList = new();
        public static Dictionary<ClickableTextureComponent, List<ColoredObject>> bouquetDict = new();

        [HarmonyPatch(typeof(CraftingPage), "GetRecipesToDisplay")]
        public class CraftingPage_GetRecipesToDisplay_Patch
        {
            public static bool Prefix(CraftingPage __instance, ref List<string> __result)
            {
                if (!Config.EnableMod || !showingBouquets)
                    return true;
                SHelper.GameContent.InvalidateCache(flowerPath1);
                SHelper.GameContent.InvalidateCache(flowerPath2);
                SHelper.GameContent.InvalidateCache(flowerPath3);
                flower1 = null;
                flower2 = null;
                flower3 = null;
                bouquetList.Clear();
                __result = new List<string>();

                for(int i = 0; i < Game1.player.Items.Count; i++)
                {
                    if (Game1.player.Items[i]?.Category == -80 && Game1.player.Items[i] is Object obj && Game1.player.Items[i] is not ColoredObject && Game1.objectData.TryGetValue(Game1.player.Items[i].ItemId, out var data))
                    {
                        var cdata = Game1.cropData.Values.FirstOrDefault(d => d.HarvestItemId == obj.ItemId && d.TintColors.Any());
                        if (cdata == null)
                            continue;
                        Color? color = Utility.StringToColor(Game1.random.ChooseFrom(cdata.TintColors));
                        if (color != null)
                        {
                            var item = new ColoredObject(obj.ItemId, obj.Stack, color.Value);
                            item.Quality = obj.Quality;
                            Game1.player.Items[i] = item;
                        }
                    }
                }

                var flowers = Game1.player.Items.Where(x => x is ColoredObject obj && x.Category == Object.flowersCategory).Select(x => x as ColoredObject).ToList();

                for (int i = 0; i < flowers.Count; i++)
                {
                    if (Config.AllowMultSame && flowers[i].Stack > 2)
                    {
                        var bouquet1 = new List<ColoredObject>
                        {
                            flowers[i],
                            flowers[i],
                            flowers[i]
                        };
                        bouquetList.Add(bouquet1);
                    }
                    if (Config.AllowMultSame && flowers[i].Stack > 1)
                    {
                        var newFlowers2 = new List<ColoredObject>();
                        newFlowers2.AddRange(flowers);
                        newFlowers2.RemoveAt(i);
                        for (int j = 0; j < newFlowers2.Count; j++)
                        {
                            var bouquet2 = new List<ColoredObject>()
                            {
                                flowers[i],
                                newFlowers2[j],
                                flowers[i]
                            };
                            bouquetList.Add(bouquet2);
                            var bouquet3 = new List<ColoredObject>()
                            {
                                flowers[i],
                                flowers[i],
                                newFlowers2[j]
                            };
                            bouquetList.Add(bouquet3);
                        }
                    }
                    var newFlowers = new List<ColoredObject>();
                    newFlowers.AddRange(flowers);
                    newFlowers.RemoveAt(i);
                    for (int j = 0; j < newFlowers.Count; j++)
                    {
                        var bouquet1 = new List<ColoredObject>()
                        {
                            flowers[i],
                            newFlowers[j]
                        };
                        if (Config.AllowMultSame && newFlowers[j].Stack > 1)
                        {
                            var bouquet2 = new List<ColoredObject>
                            {
                                flowers[i],
                                newFlowers[j],
                                newFlowers[j]
                            };
                            bouquetList.Add(bouquet2);
                        }

                        var newFlowers2 = new List<ColoredObject>();
                        newFlowers2.AddRange(newFlowers);
                        newFlowers2.RemoveAt(j);
                        for (int k = 0; k < newFlowers2.Count; k++)
                        {
                            var bouquet2 = new List<ColoredObject>()
                            {
                                flowers[i],
                                newFlowers[j],
                                newFlowers2[k]
                            };
                            bouquetList.Add(bouquet2);
                        }
                    }
                }
                foreach(var bouquet in bouquetList)
                {
                    __result.Add(recipeKey);
                }
                return false;
            }

            private static void AddBouquets(List<ColoredObject> flowers, List<ColoredObject> bouquet)
            {
                for (int i = 0; i < flowers.Count; i++)
                {
                    bouquet.Add(flowers[i]);
                    if (bouquet.Count == 3)
                    {
                        var bouquet1 = new List<ColoredObject>();
                        bouquet1.AddRange(bouquet);
                        SMonitor.Log($"1 Adding {bouquet1[0].color.Value} {bouquet1[1].color.Value} {bouquet1[2].color.Value}");
                        bouquetList.Add(bouquet1);
                        bouquet.Clear();
                        continue;
                    }
                    if (bouquet.Count == 2 && flowers[i].Stack > 1)
                    {
                        var bouquet1 = new List<ColoredObject>();
                        bouquet1.AddRange(bouquet);
                        bouquet1.Add(flowers[i]);
                        SMonitor.Log($"2 Adding {bouquet1[0].color.Value} {bouquet1[1].color.Value} {bouquet1[2].color.Value}");
                        bouquetList.Add(bouquet1);
                    }
                    if (bouquet.Count == 1 && flowers[i].Stack > 2)
                    {
                        var bouquet1 = new List<ColoredObject>();
                        bouquet1.AddRange(bouquet);
                        bouquet1.Add(flowers[i]);
                        bouquet1.Add(flowers[i]);
                        SMonitor.Log($"3 Adding {bouquet1[0].color.Value} {bouquet1[1].color.Value} {bouquet1[2].color.Value}");
                        bouquetList.Add(bouquet1);
                    }
                    if (bouquet.Count == 1 && flowers[i].Stack > 1)
                    {
                        var bouquet1 = new List<ColoredObject>();
                        bouquet1.AddRange(bouquet);
                        bouquet1.Add(flowers[i]);
                        AddBouquets(flowers.Skip(1).ToList(), bouquet1);
                    }
                    var newFlowers = new List<ColoredObject>();
                    newFlowers.AddRange(flowers);
                    newFlowers.RemoveAt(i);
                    if (newFlowers.Count + bouquet.Count > 2)
                    {
                        var bouquet1 = new List<ColoredObject>();
                        bouquet1.AddRange(bouquet);
                        AddBouquets(newFlowers, bouquet1);
                    }
                    if (newFlowers.Count > 2)
                    {
                        AddBouquets(newFlowers, new List<ColoredObject>());
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CraftingPage), "layoutRecipes")]
        public class CraftingPage_layoutRecipes_Patch
        {
            public static void Postfix(CraftingPage __instance)
            {
                if (!Config.EnableMod || !showingBouquets)
                    return;
                bouquetDict.Clear();
                int count = 0;
                foreach(var page in __instance.pagesOfCraftingRecipes)
                {
                    foreach(var kvp in page)
                    {
                        kvp.Value.recipeList.Add(bouquetList[count][0].ItemId, 1);
                        if(bouquetList[count][1].ItemId == bouquetList[count][0].ItemId)
                        {
                            kvp.Value.recipeList[bouquetList[count][0].ItemId]++;
                        }
                        else
                        {
                            kvp.Value.recipeList.Add(bouquetList[count][1].ItemId, 1);
                        }
                        if(bouquetList[count][2].ItemId == bouquetList[count][0].ItemId)
                        {
                            kvp.Value.recipeList[bouquetList[count][0].ItemId]++;
                        }
                        else if(bouquetList[count][2].ItemId == bouquetList[count][1].ItemId)
                        {
                            kvp.Value.recipeList[bouquetList[count][1].ItemId]++;
                        }
                        else
                        {
                            kvp.Value.recipeList.Add(bouquetList[count][2].ItemId, 1);
                        }
                        kvp.Value.itemToProduce.Add(count + "");
                        bouquetDict[kvp.Key] = bouquetList[count++];
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CraftingPage), nameof(CraftingPage.draw), new Type[] { typeof(SpriteBatch) })]
        public class CraftingPage_draw_Patch
        {
            public static void Postfix(CraftingPage __instance, SpriteBatch b)
            {
            }
        }

        [HarmonyPatch(typeof(CraftingPage), "clickCraftingRecipe")]
        public class CraftingPage_clickCraftingRecipe_Patch
        {
            public static void Prefix(CraftingPage __instance, ClickableTextureComponent c, bool playSound, ref Dictionary<string, int> __state)
            {
                if (!Config.EnableMod || !showingBouquets || !bouquetDict.TryGetValue(c, out var list))
                    return;
                __state = __instance.pagesOfCraftingRecipes[__instance.currentCraftingPage][c].recipeList;
                __instance.pagesOfCraftingRecipes[__instance.currentCraftingPage][c].recipeList = new Dictionary<string, int>() { { "771", 1 } };
                foreach(var co in list)
                {
                    for (int j = Game1.player.Items.Count - 1; j >= 0; j--)
                    {
                        Item i = Game1.player.Items[j];
                        if (i != null && i is ColoredObject obj && obj.ItemId == co.ItemId && obj.color.Value == co.color.Value)
                        {
                            Game1.player.Items[j] = i.ConsumeStack(1);
                            break;
                        }
                    }
                }
            }
            public static void Postfix(CraftingPage __instance, ClickableTextureComponent c, bool playSound, Dictionary<string, int> __state)
            {
                if (!Config.EnableMod || !showingBouquets || __state == null)
                    return;
                __instance.pagesOfCraftingRecipes[__instance.currentCraftingPage][c].recipeList = __state;
            }
        }

        [HarmonyPatch(typeof(ClickableTextureComponent), nameof(ClickableTextureComponent.draw), new Type[] { typeof(SpriteBatch) })]
        public class ClickableTextureComponent_draw_Patch
        {
            public static void Prefix(ClickableTextureComponent __instance, SpriteBatch b)
            {
                if (!Config.EnableMod || !showingBouquets || !bouquetDict.TryGetValue(__instance, out var list))
                    return;
                if (flower1 == null)
                {
                    flower1 = SHelper.GameContent.Load<Texture2D>(flowerPath1);
                    flower2 = SHelper.GameContent.Load<Texture2D>(flowerPath2);
                    flower3 = SHelper.GameContent.Load<Texture2D>(flowerPath3);
                }
                b.Draw(flower1, __instance.bounds, null, list[0].color.Value, 0, Vector2.Zero, SpriteEffects.None, 1);
                b.Draw(flower2, __instance.bounds, null, list[1].color.Value, 0, Vector2.Zero, SpriteEffects.None, 1);
                b.Draw(flower3, __instance.bounds, null, list[2].color.Value, 0, Vector2.Zero, SpriteEffects.None, 1);
            }
        }

        [HarmonyPatch(typeof(CraftingPage),  nameof(CraftingPage.receiveLeftClick))]
        public class CraftingPage_receiveLeftClick_Patch
        {
            public static bool Prefix(CraftingPage __instance, int x, int y, bool playSound)
            {
                if (!Config.EnableMod || showingBouquets)
                    return true;
                foreach (var kvp in __instance.pagesOfCraftingRecipes[__instance.currentCraftingPage])
                {
                    if (kvp.Key.containsPoint(x, y, 4) && !kvp.Key.hoverText.Equals("ghosted") && kvp.Value.doesFarmerHaveIngredientsInInventory((IList<Item>)AccessTools.Method(typeof(CraftingPage), "getContainerContents").Invoke(__instance, null)))
                    {
                        if (kvp.Value.name == recipeKey)
                        {
                            showingBouquets = true;
                            __instance.currentCraftingPage = 0;
                            __instance.RepositionElements();
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(IClickableMenu),  nameof(IClickableMenu.exitThisMenu))]
        public class IClickableMenu_exitThisMenu_Patch
        {
            public static bool Prefix(IClickableMenu __instance)
            {
                if (!Config.EnableMod || !showingBouquets || __instance is not GameMenu menu || menu.GetCurrentPage() is not CraftingPage page)
                    return true;
                showingBouquets = false;
                page.currentCraftingPage = 0;
                page.RepositionElements();
                return false;
            }
        }

        [HarmonyPatch(typeof(Game1),  nameof(Game1.exitActiveMenu))]
        public class Game1_exitActiveMenu_Patch
        {
            public static bool Prefix()
            {
                if (!Config.EnableMod || !showingBouquets || Game1.activeClickableMenu is not GameMenu menu || menu.GetCurrentPage() is not CraftingPage page)
                    return true;
                showingBouquets = false;
                page.currentCraftingPage = 0;
                page.RepositionElements();
                return false;
            }
        }

        [HarmonyPatch(typeof(CraftingRecipe),  nameof(CraftingRecipe.createItem))]
        public class CraftingRecipe_createItem_Patch
        {
            public static void Prefix(CraftingRecipe __instance, ref List<ColoredObject> __state)
            {
                if (!Config.EnableMod || !showingBouquets || __instance.itemToProduce[0] != "458" || __instance.itemToProduce.Count != 2 || !int.TryParse(__instance.itemToProduce[1], out var idx) || idx >= bouquetList.Count || Environment.StackTrace.Contains("performHoverAction"))
                    return;
                __state = bouquetList[idx];
                __instance.itemToProduce.RemoveAt(1);
            }
            public static void Postfix(CraftingRecipe __instance, List<ColoredObject> __state, Item __result)
            {
                if (!Config.EnableMod || __state == null)
                    return;
                __result.modData[flowerPath1] = __state[0].color.Value.R + "," + __state[0].color.Value.G + "," + __state[0].color.Value.B;
                __result.modData[flowerPath2] = __state[1].color.Value.R + "," + __state[1].color.Value.G + "," + __state[1].color.Value.B;
                __result.modData[flowerPath3] = __state[2].color.Value.R + "," + __state[2].color.Value.G + "," + __state[2].color.Value.B;
            }
        }
    }
}