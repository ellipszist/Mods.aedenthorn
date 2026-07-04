using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;
using System;
using Object = StardewValley.Object;

namespace NozzleAndEnricher
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Object_draw_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || !__instance.IsSprinkler() || !__instance.modData.TryGetValue(nozzleKey, out var id))
                {
                    return;
                }
                Rectangle bounds = __instance.GetBoundingBoxAt(x, y);
                ParsedItemData heldItemData = ItemRegistry.GetDataOrErrorItem(id);
                spriteBatch.Draw(heldItemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), new Rectangle?(heldItemData.GetSourceRect(1, null)), Color.White * alpha, 0f, new Vector2(8f, 8f), (__instance.scale.Y > 1f) ? __instance.getScale().Y : 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.isPassable() ? bounds.Top : bounds.Bottom) / 10000f + 1E-06f);
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.GetModifiedRadiusForSprinkler))]
        public static class Object_GetModifiedRadiusForSprinkler_Patch
        {
            public static void Postfix(Object __instance, ref int __result)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(nozzleKey, out var id))
                {
                    return;
                }
                if(!Nozzles.TryGetValue(id, out var bonus))
                {
                    return;
                }
                __result += bonus;
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.performToolAction))]
        public static class Object_performToolAction_Patch
        {
            public static bool Prefix(Object __instance, Tool t, ref bool __result)
            {
                if (!Config.ModEnabled || (t is not Pickaxe && t is not Axe) || (__instance.heldObject.Value is not null && !SHelper.Input.IsDown(Config.ModKey)) || !__instance.IsSprinkler() || !__instance.modData.TryGetValue(nozzleKey, out var id))
                {
                    return true;
                }
                __instance.playNearbySoundAll("hammer");
                var obj = ItemRegistry.Create<Object>(id);
                __instance.Location.debris.Add(new Debris(obj, __instance.TileLocation * 64f + new Vector2(32f, 32f)));
                __instance.modData.Remove(nozzleKey);
                __result = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.performRemoveAction))]
        public static class Object_performRemoveAction_Patch
        {
            public static void Postfix(Object __instance)
            {
                if (!Config.ModEnabled || !__instance.IsSprinkler() || !__instance.modData.TryGetValue(nozzleKey, out var id))
                {
                    return;
                }
                var obj = ItemRegistry.Create<Object>(id);
                __instance.Location.debris.Add(new Debris(obj, __instance.TileLocation * 64f + new Vector2(32f, 32f)));
                __instance.modData.Remove(nozzleKey);
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.performObjectDropInAction))]
        public static class Object_performObjectDropInAction_Patch
        {
            public static bool Prefix(Object __instance, ref Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.IsSprinkler() || __instance.modData.ContainsKey(nozzleKey))
                {
                    return true;
                }
                var heldId = __instance.heldObject.Value?.QualifiedItemId;
                var dropInId = dropInItem?.QualifiedItemId;
                if (heldId is not null && Nozzles.ContainsKey(heldId)) 
                {
                    if (probe)
                    {
                        __result = true;
                        return false;
                    }
                    __instance.modData[nozzleKey] = heldId;
                    __instance.heldObject.Value = null;
                }
                else if (Nozzles.ContainsKey(dropInId))
                {
                    if (!probe)
                    {
                        __instance.modData[nozzleKey] = dropInId;
                        __instance.Location.playSound("axe");
                    }
                    __result = true;
                    return false;
                }
                return true;

            }
        }
    }
}