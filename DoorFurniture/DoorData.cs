
using Microsoft.Xna.Framework;

namespace DoorFurniture
{
    public class DoorData
    {
        public Rectangle[] Bounds = new Rectangle[]
        {
            new(0, 48, 64, 16),
            new(48, 0, 16, 64),
            new(0, 0, 64, 16),
            new(0, 0, 16, 64)
        };
        public bool AutoOpen = false;
        public int AutoCloseDelay = -1;
    }
}