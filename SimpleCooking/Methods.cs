using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Object = StardewValley.Object;

namespace SimpleCooking
{
    public partial class ModEntry
    {
        public static Color SwitchColor(Color color, Object obj)
        {
            if(!Config.ModEnabled) { return color; }
            if (obj.ItemId.StartsWith(grilledPrefix))
            {
                return Config.GrilledColor;
            }
            return color;
        }
        private static bool IsCookable(Object obj, out CookingData data)
        {
            data = null;
            if (obj is null)
                return false;
            if (CookableDict.TryGetValue(obj.ItemId, out var cdata))
            {
                data = new CookingData()
                {
                    LastCheckTime = Game1.timeOfDay,
                    MinutesToCook = cdata.CookTime,
                    BurnedAt = cdata.Burned,
                    Smoke = cdata.ShowSmoke,
                    InputID = obj.QualifiedItemId,
                    ProductID = cdata.ProductID,
                    Quality = obj.Quality,
                    BurntID = cdata.BurntID
                };
            }
            else if(obj.Edibility > 0 && (obj.Category == Object.FishCategory || obj.Category == Object.VegetableCategory))
            {
                data = new CookingData()
                {
                    LastCheckTime = Game1.timeOfDay,
                    MinutesToCook = obj.Category == Object.FishCategory ? Config.FishCookTime : Config.VegetableCookTime,
                    BurnedAt = obj.Category == Object.FishCategory ? Config.FishBurn : Config.VegetableBurn,
                    Smoke = true,
                    InputID = obj.QualifiedItemId,
                    ProductID = grilledPrefix + obj.ItemId,
                    Quality = obj.Quality,
                    BurntID = "(O)382"
                };
            }
            else
            {
                return false;
            }
            return true;
        }
        public static bool TryGetCookingData(Object obj, out CookingData data)
        {
            if (obj?.modData.TryGetValue(cookingKey, out var str) == true)
            {
                data = JsonConvert.DeserializeObject<CookingData>(str);
                return true;
            }
            data = null;
            return false;
        }
        public static int AddMinutes(int time, int min)
        {
            var hours = time / 100 + min / 60;
            var mins = time % 100 + min % 60;
            while(mins > 60)
            {
                mins -= 60;
                hours++;
            }
            return hours * 100 + mins;
        }

        private static CookingData GetCookingData(string str)
        {
            return JsonConvert.DeserializeObject<CookingData>(str);
        }
        public static void TryReturnObject(Item obj, Farmer who)
        {
            if (obj is null)
                return;
            if (!who.addItemToInventoryBool(obj))
            {
                who.currentLocation.debris.Add(new Debris(obj, who.Position));
            }
        }
        public static int TimeToMinutes(int time)
        {
            return (time / 100 * 60 + time % 100);
        }
        public static int MinutesToTime(int time)
        {
            return (time / 60 * 100  + time % 60);
        }

    }
}