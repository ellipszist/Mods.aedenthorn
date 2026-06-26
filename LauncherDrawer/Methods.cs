using StardewValley;

namespace LauncherDrawer
{
    public partial class ModEntry
    {

        public static void ToggleMenu()
        {
            drawerOpen.Value = !drawerOpen.Value;
            Game1.playSound(drawerOpen.Value ? "bigSelect" : "bigDeSelect");
        }
    }
}