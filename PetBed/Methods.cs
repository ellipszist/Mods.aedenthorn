using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace PetBed
{
    public partial class ModEntry
    {

        private static bool WarpPetToBed(Pet pet, GameLocation location, bool outdoor)
        {
            if(Config.Debug)
            {
                SHelper.GameContent.InvalidateCache(dictPath);
            }
            var dict = SHelper.GameContent.Load<Dictionary<string, PetBedData>>(dictPath);
            string which = outdoor ? Config.OutdoorBedName : Config.IndoorBedName;
            bool canFlip = true;
            List<string> names = new List<string>();
            if (which.Contains(";"))
            {
                string[] parts = which.Split(';');
                foreach (string s in parts)
                {
                    if (s.StartsWith(pet.Name + ":"))
                    {
                        names.Add(s.Substring((pet.Name + ":").Length));
                    }
                    else if (!s.Contains(":"))
                        names.Add(s);
                }
            }
            else
            {
                names.Add(which);
            }
            Furniture bed = null;
            SMonitor.Log($"{names.Count} possible beds for {pet.Name}");
            if (!names.Any())
            {
                if (dict.Any())
                {
                    names.AddRange(dict.Keys);
                }
            }
            if (!names.Any())
            {

                SMonitor.Log($"No bed found for '{pet.Name}'");
                return false;
            }
            
            Vector2 sleeping_tile = new Vector2(-1, -1);
            names = ShuffleList(names);
            foreach (string name in names)
            {
                SMonitor.Log($"Checking bed {name}");
                Vector2 offset = Vector2.Zero;
                bool? isBed = null;
                if (name.Contains(","))
                {
                    string[] parts = name.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int X) && int.TryParse(parts[1], out int Y))
                    {
                        if (location.isCharacterAtTile(sleeping_tile) != null)
                            continue;
                        sleeping_tile = new Vector2(X, Y);
                        SMonitor.Log($"Setting sleeping tile manually to {sleeping_tile}");
                    }
                }
                if (sleeping_tile.X == -1)
                {
                    List<Furniture> flist = new List<Furniture>();
                    foreach (Furniture furniture in location.furniture)
                    {
                        //SMonitor.Log($"Checking furniture {furniture.Name} is {name}");

                        if ((furniture.ItemId == name || furniture.Name == name || furniture.Name.EndsWith($"_{name}")) && (location.isCharacterAtTile(furniture.TileLocation) == null || location.isCharacterAtTile(furniture.TileLocation) == pet))
                        {
                            flist.Add(furniture);
                        }
                    }
                    if (flist.Count > 0)
                    {
                        bed = flist[Game1.random.Next(0, flist.Count)];
                        if (dict.TryGetValue(bed.ItemId, out PetBedData data) && data.PetTypes.Split(',').Contains(pet.petType.Value))
                        {
                            sleeping_tile = bed.TileLocation;
                            offset = new Vector2(data.X, data.Y);
                            isBed = true;
                            canFlip = data.CanFlip;
                        }
                        else
                        {
                            sleeping_tile = bed.TileLocation;
                        }
                        SMonitor.Log($"Found ped bed {name} at {sleeping_tile}");
                    }
                }
                if (sleeping_tile.X > -1)
                {
                    if (offset == Vector2.Zero)
                    {
                        string offsetString = outdoor ? Config.OutdoorBedOffset : Config.IndoorBedOffset;
                        var offsetParts = offsetString.Split(',');
                        if (offsetParts.Length == 2 && int.TryParse(offsetParts[0].Trim(), out int oX) && int.TryParse(offsetParts[1].Trim(), out int oY))
                        {
                            offset = new Vector2(oX, oY);
                        }
                    }
                    SMonitor.Log($"Moving pet to {sleeping_tile}, pixel offset {offset}");

                    pet.UpdateSleepingOnBed();
                    pet.isSleepingOnFarmerBed.Value = isBed ?? outdoor ? Config.OutdoorIsBed : Config.IndoorIsBed;
                    pet.faceDirection(2);
                    Game1.warpCharacter(pet, location, sleeping_tile);
                    pet.position.Value += offset;

                    AccessTools.FieldRefAccess<Pet, string>(pet, "_currentBehavior") = pet.CurrentBehavior;

                    pet.Halt();
                    pet.Sprite.CurrentAnimation = null;
                    pet.OnNewBehavior();
                    while (!canFlip && pet.Sprite.CurrentAnimation[pet.Sprite.currentAnimationIndex].flip)
                    {
                        pet.OnNewBehavior();
                    }
                    pet.Sprite.UpdateSourceRect();
                    pet.isSleeping.Value = true;
                    pet.isSleeping.Value = true;
                    if (bed != null)
                        pet.modData[sleepingKey] = bed.TileLocation.ToString();
                    return true;
                }
            }

            SMonitor.Log($"No bed found for '{pet.Name}'");
            return false;

        }
        public static List<T> ShuffleList<T>(List<T> _list)
        {
            List<T> list = new List<T>(_list);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
