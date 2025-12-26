using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = StardewValley.Object;

namespace InventoryIndicators
{
	public partial class ModEntry : Mod
	{
        public static string loveText;
        public static bool seed;
        public static bool bundle;
        public static string hoverItem;

        public static IEnumerable<FieldInfo> GetFieldInfos()
		{
			return typeof(Object).GetFields().Where(f => f.FieldType == typeof(int) && f.IsLiteral && (f.Name.EndsWith("Category") || f.Name == "metalResources" || f.Name == "buildingResources" || f.Name == "sellAtPierresAndMarnies"));
        }
        public static string GetCategory(int category)
        {
            foreach (var f in GetFieldInfos())
            {
                if((int)f.GetValue(new Object()) == category)
                    return f.Name;
            }
            return null;
        }
        public static int GetCategory(string category)
        {
            foreach (var f in GetFieldInfos())
            {
                if(f.Name == category)
                    return (int)f.GetValue(new Object());
            }
            return 0;
        }
        public static void DrawBefore(Item __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
        {

            if (!Config.ModEnabled)
                return;
            var cat = GetCategory(__instance.Category);
            if (cat != null && Config.Colors.TryGetValue(cat, out var color) && color.A > 0)
            {
                if (Config.OutlineWidth < 1)
                {
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X + 4, (int)location.Y + 4, 56, 56), null, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                }
                else
                {
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X + 4, (int)location.Y + 4, 56, Config.OutlineWidth), null, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X + 4, (int)location.Y + 4 + Config.OutlineWidth, Config.OutlineWidth, 56 - Config.OutlineWidth), null, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X + 60 - Config.OutlineWidth, (int)location.Y + 4 + Config.OutlineWidth, Config.OutlineWidth, 56 - Config.OutlineWidth), null, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X + 4 + Config.OutlineWidth, (int)location.Y + 60 - Config.OutlineWidth, 56 - Config.OutlineWidth * 2, Config.OutlineWidth), null, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                }
            }
        }
        public static void DrawAfter(Item __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
        {
            if (!Config.ModEnabled)
                return;
            bool hover = new Rectangle(location.ToPoint(), new Point(64, 64)).Contains(Game1.getMousePosition());

            if (hover)
            {
                seed = false;
                bundle = false;
            }
            bool foundLove = false;
            bool setText = false;

            int offset = -2;

            string[] universalLoves = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Love"]);
            foreach(var id in universalLoves)
            {
                if(id == __instance.ItemId)
                {
                    spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(offset, offset), new Rectangle(172, 514, 9, 10), Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, layerDepth);
                    if(hover && hoverItem != __instance.ItemId)
                    {
                        hoverItem = __instance.ItemId;
                        loveText = SHelper.Translation.Get("universal_love");
                    }
                    setText = true;
                    foundLove = true;
                    break;
                }
            }
            if (!foundLove && favoriteThings.TryGetValue(__instance.ItemId, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var npc = list[i];
                    if (Config.ShowUngiftedFavorites || (Game1.player.giftedItems.TryGetValue(npc, out var giftData) && giftData.TryGetValue(__instance.ItemId, out var value) && value > 0))
                    {
                        var portrait = Game1.getCharacterFromName(npc)?.Portrait;
                        if (portrait == null)
                            continue;
                        spriteBatch.Draw(portrait, location + new Vector2(i * 2 + offset, offset), new Rectangle(0, 0, 64, 64), Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, layerDepth);
                        if (hover && hoverItem != __instance.ItemId)
                        {
                            if (!foundLove)
                                loveText = "";
                            loveText = (foundLove ? loveText + ", " : "") + npc;
                        }
                        foundLove = true;
                        setText = true;
                    }
                }

                if (foundLove && hover && hoverItem != __instance.ItemId)
                {
                    loveText = string.Format(SHelper.Translation.Get("x_love"), loveText);
                    hoverItem = __instance.ItemId;
                }
            }
            if(Config.ShowBundleItems && __instance is Object && Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).couldThisIngredienteBeUsedInABundle(__instance as Object))
            {
                spriteBatch.Draw(SHelper.GameContent.Load<Texture2D>("Characters/Junimo"), location + new Vector2(32 - offset, offset), new Rectangle(0, 1, 16, 15), Config.JunimoColor, 0, Vector2.Zero, 2f, SpriteEffects.None, layerDepth);
                if (hover)
                {
                    bundle = true;
                    setText = true;
                }
            }
            if (Config.ShowPlantableSeeds && __instance is Object && __instance.Category == Object.SeedsCategory)
            {
                if(__instance.Name.Contains("Mixed") || Crop.TryGetData(Crop.ResolveSeedId(__instance.ItemId, Game1.currentLocation), out var cropData) && cropData.Seasons.Contains(Game1.currentLocation.GetSeason()))
                {
                    spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(offset, 32 - offset), new Rectangle(18, 625, 13, 15), new Color(1f,1f,1f,Config.PlantableOpacity), 0, Vector2.Zero, 2f, SpriteEffects.None, layerDepth);

                    if (hover)
                    {
                        seed = true;
                        setText = true;
                    }
                }
            }
            if (!setText && hover)
            {
                hoverItem = null;
                loveText = null;
                seed = false;
                bundle = false;
            }
        }
    }
}
