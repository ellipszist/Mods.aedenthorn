using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;

namespace MineShipping
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public static class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || __instance is not MineShaft shaft || Game1.getFarm() is not Farm farm)
                    return true;
                Point binTile = shaft.tileBeneathLadder.ToPoint() + new Point(1, -1);
                if((tileLocation.X == binTile.X || tileLocation.X == binTile.X + 1 )&& tileLocation.Y == binTile.Y)
                {
                    ItemGrabMenu itemGrabMenu = new ItemGrabMenu(null, true, false, new InventoryMenu.highlightThisItem(Utility.highlightShippableObjects), new ItemGrabMenu.behaviorOnItemSelect(shipItem), "", null, true, true, false, true, false, 0, null, -1, null, ItemExitBehavior.ReturnToPlayer, false);
                    itemGrabMenu.initializeUpperRightCloseButton();
                    itemGrabMenu.setBackgroundTransparency(false);
                    itemGrabMenu.setDestroyItemOnClick(true);
                    itemGrabMenu.initializeShippingBin();
                    Game1.activeClickableMenu = itemGrabMenu;
                    if (who.IsLocalPlayer)
                    {
                        Game1.playSound("shwip", null);
                    }
                    if (Game1.player.FacingDirection == 1)
                    {
                        Game1.player.Halt();
                    }
                    Game1.player.showCarrying();
                    __result = true;
                    return false;
                }
                return true;
            }
        }
        //[HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.behaviors))]
        public static class FarmAnimal_behaviors_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FarmAnimal.behaviors");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>), "ContainsKey"))
                    {
                        SMonitor.Log("Intercepting check for terrain feature");
                        codes[i].opcode = OpCodes.Call;
                        //codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(CheckForGrass));
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}