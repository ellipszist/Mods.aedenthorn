using StardewValley.Characters;
using StardewValley;

namespace PetClothes
{
    public partial class ModEntry
    {
        private static bool IsPetClothes(Pet pet, Item item, out string texture)
        {
            if (!ClothesDict.TryGetValue(item.QualifiedItemId, out var data) || !data.TryGetValue(pet.petType.Value + pet.whichBreed.Value, out texture))
            {
                texture = null;
                return false;
            }
            return true;
        }
    }
}