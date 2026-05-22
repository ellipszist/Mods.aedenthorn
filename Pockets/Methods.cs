using Newtonsoft.Json;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects;
using System.Collections.Generic;

namespace Pockets
{
    public partial class ModEntry
    {

        public static bool TryGetPocket(Farmer who, out PocketData data, out IList<Item> items, int x = -1, int y = -1)
        {
            if (who.pantsItem.Value is Clothing s)
            {
                if (TryGetPocketInventory(s, out data, out items, x, y))
                    return true;
                foreach (var kvp in Config.DefaultPockets)
                {
                    if (kvp.Value.ClothesType == Clothing.ClothesType.PANTS)
                    {
                        if (TryGetDefaultPocketInventory(who.UniqueMultiplayerID, kvp.Key, kvp.Value, out items, x, y))
                        {
                            data = kvp.Value;
                            return true;
                        }
                    }
                }
            }
            if (who.shirtItem.Value is Clothing p)
            {
                if (TryGetPocketInventory(p, out data, out items, x, y))
                    return true;
                foreach (var kvp in Config.DefaultPockets)
                {
                    if (kvp.Value.ClothesType == Clothing.ClothesType.SHIRT)
                    {
                        if (TryGetDefaultPocketInventory(who.UniqueMultiplayerID, kvp.Key, kvp.Value, out items, x, y))
                        {
                            data = kvp.Value;
                            return true;
                        }
                    }
                }
            }
            data = null;
            items = null;
            return false;
        }
        private static bool TryGetPocketInventory(Item item, out PocketData data, out IList<Item> items, int x, int y)
        {
            if (PocketDict.TryGetValue(item.ItemId, out var clothesPockets))
            {
                foreach(var kvp in clothesPockets)
                {
                    if (SHelper.Input.IsDown(kvp.Value.HotKey) || (x > -1 && kvp.Value.StartX <= x && kvp.Value.Width > x && kvp.Value.StartY <= y && kvp.Value.Height > y))
                    {
                        data = kvp.Value;
                        if (!InventoryDict.TryGetValue(item, out var list))
                        {
                            if(item.modData.TryGetValue(modKey, out var str))
                            {
                                try
                                {
                                    list = JsonConvert.DeserializeObject<Dictionary<string, Inventory>>(str);
                                    InventoryDict[item] = list;
                                }
                                catch
                                {

                                }

                            }
                        }
                        if (list is null)
                        {
                            list = new Dictionary<string, Inventory>();
                            InventoryDict[item] = list;
                        }
                        if (!list.TryGetValue(kvp.Key, out var inv))
                        {
                            inv = new Inventory();
                            list[kvp.Key] = inv;
                        }
                        items = inv;
                        return true;
                    }
                }
            }
            data = null;
            items = null;
            return false;
        }


        private static bool TryGetDefaultPocketInventory(long who, string pocketId, PocketData data, out IList<Item> items, int x, int y)
        {
            if (SHelper.Input.IsDown(data.HotKey) || (x > -1 && data.StartX <= x && data.StartX + data.Width > x && data.StartY <= y && data.StartY + data.Height > y))
            {
                if (!DefaultInventoryDict.TryGetValue(who, out var list))
                {
                    if (Game1.GetPlayer(who).modData.TryGetValue(modKey, out var str))
                    {
                        try
                        {
                            list = JsonConvert.DeserializeObject<Dictionary<string, Inventory>>(str);
                            DefaultInventoryDict[who] = list;
                        }
                        catch
                        {

                        }

                    }
                }
                if (list is null)
                {
                    list = new Dictionary<string, Inventory>();
                    DefaultInventoryDict[who] = list;
                }
                if (!list.TryGetValue(pocketId, out var inv))
                {
                    inv = new Inventory();
                    list[pocketId] = inv;
                }
                items = inv;
                return true;
            }
            items = null;
            return false;
        }
    }
}