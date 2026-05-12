using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.FarmAnimals;
using StardewValley.Objects;
using System.Linq;
using Object = StardewValley.Object;

namespace AdvancedAutoGrabber
{
	public partial class ModEntry
    {

        public static void TriggerAutoGrabbers()
        {
            Utility.ForEachLocation(location =>
            {
                foreach (var kvp in location.Objects.Pairs)
                {
                    if (kvp.Value?.QualifiedItemId == "(BC)165")
                    {

                        TriggerAutoGrabber(kvp.Value);
                    }
                }
                return true;
            }, true, true);
        }

        public static void TriggerAutoGrabber(Object grabber)
        {
            Chest gchest = grabber.heldObject.Value as Chest;
            if (gchest != null)
            {
                foreach (var animal in grabber.Location.Animals.Values.Where(a => a.currentProduce.Value != null && a.isAdult() && a.GetHarvestType().GetValueOrDefault() != FarmAnimalHarvestType.DigUp && (Config.GrabRange < 0 || Vector2.Distance(a.Tile, grabber.TileLocation) <= Config.GrabRange)))
                {
                    Object produce = ItemRegistry.Create<Object>("(O)" + animal.currentProduce.Value, 1, 0, false);
                    produce.CanBeSetDown = false;
                    produce.Quality = animal.produceQuality.Value;
                    if (animal.hasEatenAnimalCracker.Value)
                    {
                        produce.Stack = 2;
                    }
                    if(TryGrab(grabber, gchest, produce))
                    {
                        animal.HandleStatsOnProduceCollected(produce, (uint)produce.Stack);
                        animal.currentProduce.Value = null;
                        animal.ReloadTextureIfNeeded(false);
                        grabber.showNextIndex.Value = true;
                        SMonitor.Log($"Auto-grabbed {produce.ItemId} at {grabber.TileLocation} from {animal.displayName} at {animal.Tile}");

                    }
                }
                if (Config.SendToChests)
                {
                    for(int i = gchest.Items.Count - 1; i >=0; i--)
                    {
                        var item = gchest.Items[i];
                        if (item is not Object obj)
                            continue;
                        foreach (var chest in grabber.Location.Objects.Values.OfType<Chest>().Where(c => Config.ChestRange < 0 || Vector2.Distance(c.TileLocation, grabber.TileLocation) <= Config.ChestRange))
                        {
                            if (chest.Items.ContainsId(obj.QualifiedItemId) && chest.addItem(obj) == null)
                            {
                                gchest.GetItemsForPlayer().Remove(obj);
                            }
                        }
                    }
                }
            }
        }

        public static bool TryGrab(Object grabber, Chest chest, Object produce)
        {
            return (!grabber.modData.TryGetValue(limitKey, out var itemId) || IsSameAnimal(produce, itemId)) && chest.addItem(produce) == null;
        }
        public static bool IsSameAnimal(Object obj, string itemId)
        {
            return obj.HasTypeObject() && Game1.farmAnimalData.Values.Any(d => (d.ProduceItemIds.Any(p => p.ItemId == obj.ItemId) || d.DeluxeProduceItemIds.Any(p => p.ItemId == obj.ItemId)) && (d.ProduceItemIds.Any(p => p.ItemId == itemId) || d.DeluxeProduceItemIds.Any(p => p.ItemId == itemId)));
        }

    }
}
