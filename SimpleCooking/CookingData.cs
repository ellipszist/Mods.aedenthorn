using StardewValley;

namespace SimpleCooking
{
    public class CookingData
    {
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public float Burned { get; set; }
        public int Quality { get; set; }
        public string InputID { get; set; }
        public string ProductID { get; set; }
        public string BurntID { get; set; }
        public bool Smoke { get; set; }
        public string CookedSound { get; set; } = "fireball";
        public string BurntSound { get; set; } = "furnace";
    }
}