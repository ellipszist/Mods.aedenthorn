using Microsoft.Xna.Framework;
using StardewValley;

namespace ImmersiveSprinklers
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
            return ModEntry.GetSprinklerAtMouse();
        }
        public Object GetObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner)
        {
            if (!ModEntry.GetSprinklerTileBool(location, ref tile, ref corner, out var str))
                return null;

            location.terrainFeatures.TryGetValue(tile, out var tf);
            return ModEntry.GetSprinkler(tf, corner, false);
        }
        public bool IsObjectAtMouse()
        {
            var tile = Game1.currentCursorTile;
            var corner = ModEntry.GetMouseCorner();
            return ModEntry.GetSprinklerTileBool(Game1.currentLocation, ref tile, ref corner, out var str);
        }
        public bool IsObjectAtTileCorner(GameLocation location, ref Vector2 tile, ref int corner)
        {
            return ModEntry.GetSprinklerTileBool(location, ref tile, ref corner, out var str);
        }
    }
}