using FashionSense.Framework.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;

namespace FashionSenseAppearanceMenu
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(HandMirrorMenu), nameof(HandMirrorMenu.receiveKeyPress))]
        public class HandMirrorMenu_receiveKeyPress_Patch
        {

            public static bool Prefix(HandMirrorMenu __instance, Keys key)
            {
                if (!Config.ModEnabled || Config.MenuKey != (SButton)key)
                    return true;
                string filter = (string)AccessTools.Method(typeof(HandMirrorMenu), "GetNameOfEnabledFilter").Invoke(__instance, Array.Empty<Type>());
                Game1.activeClickableMenu = new FashionSenseAppearanceMenuMenu(__instance, Game1.player, filter);
                Game1.playSound("bigSelect");
                return false;
            }
        }

    }
}