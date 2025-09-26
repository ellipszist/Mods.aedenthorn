using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace InventoryIndicators
{
	public partial class ModEntry
    {
        public static void drawInMenu_Prefix(Item __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
        {
            DrawBefore(__instance, spriteBatch, location, scaleSize, transparency, layerDepth);
        }

        public static void drawInMenu_Postfix(Item __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
        {
            if (!Config.ModEnabled)
                return;
            DrawAfter(__instance, spriteBatch, location, scaleSize, transparency, layerDepth);
        }
        public static void getDescription_Postfix(Item __instance, ref string __result)
        {
            if (!Config.ModEnabled || __result == null)
                return;
            var text = "";
            if(loveText != null)
                text += loveText + " ";
            if(seed)
                text += SHelper.Translation.Get("can_plant") + " ";
            if(bundle)
                text += SHelper.Translation.Get("can_bundle") + " ";

            __result += "\n" + Game1.parseText(text, Game1.smallFont, (int)AccessTools.Method(typeof(Item), "getDescriptionWidth").Invoke(__instance, new object[0]));
        }
    }
}
