using Newtonsoft.Json;
using StardewValley;
using System;
using Object = StardewValley.Object;

namespace SimpleCooking
{
    public class CookingData
    {
        public int MinutesProgress { get; set; }
        public int MinutesToCook { get; set; }
        public int LastCheckTime { get; set; }
        public float Progress 
        {
            get
            {
                return MinutesProgress / (float)MinutesToCook;
            }
        }
        public bool Burned
        {
            get
            {
                return Progress >= BurnedAt;
            }
        }
        public float BurnedAt { get; set; }
        public int Quality { get; set; }
        public string InputID { get; set; }
        public string ProductID { get; set; }
        public string BurntID { get; set; }
        public bool Smoke { get; set; }
        public string PlacedSound { get; set; } = "fireball";
        public string CookedSound { get; set; } = "fireball";
        public string BurntSound { get; set; } = "furnace";

        public Object GetProduct()
        {
            if (Progress < 1)
                return null;
            return ItemRegistry.Create<Object>(ProductID, 1, Quality);
        }

        public bool WillCook(int time)
        {
            if (Progress >= 1)
                return false;
            var min = ModEntry.TimeToMinutes(time) - ModEntry.TimeToMinutes(LastCheckTime);
            return (MinutesProgress + min) / (float)MinutesToCook >= 1;
        }

        public bool WillBurn(int time)
        {
            if (Burned)
                return false;
            var min = ModEntry.TimeToMinutes(time) - ModEntry.TimeToMinutes(LastCheckTime);
            return (MinutesProgress + min) / (float)MinutesToCook >= BurnedAt;
        }

        public void Update(Object obj, int timeOfDay)
        {
            if (Burned)
                return;
            if (timeOfDay < LastCheckTime)
                timeOfDay += 2400;
            if (WillBurn(timeOfDay))
            {
                obj.Location.playSound(BurntSound, obj.TileLocation);
            }
            else if (WillCook(timeOfDay))
            {
                obj.Location.playSound(CookedSound, obj.TileLocation);
            }
            var min = ModEntry.TimeToMinutes(timeOfDay) - ModEntry.TimeToMinutes(LastCheckTime);
            MinutesProgress += min;
            LastCheckTime = timeOfDay;
            obj.modData[ModEntry.cookingKey] = JsonConvert.SerializeObject(this);
        }
    }
}