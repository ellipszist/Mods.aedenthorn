using Microsoft.Xna.Framework;
using StardewValley;

namespace ImmersiveScarecrows
{
    public interface IImmersiveApi
    {
        public Object GetObjectAtMouse();
        public Object GetObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner);
        public bool IsObjectAtMouse();
        public bool IsObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner);
    }
    public class ImmersiveApi : IImmersiveApi
    {
        public Object GetObjectAtMouse()
        {
            return ModEntry.GetScarecrowAtMouse();
        }
        public Object GetObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner)
        {
            if (!ModEntry.GetScarecrowTileBool(location, ref tile, ref corner))
                return null;

            location.terrainFeatures.TryGetValue(tile, out var tf);
            return ModEntry.GetScarecrow(tf, corner);
        }
        public bool IsObjectAtMouse()
        {
            var tile = Game1.currentCursorTile;
            var corner = ModEntry.GetMouseCorner();
            return ModEntry.GetScarecrowTileBool(Game1.currentLocation, ref tile, ref corner);
        }
        public bool IsObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner)
        {
            return ModEntry.GetScarecrowTileBool(location, ref tile, ref corner);
        }
    }
}