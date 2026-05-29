using HarmonyLib;
using StardewValley;
using StardewValley.Extensions;
using System.Linq;
using xTile;
using xTile.Tiles;

namespace DoorKnock
{
    public partial class ModEntry
    {
        //[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performTouchAction), new Type[] {typeof(string[]), typeof(Vector2) })]
        //public static class GameLocation_performTouchAction_Patch
        //{
        //    public static bool Prefix(GameLocation __instance, string[] action, Vector2 playerStandingPosition)
        //    {
        //        return true;
        //        if (!Config.ModEnabled || !ArgUtility.TryGet(action, 0, out var actionType, out var error, true, "string actionType") || actionType != "Door" || !__instance.interiorDoors.TryGetValue(playerStandingPosition.ToPoint(), out var open) || !open)
        //        {
        //            return true;
        //        }

        //        return false;
        //    }
        //}
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