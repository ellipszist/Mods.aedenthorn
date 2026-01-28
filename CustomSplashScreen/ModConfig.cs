
using Microsoft.Xna.Framework;

namespace CustomSplashScreen
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
        public Color BackgroundColor { get; set; } = Color.Black;
        public double AltSurpriseChance { get; set; } = 0.02;
    }
}
