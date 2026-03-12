using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using System.Linq;

namespace Gravedigger
{
	public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkForBuriedItem))]
        public static class GameLocation_checkForBuriedItem_Patch
        {
            public static bool Prefix(GameLocation __instance, int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
            {
                if (__instance is not Town || !graveTiles.Contains(new Point(xLocation, yLocation)) || Game1.random.NextDouble() < Config.VanillaChance / 100f)
                    return true;
                if (Game1.random.NextDouble() < Config.ArtifactChance / 100f)
                {
                    __instance.digUpArtifactSpot(xLocation, yLocation, who);
                }
                else
                {
                    var bones = Game1.objectData.Where(p => p.Value.ContextTags is not null && p.Value.ContextTags.Contains("bone_item") && !Config.NotBones.Contains(p.Key)).Select(p => p.Key);
                    if (bones.Any())
                    {
                        Game1.createObjectDebris(Game1.random.Choose(bones.ToArray()), xLocation, yLocation, -1, 0, 1f, null);
                    }
                }
                return false;
            }
        }
    }
}
