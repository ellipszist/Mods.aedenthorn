using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewGames
{
	public partial class ModEntry
    {
        public static ClickableTextureComponent playButton;
        public static bool hovering;
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.setUpIcons))]
        public class TitleMenu_setUpIcons_Patch
        {
            public static void Postfix(TitleMenu __instance)
            {
                if (!Config.ModEnabled)
                {
                    return;
                }
                playButton = new ClickableTextureComponent("play games", new Rectangle(__instance.width + -22 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, __instance.height - 25 * TitleMenu.pixelZoom * 3 - 24 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom, 25 * TitleMenu.pixelZoom), null, "", SHelper.ModContent.Load<Texture2D>("assets/play.png"), new Rectangle(0, 0, 27, 25), TitleMenu.pixelZoom, false);
            }
        }
        [HarmonyPatch(typeof(TitleMenu), "ShouldDrawCursor")]
        public class TitleMenu_draw_Patch
        {
            public static void Prefix(TitleMenu __instance)
            {
                if (!Config.ModEnabled || TitleMenu.subMenu is not null || __instance.isTransitioningButtons || !__instance.titleInPosition || __instance.transitioningCharacterCreationMenu || !__instance.HasActiveUser)
                {
                    return;
                }
                playButton?.draw(Game1.spriteBatch);
                if (hovering)
                {
                    IClickableMenu.drawHoverText(Game1.spriteBatch, SHelper.Translation.Get("games"), Game1.smallFont);
                }
            }
        }
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.receiveLeftClick))]
        public class TitleMenu_receiveLeftClick_Patch
        {
            public static void Postfix(TitleMenu __instance, int x, int y)
            {
                if (!Config.ModEnabled || (TitleMenu.subMenu is not null && !TitleMenu.subMenu.readyToClose()) || __instance.isTransitioningButtons)
                {
                    return;
                }
                if (playButton.visible && playButton.containsPoint(x, y))
                {
                    TitleMenu.subMenu = new GamesLoadGameMenu();
                    Game1.playSound("newArtifact", null);
                }
            }
        }
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.performHoverAction))]
        public class TitleMenu_performHoverAction_Patch
        {
            public static void Postfix(TitleMenu __instance, int x, int y)
            {
                if (!Config.ModEnabled || (TitleMenu.subMenu is not null && !TitleMenu.subMenu.readyToClose()) || __instance.isTransitioningButtons)
                {
                    return;
                }
                hovering = false;
                if (playButton.visible)
                {
                    playButton.tryHover(x, y, 0.25f);
                    if (playButton.containsPoint(x, y))
                    {
                        if (playButton.sourceRect.X == 0)
                        {
                            Game1.playSound("Cowboy_Footstep", null);
                        }
                        playButton.sourceRect.X = 27;
                        hovering = true;
                        return;
                    }
                    playButton.sourceRect.X = 0;
                }
            }
        }
    }
}
