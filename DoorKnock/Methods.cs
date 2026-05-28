using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Pathfinding;
using System;
using System.Collections.Generic;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;

namespace DoorKnock
{
    public partial class ModEntry
    {
        public static void DoneDelaying(NPC npc)
        {
            var ps = npc.modData[answerPointKey].Split(',');
            npc.modData.Remove(answerPointKey);
            var schedule = npc.pathfindToNextScheduleLocation("knock", Game1.currentLocation.Name, npc.TilePoint.X, npc.TilePoint.Y, Game1.currentLocation.Name, int.Parse(ps[0]), int.Parse(ps[1]), int.Parse(ps[2]), null, null);
            npc.queuedSchedulePaths.Clear();
            npc.queuedSchedulePaths.Add(schedule);
            AccessTools.Method(typeof(NPC), "prepareToDisembarkOnNewSchedulePath").Invoke(npc, Array.Empty<object>());
            npc.modData[returnPointKey] = $"{npc.TilePoint.X},{npc.TilePoint.Y},{npc.FacingDirection}";
            npc.controller = new PathFindController(schedule.route, npc, Utility.getGameLocationOfCharacter(npc))
            {
                finalFacingDirection = schedule.facingDirection,
                endBehaviorFunction = new PathFindController.endBehavior(WaitBehaviour),
            };

        }
       public static void WaitBehaviour(Character c, GameLocation l)
        {
            NPC npc = c as NPC;
            //var doorTile = c.TilePoint + new Point(0, c.FacingDirection == 0 ? -1 : 1);
            //c.currentLocation.openDoor(new xTile.Dimensions.Location(c.TilePoint.X, c.TilePoint.Y), true);
            returnDict[npc.Name] = Config.WaitTime;
        } 
        public static void DoneWaiting(NPC npc)
        {
            //TryCloseDoor(npc.currentLocation, npc.TilePoint);
            var ps = npc.modData[returnPointKey].Split(',');
            npc.modData.Remove(returnPointKey);
            var schedule = npc.pathfindToNextScheduleLocation("knock", Game1.currentLocation.Name, npc.TilePoint.X, npc.TilePoint.Y, Game1.currentLocation.Name, int.Parse(ps[0]), int.Parse(ps[1]), int.Parse(ps[2]), null, null);
            npc.queuedSchedulePaths.Clear();
            npc.queuedSchedulePaths.Add(schedule);
            npc.controller = new PathFindController(schedule.route, npc, Utility.getGameLocationOfCharacter(npc))
            {
                finalFacingDirection = int.Parse(ps[2]),
                endBehaviorFunction = new PathFindController.endBehavior(ReturnedBehaviour)
            };

        }
        public static void ReturnedBehaviour(Character c, GameLocation l)
        {
            
            NPC npc = c as NPC;
            npc.faceTowardFarmerForPeriod(3000, 100, false, Game1.player);
        }


        private static bool TryCloseDoor(GameLocation location, Point tilePoint)
        {
            foreach (var d in location.interiorDoors.Doors)
            {
                if (d.Position == tilePoint)
                {
                    if (!d.Value)
                        return false;
                    location.playSound("doorClose", Utility.PointToVector2(tilePoint), null, SoundContext.Default);
                    d.Sprite.paused = true;
                    location.interiorDoors[tilePoint] = false;

                    Location doorLocation = new(d.Position.X, d.Position.Y);
                    Map map = d.Location.Map;
                    if (map == null)
                    {
                        return false;
                    }
                    if (d.Tile == null)
                    {
                        return false;
                    }
                    map.GetLayer("Buildings").Tiles[doorLocation] = d.Tile;
                    d.Location.removeTileProperty(d.Position.X, d.Position.Y, "Back", "TemporaryBarrier");
                    doorLocation.Y--;
                    map.GetLayer("Front").Tiles[doorLocation] = new StaticTile(map.GetLayer("Front"), d.Tile.TileSheet, BlendMode.Alpha, d.Tile.TileIndex - d.Tile.TileSheet.SheetWidth);
                    doorLocation.Y--;
                    map.GetLayer("Front").Tiles[doorLocation] = new StaticTile(map.GetLayer("Front"), d.Tile.TileSheet, BlendMode.Alpha, d.Tile.TileIndex - d.Tile.TileSheet.SheetWidth * 2);

                    d.ResetLocalState();
                    return true;
                }
            }
            return false;
        }
    }
}