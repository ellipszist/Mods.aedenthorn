using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using System;
using Object = StardewValley.Object;

namespace CraftableBalloons
{
	public partial class ModEntry
    {

        [HarmonyPatch(typeof(ObjectDataDefinition), nameof(ObjectDataDefinition.CreateItem))]
        public static class ObjectDataDefinition_CreateItem_Patch
        {
            public static bool Prefix(ParsedItemData data, ref Item __result)
            {
                if (!Config.ModEnabled || data.ItemId != balloonKey)
                    return true;
                var color = Utility.getRandomRainbowColor();
                __result =  new ColoredObject(balloonKey, 1, color);
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.IsHeldOverHead))]
        public static class Object_IsHeldOverHead_Patch
        {
            public static bool Prefix(Object __instance, ref bool __result)
            {
                if (!Config.ModEnabled || __instance.ItemId != balloonKey)
                    return true;
                __result = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(ColoredObject), nameof(ColoredObject.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool)})]
        public static class Object_drawInMenu_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
            {
                if (!Config.ModEnabled || __instance.ItemId != balloonKey)
                    return;
                var tex = SHelper.GameContent.Load<Texture2D>(balloonPath);
                spriteBatch.Draw(tex, location, new Rectangle(16, 0, 16, 16), Color.White * transparency, 0f, Vector2.Zero, 4f * scaleSize, SpriteEffects.None, layerDepth + 1);
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) })]
        public static class Farmer_draw_Patch
        {
            public static void Postfix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || __instance.ActiveObject?.ItemId != balloonKey)
                    return;
                DrawBalloon(b, (__instance.ActiveObject as ColoredObject).color.Value, __instance.Position, __instance.FacingDirection, farmerMovement.TryGetValue(__instance.UniqueMultiplayerID, out var data) ? data.vel : Point.Zero, __instance.StandingPixel.Y);
            }
        }
    }
}
