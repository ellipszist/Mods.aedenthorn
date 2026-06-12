using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SimpleCooking
{
    public class CookableData
    {
        public string ProductID { get; set; }
        public string BurntID { get; set; } = "(O)382";
        public string CookingTexture { get; set; }
        public bool ShowSmoke { get; set; }
        public int CookTime { get; set; }
        public float Burned { get; set; } = 2f;
    }

}