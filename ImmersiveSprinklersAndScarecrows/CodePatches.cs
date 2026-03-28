using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using xTile.Dimensions;
using Color = Microsoft.Xna.Framework.Color;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ImmersiveSprinklersAndScarecrows
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.EnableMod)
                    return true;
                Vector2 placementTile = new Vector2(tileLocation.X, tileLocation.Y);
                var mouseTile = GetMouseCornerTile();
                if (Vector2.Distance(placementTile, mouseTile.ToVector2()) >= 3)
                    return true; 
                var tile = GetMouseCornerTile();
                var x = tile.X;
                var y = tile.Y;
                var obj = GetSprinklerAtMouse();
                if (obj is not null && who.CurrentItem is not null)
                {

                    if (who.CurrentItem.ItemId == "913" && !EnricherAt(__instance, x, y))
                    {
                        __instance.modData[$"{enricherKey},{x},{y}"] = "true";
                        who.reduceActiveItemByOne();
                        __instance.playSound("axe");
                    }
                    else if (who.CurrentItem.ItemId == "915" && !NozzleAt(__instance, x, y))
                    {
                        __instance.modData[$"{nozzleKey},{x},{y}"] = "true";
                        who.reduceActiveItemByOne();
                        __instance.playSound("axe");
                    }
                    else if (who.CurrentItem.Category == -19 && EnricherAt(__instance, x, y))
                    {
                        int stack = who.CurrentItem.Stack;
                        int addStack = stack;
                        int index = who.CurrentItem.ParentSheetIndex;
                        var fertString = FertilizerAt(__instance, x, y);
                        if (fertString != null)
                        {
                            Object f = GetFertilizer(fertString);
                            if (f.ParentSheetIndex == who.CurrentItem.ParentSheetIndex)
                            {
                                int add = Math.Min(f.maximumStackSize() - f.Stack, stack);
                                addStack = f.Stack + add;
                                stack -= add;
                                who.CurrentItem.Stack = stack;
                                if (stack == 0)
                                {
                                    who.removeItemFromInventory(who.CurrentItem);
                                    who.showNotCarrying();
                                }
                            }
                            else
                            {
                                var slot = who.CurrentToolIndex;
                                who.removeItemFromInventory(who.CurrentItem);
                                who.showNotCarrying();
                                who.Items[slot] = f;
                            }
                        }
                        else
                        {
                            who.removeItemFromInventory(who.CurrentItem);
                            who.showNotCarrying();
                        }
                        __instance.modData[$"{fertilizerKey},{x},{y}"] = index + "," + addStack;

                        __instance.playSound("dirtyHit");
                    }
                    __result = true;

                    return false;
                }
                obj = GetScarecrowAtMouse();
                if (obj != null)
                {
                    if (obj.ParentSheetIndex == 126 && who.CurrentItem is not null && who.CurrentItem is Hat hat)
                    {
                        var hatString = HatAt(__instance, x, y);
                        if (hatString != null)
                        {
                            Game1.createItemDebris(new Hat(hatString), Game1.currentCursorTile * 64f, (who.FacingDirection + 2) % 4, null, -1);
                            __instance.modData.Remove($"{hatKey},{x},{y}");

                        }
                        __instance.modData[$"{hatKey},{x},{y}"] = hat.ItemId;
                        who.Items[who.CurrentToolIndex] = null;
                        who.currentLocation.playSound("dirtyHit");
                        __result = true;
                        return false;
                    }
                    if (Game1.didPlayerJustRightClick(true))
                    {
                        var scaredString = ScaredAt(__instance, x, y);
                        int scared = 0;
                        int.TryParse(scaredString, out scared);
                        if (scaredString == null)
                        {
                            __instance.modData[$"{scaredKey},{x},{y}"] = "0";
                        }
                        if (scared == 0)
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12926"));
                        }
                        else
                        {
                            Game1.drawObjectDialogue((scared == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12927") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12929", scared));
                        }
                        __result = true;
                        return false;
                    }

                }
                    
                return true;


            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.EnableMod)
                    return true;
                Vector2 placementTile = new Vector2(x / 64, y / 64);

                var tile = GetMouseCornerTile();
                int tileX = (int)tile.X;
                int tileY = (int)tile.Y;
                if (__instance.IsSprinkler() && location.terrainFeatures.ContainsKey(placementTile))
                {
                    SMonitor.Log($"Placing {__instance.Name} at {tileX},{tileY}");
                    location.playSound("woodyStep");
                    ReturnOrDropSprinkler(location, tileX, tileY, who, false);
                    if (__instance.bigCraftable.Value)
                    {
                        location.modData[$"{sprinklerBigCraftableKey},{tileX},{tileY}"] = "true";
                        location.modData[$"{sprinklerKey},{tileX},{tileY}"] = (string)AccessTools.Method(typeof(Item), "ValidateUnqualifiedItemId").Invoke(__instance, new object[] { __instance.ItemId });
                    }
                    else
                    {
                        location.modData[$"{sprinklerKey},{tileX},{tileY}"] = __instance.ItemId;
                    }
                    var guid = Guid.NewGuid().ToString();
                    location.modData[$"{sprinklerGuidKey},{tileX},{tileY}"] = guid;
                    sprinklerDict[guid] = __instance;
                    if (atApi is not null)
                    {
                        Object obj = (Object)__instance.getOne();
                        SetAltTextureForObject(obj);
                        foreach (var kvp in obj.modData.Pairs)
                        {
                            if (kvp.Key.StartsWith(altTextureKey))
                            {
                                location.modData[$"{sprinklerPrefix}{kvp.Key},{tileX},{tileY}"] = kvp.Value;
                            }
                        }
                    }
                    __result = true;
                    return false;
                }
                else if (__instance.IsScarecrow() && location.terrainFeatures.ContainsKey(placementTile))
                {
                    SMonitor.Log($"Placing {__instance.Name} at {tileX},{tileY}");
                    location.playSound("woodyStep");
                    ReturnOrDropScarecrow(location, tileX, tileY, who, false);
                    if (__instance.bigCraftable.Value)
                    {
                        location.modData[$"{scarecrowBigCraftableKey},{tileX},{tileY}"] = "true";
                        location.modData[$"{scarecrowKey},{tileX},{tileY}"] = (string)AccessTools.Method(typeof(Item), "ValidateUnqualifiedItemId").Invoke(__instance, new object[] { __instance.ItemId });
                    }
                    else
                    {
                        location.modData[$"{scarecrowKey},{tileX},{tileY}"] = __instance.ItemId;
                    }
                    var guid = Guid.NewGuid().ToString();
                    location.modData[$"{scarecrowGuidKey},{tileX},{tileY}"] = guid;
                    scarecrowDict[guid] = __instance;
                    if (atApi is not null)
                    {
                        Object obj = (Object)__instance.getOne();
                        SetAltTextureForObject(obj);
                        foreach (var kvp in obj.modData.Pairs)
                        {
                            if (kvp.Key.StartsWith(altTextureKey))
                            {
                                location.modData[$"{scarecrowPrefix}{kvp.Key},{tileX},{tileY}"] = kvp.Value;
                            }
                        }
                    }
                    __result = true;
                    return false;

                }
                else if (__instance.Category == -74 && location.terrainFeatures.TryGetValue(placementTile, out var tf) && tf is HoeDirt dirt)
                {
                    foreach (var point in GetSprinklerPoints(location))
                    {
                        int sprX = point.X;
                        int sprY = point.Y;
                        if (EnricherAt(location, sprX, sprY))
                        {
                            var fertString = FertilizerAt(location, sprX, sprY);
                            if (fertString != null)
                            {
                                var obj = GetSprinkler(location, sprX, sprY);
                                if (obj is not null)
                                {
                                    var radius = obj.GetModifiedRadiusForSprinkler();
                                    if (GetSprinklerTiles(tile.ToVector2(), radius).Contains(placementTile))
                                    {
                                        Object f = GetFertilizer(fertString);
                                        if (dirt.plant(Crop.ResolveSeedId(f.ItemId, location), who, true))
                                        {
                                            f.Stack--;
                                            if (f.Stack > 0)
                                            {
                                                location.modData[$"{fertilizerKey},{sprX},{sprY}"] = f.ParentSheetIndex + "," + f.Stack;
                                            }
                                            else
                                            {
                                                location.modData.Remove($"{fertilizerKey},{sprX},{sprY}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.draw))]
        public class HoeDirt_DrawOptimized_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.EnableMod)
                    return;
                foreach (var p in GetSprinklerPoints(__instance))
                {
                    var obj = GetSprinklerCached(__instance, p.X, p.Y);

                    if (obj is not null)
                    {
                        Vector2 globalPosition = p.ToVector2() * 64 + new Vector2(32, 16);
                        if (obj.bigCraftable.Value)
                            globalPosition -= new Vector2(0, 64);
                        var layerDepth = (globalPosition.Y + (obj.bigCraftable.Value ? 81 : 33) + Config.DrawOffsetZ) / 10000f;
                        var position = Game1.GlobalToLocal(globalPosition);

                        Texture2D texture = null;
                        ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId);
                        Rectangle sourceRect = itemData.GetSourceRect(obj is Mannequin ? 2 : 0, new int?(obj.ParentSheetIndex));
                        texture = itemData.GetTexture();
                        if (atApi is not null && obj.modData.ContainsKey("AlternativeTextureName"))
                        {
                            texture = GetAltTextureForObject(obj, out sourceRect);
                        }
                        if (texture is null)
                        {
                            texture = obj.bigCraftable.Value ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet;
                            sourceRect = obj.bigCraftable.Value ? Object.getSourceRectForBigCraftable(obj.ParentSheetIndex) : GameLocation.getSourceRectForObject(obj.ParentSheetIndex);
                        }
                        b.Draw(texture, position, sourceRect, Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);

                        if (EnricherAt(__instance, p.X, p.Y))
                        {
                            b.Draw(Game1.objectSpriteSheet, position + new Vector2(0f, -20f), GameLocation.getSourceRectForObject(914), Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 2E-05f);
                        }
                        if (NozzleAt(__instance, p.X, p.Y))
                        {
                            b.Draw(Game1.objectSpriteSheet, position, GameLocation.getSourceRectForObject(916), Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, obj.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + 1E-05f);
                        }
                    }
                }
                foreach (var p in GetScarecrowPoints(__instance))
                {
                    var obj = GetScarecrowCached(__instance, p.X, p.Y);

                    if (obj is not null)
                    {
                        Vector2 scaleFactor = obj.getScale();


                        ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId);
                        Rectangle sourceRect = itemData.GetSourceRect(obj is Mannequin ? 2 : 0, new int?(obj.ParentSheetIndex));
                        Texture2D texture = itemData.GetTexture();
                        if (atApi is not null && obj.modData.ContainsKey("AlternativeTextureName"))
                        {
                            texture = GetAltTextureForObject(obj, out sourceRect);
                        }
                        if (texture is null)
                        {
                            texture = Game1.bigCraftableSpriteSheet;
                            sourceRect = Object.getSourceRectForBigCraftable(obj.ParentSheetIndex);
                        }
                        Vector2 globalPosition = p.ToVector2() * 64 + new Vector2(32f, 16);
                        if (obj.bigCraftable.Value)
                            globalPosition -= new Vector2(0, 64);
                        var layerDepth = (globalPosition.Y + (obj.bigCraftable.Value ? 81 : 33) + Config.DrawOffsetZ) / 10000f;
                        var position = Game1.GlobalToLocal(globalPosition);
                        b.Draw(texture, position, sourceRect, Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, SpriteEffects.None, layerDepth);
                        var hatString = HatAt(__instance, p.X, p.Y);
                        if (hatString is not null && int.TryParse(hatString, out var hat))
                        {
                            b.Draw(FarmerRenderer.hatsTexture, position + new Vector2(-3f, -6f) * 4f, new Rectangle(hat * 20 % FarmerRenderer.hatsTexture.Width, hat * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20), Color.White * Config.Alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth + 1E-05f);
                        }
                    }
                }

            }

        }
        [HarmonyPatch(typeof(Object), nameof(Object.canBePlacedHere))]
        public class Object_canBePlacedHere_Patch
        {
            public static bool Prefix(Object __instance, GameLocation l, Vector2 tile, ref bool __result)
            {
                if (!Config.EnableMod || (!__instance.IsSprinkler() && !__instance.IsScarecrow()) || !l.terrainFeatures.TryGetValue(tile, out var tf) || tf is not HoeDirt)
                    return true;
                __result = true;
                return false;
            }

        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.playerCanPlaceItemHere))]
        public class Utility_playerCanPlaceItemHere_Patch
        {
            public static bool Prefix(GameLocation location, Item item, int x, int y, Farmer f, ref bool __result)
            {
                if (!Config.EnableMod || item is not Object obj || (!obj.IsSprinkler() && obj.IsScarecrow()) || !location.terrainFeatures.TryGetValue(new Vector2(x, y), out var tf) || tf is not HoeDirt)
                    return true;
                __result = Utility.withinRadiusOfPlayer(x, y, 1, Game1.player);
                return false;
            }

        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawPlacementBounds))]
        public class Object_drawPlacementBounds_Patch
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, GameLocation location)
            {
                if (!Config.EnableMod || !Context.IsPlayerFree || (!__instance.IsSprinkler() && !__instance.IsScarecrow()) || Game1.currentLocation?.terrainFeatures?.TryGetValue(Game1.currentCursorTile, out var tf) != true || tf is not HoeDirt)
                    return true;
                var which = GetMouseCornerTile();
                var mouseTile = GetMouseCornerTile();

                Vector2 pos = Game1.GlobalToLocal(mouseTile.ToVector2() * 64 + new Vector2(32, 16));

                spriteBatch.Draw(Game1.mouseCursors, pos, new Rectangle(Utility.withinRadiusOfPlayer((int)Game1.currentCursorTile.X * 64, (int)Game1.currentCursorTile.Y * 64, 1, Game1.player) ? 194 : 210, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);

                if (Config.ShowRangeWhenPlacing)
                {
                    foreach(var tile in __instance.IsSprinkler() ? GetSprinklerTiles(mouseTile.ToVector2(), GetSprinklerRadius(__instance)) : GetScarecrowTiles(mouseTile.ToVector2(), __instance.GetRadiusForScarecrow()))
                    {
                        spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(tile * 64), new Rectangle(194, 388, 16, 16), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                    }
                }
                if(__instance.bigCraftable.Value)
                    pos -= new Vector2(0, 64);
                Texture2D texture = null;
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
                Rectangle sourceRect = itemData.GetSourceRect(__instance is Mannequin ? 2 : 0, new int?(__instance.ParentSheetIndex));
                texture = itemData.GetTexture();
                if (atApi is not null && __instance.modData.ContainsKey("AlternativeTextureName"))
                {
                    texture = GetAltTextureForObject(__instance, out sourceRect);
                }
                if (texture is null)
                {
                    texture = Game1.bigCraftableSpriteSheet;
                    sourceRect = Object.getSourceRectForBigCraftable(__instance.ParentSheetIndex);
                }
                spriteBatch.Draw(texture, pos, sourceRect, Color.White * Config.Alpha, 0, Vector2.Zero, Config.Scale, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.02f);

                return false;
            }

        }

        [HarmonyPatch(typeof(Axe), nameof(Axe.DoFunction))]
        public class Axe_DoFunction_Patch
        {
            public static bool Prefix(GameLocation location, int x, int y, int power, Farmer who)
            {
                if (!Config.EnableMod || power > 1)
                    return true;
                Vector2 placementTile = new Vector2(x, y);

                Rectangle boundingBox = new Rectangle(x * 64, y * 64, 64, 64);
                using (List<ResourceClump>.Enumerator enumerator2 = location.resourceClumps.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        if (enumerator2.Current.getBoundingBox().Intersects(boundingBox))
                        {
                            return true;
                        }
                    }
                }

                var tile = GetMouseCornerTile();
                if (Vector2.Distance(tile.ToVector2(), new Vector2(x, y) / 64) < 3 && (ReturnOrDropSprinkler(location, tile.X, tile.Y, who, true) || ReturnOrDropScarecrow(location, tile.X, tile.Y, who, true)))
                {
                    location.playSound("hammer");
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Pickaxe), nameof(Pickaxe.DoFunction))]
        public class Pickaxe_DoFunction_Patch
        {
            public static bool Prefix(GameLocation location, int x, int y, int power, Farmer who)
            {
                if (!Config.EnableMod)
                    return true; 
                var tile = GetMouseCornerTile();
                if (Vector2.Distance(tile.ToVector2(), new Vector2(x, y) / 64) < 3 && (ReturnOrDropSprinkler(location, tile.X, tile.Y, who, true) || ReturnOrDropScarecrow(location, tile.X, tile.Y, who, true)))
                {
                    location.playSound("woodyHit");
                    return false;
                }
                return true;
            }
        }

    }
}