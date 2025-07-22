using Microsoft.Xna.Framework;

namespace CustomMounts
{
    public class MountData
    {
        public string Name;
        public string Stable;
        public Point SpawnOffset = new Point(1,1);
        public string FootstepSound;
        public string FootstepSoundWood;
        public string FootstepSoundStone;
        public int Speed = 2;
        public float HorizontalSizeDiff;
        public float VerticalSizeDiff;
        public Point Size = Point.Zero;
        public string TexturePath;
        public int FrameWidth = 32;
        public int FrameHeight = 32;
        public string EatItem = "(O)Carrot";
        public string EatSound = "eat";
        public float EatSpeedBonus = 0.4f;
        public string FluteItem = "(O)911";
        public string FluteSound = "horse_flute";
        public bool AllowHats = true;
        public float HatScale = 1;
        public Vector2[] HatOffsets = {
            Vector2.Zero,
            Vector2.Zero,
            Vector2.Zero,
            Vector2.Zero
        };
    }
}