using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;

namespace PettingAnimation
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(FarmerRenderer), nameof(FarmerRenderer.draw), new Type[] { typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer) })]
        public static class FarmerRenderer_draw_Patch
        {
            public static void Prefix(FarmerRenderer __instance, ref Vector2 position, Farmer who)
            {
                if (!Config.ModEnabled || !who.modData.TryGetValue(facingKey, out var str))
                    return;
                position += GetOffset(str);
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.GetShadowOffset))]
        public static class Character_GetShadowOffset_Patch
        {
            public static bool Prefix(Character __instance, ref Vector2 __result)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(facingKey, out var str))
                    return true;
                __result = GetOffset(str);
                return false;
            }
        }
        [HarmonyPatch(typeof(Pet), nameof(Pet.checkAction))]
        public static class Pet_checkAction_Patch
        {
            public static void Prefix(Pet __instance, Farmer who, ref bool __state)
            {
                if (Config.ModEnabled && Config.AlwaysPet && who.CurrentItem is not Hat && who.CurrentItem?.QualifiedItemId != "(O)ButterflyPowder")
                    __state = true;
            }
            public static void Postfix(Pet __instance, Farmer who, ref bool __result, bool __state)
            {
                if (!Config.ModEnabled)
                    return;
                if(__result || __state)
                {
                    PetPet(who);
                }
            }
        }
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.pet))]
        public static class FarmAnimal_pet_Patch
        {
            public static void Prefix(FarmAnimal __instance, Farmer who, bool is_auto_pet)
            {
                if (Config.ModEnabled && !is_auto_pet && who.CurrentItem?.QualifiedItemId != "(O)GoldenAnimalCracker" && (Config.AlwaysPet || !__instance.wasPet.Value))
                    PetPet(who);
            }
        }
    }
}