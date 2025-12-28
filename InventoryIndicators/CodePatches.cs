using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;

namespace InventoryIndicators
{
	public partial class ModEntry
    {
        public static void InventoryMenu_hover_Prefix(InventoryMenu __instance, int x, int y, Item heldItem)
        {
            if (!Config.ModEnabled || hoverItem != null)
                return;
            foreach (ClickableComponent c in __instance.inventory)
            {
                int slotNumber = Convert.ToInt32(c.name);
                c.scale = Math.Max(1f, c.scale - 0.025f);
                if (c.containsPoint(x, y) && slotNumber < __instance.actualInventory.Count && __instance.actualInventory[slotNumber] != null && __instance.highlightMethod(__instance.actualInventory[slotNumber]))
                {
                    hoverItem = __instance.actualInventory[slotNumber].QualifiedItemId;
                }
            }
        }
        
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
            if (!Config.ModEnabled || __result == null || __instance.QualifiedItemId != hoverItem || (loveText == null && !seed && !bundle))
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
