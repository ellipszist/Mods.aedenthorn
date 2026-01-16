using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace QuestHelper
{
	public partial class ModEntry : Mod
    {

        public static void DrawQuestMarker(SpriteBatch b, Vector2 position)
        {
            float yOffset2 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(32, yOffset2 - 96)), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(395, 497, 3, 8)), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset2 / 16f), SpriteEffects.None, 1f);
        }

        public static bool IsSlimeName(string s)
        {
            return s.Contains("Slime") || s.Contains("Jelly") || s.Contains("Sludge");
        }

        private static List<string> GetItemInfo(string itemId)
        {
            var uItemId = itemId.Substring(itemId.IndexOf(')') + 1);

            List<string> list;

            list = GetFishInfo(itemId);
            if (list != null)
                return list;
            
            list = new List<string>();
            var item = ItemRegistry.Create(itemId);
            if(item is null) 
                return null;


            var r = CraftingRecipe.craftingRecipes.FirstOrDefault(kvp => kvp.Value.Contains($"/{uItemId}/"));
            int unlock = 4;
            string which = "crafting";
            if(r.Value is null)
            {
                r = CraftingRecipe.cookingRecipes.FirstOrDefault(kvp => kvp.Value.Contains($"/{uItemId}/"));
                unlock = 3;
                which = "cooking";
            }
            if (r.Value is not null)
            {
                var data = r.Value.Split('/');
                if (data.Length >= 3 && data[2] == uItemId)
                {
                    if (Game1.player.craftingRecipes.ContainsKey(r.Key))
                    {
                        list.Add(string.Format(SHelper.Translation.Get($"x-{which}-recipe-known"), item.DisplayName));
                    }
                    else
                    {
                        list.Add(string.Format(SHelper.Translation.Get($"x-{which}-recipe-unknown"), item.DisplayName));
                        if(data.Length >= unlock + 1 && !string.IsNullOrEmpty(data[unlock]))
                        {
                            var split = data[unlock].Split(' ');
                            if (split[0] == "f" && split.Length == 3)
                            {
                                list.Add(string.Format(SHelper.Translation.Get($"x-y-friendship"), split[2], Game1.getCharacterFromName(split[1])?.displayName ?? split[1]));
                            }
                            else if (split[0] == "s" && split.Length == 3)
                            {
                                list.Add(string.Format(SHelper.Translation.Get($"x-y-skill"), split[2], Farmer.getSkillDisplayNameFromIndex(Farmer.getSkillNumberFromName(split[1]))));
                            }
                            else
                            {
                                list.Add(string.Format(SHelper.Translation.Get($"x-special-recipe"), item.DisplayName));
                            }
                        }
                    }
                }
            }
            return list;
        }

        private static List<string> GetFishInfo(string itemId)
        {
            var fish = ItemRegistry.Create(itemId);
            if (fish == null || fish.Category != Object.FishCategory)
                return null;
            List<string> output = new List<string>();
            var spawnData = Game1.locationData["Default"].Fish.FirstOrDefault(d => d.ItemId == itemId);
            if (spawnData != null)
            {
                output.Add(string.Format(SHelper.Translation.Get("fish-everywhere"), fish.DisplayName ?? itemId));
            }
            else
            {
                List<string> locations = new List<string>();
                foreach (var l in Game1.locations)
                {
                    if (l.GetData()?.Fish?.Exists(d => d.ItemId == fish.QualifiedItemId) == true)
                    {
                        locations.Add(l.DisplayName);
                    }
                }
                if (locations.Any())
                {
                    output.Add(string.Format(SHelper.Translation.Get("fish-location"), fish.DisplayName ?? itemId, string.Join(", ", locations)));
                }
            }
            if (!DataLoader.Fish(Game1.content).TryGetValue(fish.ItemId, out var fishDataString))
                return null;
            string[] fishData = fishDataString.Split('/');
            if (fishData.Length < 8 || fishData[7] == "both")
                return output;
            if (fishData[7] == "sunny")
            {
                output.Add(string.Format(SHelper.Translation.Get("fish-sunny"), fish.DisplayName ?? itemId));
            }
            else
            {
                output.Add(string.Format(SHelper.Translation.Get("fish-rainy"), fish.DisplayName ?? itemId));
            }
            return output;
        }
    }
}
