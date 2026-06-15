
using Microsoft.Xna.Framework;

namespace WallClock
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool FixClocks { get; set; } = true;
        public Color HourHandColor { get; set; } = new Color(100, 59, 14);
        public Color MinuteHandColor { get; set; } = new Color(100, 59, 14);
        public Color NubColor { get; set; } = new Color(100, 59, 14);
        public float Alpha { get; set; } = 0.75f;
        public bool Debug { get; set; } = false;
    }
}
