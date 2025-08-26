using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace IndoorOutdoor
{
    public partial class ModEntry
    {
        public static Rectangle ToXna(xTile.Dimensions.Rectangle rect)
        {
            return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }
        public static xTile.Dimensions.Rectangle ToXTile(Rectangle rect)
        {
            return new xTile.Dimensions.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }


        public static bool DrawOverrideVector(Texture2D texture, Vector2 position, Rectangle? sourceRectangle) 
        {
            if (!Config.ModEnabled || !renderingWorld)
                return true;
            var rect = new Rectangle((int)position.X, (int)position.Y, (int)((sourceRectangle is null ? texture.Width : sourceRectangle.Value.Width)), (int)((sourceRectangle is null ? texture.Height : sourceRectangle.Value.Height)));
            return CheckScreenRect(rect);
        }
        public static bool DrawOverrideVectorVector(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Vector2 scale, Vector2 origin) 
        {
            if (!Config.ModEnabled || !renderingWorld)
                return true;
            var rect = new Rectangle((int)(position.X - origin.X), (int)(position.Y - origin.Y), (int)((sourceRectangle is null ? texture.Width : sourceRectangle.Value.Width) * scale.X), (int)((sourceRectangle is null ? texture.Height : sourceRectangle.Value.Height) * scale.Y));
            return CheckScreenRect(rect);
        }
        public static bool DrawOverrideRect(Texture2D texture, Rectangle destinationRectangle) 
        {
            if (!Config.ModEnabled || !renderingWorld)
                return true;
            return CheckScreenRect(destinationRectangle);
        }
        public static bool DrawOverrideRectVector(Texture2D texture, Rectangle destinationRectangle, Vector2 origin) 
        {
            if (!Config.ModEnabled || !renderingWorld)
                return true;
            destinationRectangle.Offset(-origin);
            return CheckScreenRect(destinationRectangle);
        }


        public static bool CheckScreenRect(Rectangle bb)
        {
            foreach (var kvp in currentLocationIndoorMapDict.Value)
            {
                if (kvp.Key == currentIndoors.Value)
                {
                    bool tl = false;
                    bool bl = false;
                    bool tr = false;
                    bool br = false;
                    foreach (var rect in kvp.Value)
                    {
                        var screenRect = new Rectangle(rect.Location - new Point(Game1.viewport.X, Game1.viewport.Y), rect.Size);
                        if (screenRect.Contains(bb.Center))
                        {
                            return true;
                        }
                        if (screenRect.Contains(bb.Location))
                        {
                            tl = true;
                        }
                        if (screenRect.Contains(bb.Location + new Point(bb.Width, 0)))
                        {
                            tr = true;
                        }
                        if (screenRect.Contains(bb.Location + new Point(bb.Width, bb.Height)))
                        {
                            br = true;
                        }
                        if (screenRect.Contains(bb.Location + new Point(0, bb.Height)))
                        {
                            bl = true;
                        }
                    }
                    return tl && bl && tr && br;
                }
                else
                {
                    bool tl = false;
                    bool bl = false;
                    bool tr = false;
                    bool br = false;
                    foreach (var rect in kvp.Value)
                    {
                        var screenRect = new Rectangle(rect.Location - new Point(Game1.viewport.X, Game1.viewport.Y), rect.Size);
                        if (screenRect.Contains(bb.Location))
                        {
                            tl = true;
                        }
                        if (screenRect.Contains(bb.Location + new Point(bb.Width, 0)))
                        {
                            tr = true;
                        }
                        if (screenRect.Contains(bb.Location + new Point(bb.Width, bb.Height)))
                        {
                            br = true;
                        }
                        if (screenRect.Contains(bb.Location + new Point(0, bb.Height)))
                        {
                            bl = true;
                        }
                    }
                    return !tl || !bl || !tr || !br;
                }
            }
            return currentIndoors.Value == null;
        }
    }
}