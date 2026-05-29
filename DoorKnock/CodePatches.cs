using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using System;
using System.Linq;
using xTile;
using xTile.Tiles;

namespace DoorKnock
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.Halt))]
        public static class Farmer_Halt_Patch
        {
            public static bool Prefix(Farmer __instance)
            {
                return farmerController.Value == null;
                     
            }
        }
            
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.Update))]
        public static class Farmer_Update_Patch
        {
            public static void Prefix(Farmer __instance, GameTime time)
            {
                if (__instance == Game1.player && farmerController.Value != null)
                {
                    Farmer player = __instance;
                    player.facingDirection.Value = 2;
                    player.setRunning(false, true);
                    player.ignoreCollisions = true;
                    if(!player.movementDirections.Contains(2))
                        player.movementDirections.Add(2);
                }
            }
            public static void Postfix(Farmer __instance, GameTime time)
            {

                if (__instance == Game1.player && farmerController.Value != null)
                {

                    Vector2 start = farmerController.Value.Value;
                    Farmer player = __instance;
                    if (Math.Abs(Vector2.Distance(player.Position, start)) + player.Speed > 64)
                    {
                        farmerController.Value = null;
                        player.ignoreCollisions = false;
                        player.Halt();
                        player.faceDirection(0);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(InteriorDoorDictionary), nameof(InteriorDoorDictionary.ResetLocalState))]
        public static class InteriorDoorDictionary_ResetLocalState_Patch
        {
            public static void Postfix(InteriorDoorDictionary __instance)
            {
                if (!Config.ModEnabled)
                {
                    return;
                }
                foreach (var kvp in __instance.Pairs)
                {
                    if (kvp.Value != true)
                    {
                        try
                        {
                            var door = Game1.currentLocation.interiorDoors.Doors.First(d => d.Position == kvp.Key);
                            Map map = door.Location.Map;
                            if (map == null)
                            {
                                continue;
                            }
                            if (door != null && map.RequireLayer("Buildings").Tiles[kvp.Key.X, kvp.Key.Y] == null)
                            {
                                if (door.Tile == null)
                                {
                                    continue;
                                }
                                map.RequireLayer("Buildings").Tiles[kvp.Key.X, kvp.Key.Y] = door.Tile;
                                door.Location.removeTileProperty(door.Position.X, door.Position.Y, "Back", "TemporaryBarrier");
                                map.RequireLayer("Front").Tiles[kvp.Key.X, kvp.Key.Y - 1] = new StaticTile(map.RequireLayer("Front"), door.Tile.TileSheet, BlendMode.Alpha, door.Tile.TileIndex - door.Tile.TileSheet.SheetWidth);
                                map.RequireLayer("Front").Tiles[kvp.Key.X, kvp.Key.Y - 2] = new StaticTile(map.RequireLayer("Front"), door.Tile.TileSheet, BlendMode.Alpha, door.Tile.TileIndex - door.Tile.TileSheet.SheetWidth * 2);
                            }
                        }
                        catch { }
                    }
                }
            }
        }
    }
}