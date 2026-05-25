using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Audio;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;

namespace CloseDoors
{
    public partial class ModEntry
    {
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
        private static bool IsDoorOpen(GameLocation location, Point tilePoint)
        {

            foreach (var d in location.interiorDoors.Doors)
            {
                if (d.Position == tilePoint)
                {
                    return d.Value;
                }
            }
            return false;
        }
    }
}