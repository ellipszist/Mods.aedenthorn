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
