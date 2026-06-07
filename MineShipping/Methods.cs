using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Audio;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Objects;

namespace MineShipping
{
    public partial class ModEntry
    {
        public static void shipItem(Item i, Farmer who)
        {
            if (i != null)
            {
                who.removeItemFromInventory(i);
                Farm farm = Game1.getFarm();
                if (farm != null)
                {
                    farm.getShippingBin(who).Add(i);
                }
                ShowShipment(i, false);
                farm.lastItemShipped = i;
                if (Game1.player.ActiveItem == null)
                {
                    Game1.player.showNotCarrying();
                    Game1.player.Halt();
                }
            }
        }
        public static void ShowShipment(Item item, bool playThrowSound = true)
        {
            if (Game1.player.currentLocation is not MineShaft shaft)
                return;

            if (playThrowSound)
            {
                shaft.localSound("backpackIN", null, null, SoundContext.Default);
            }
            DelayedAction.playSoundAfterDelay("Ship", playThrowSound ? 250 : 0, null, null, -1, false);
            int id = Game1.random.Next();
            
            Vector2 tile = shaft.tileBeneathLadder + new Vector2(1f, -1f);

            ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
            ColoredObject coloredObj = item as ColoredObject;
            Vector2 initialPosition = new Vector2((float)tile.X, (float)(tile.Y - 1)) * 64f + new Vector2((float)(7 + Game1.random.Next(6)), 2f) * 4f;
            foreach (bool isColorOverlay in new bool[]
            {
                false,
                true
            })
            {
                if (!isColorOverlay || (coloredObj != null && !coloredObj.ColorSameIndexAsParentSheetIndex))
                {
                    shaft.temporarySprites.Add(new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect(isColorOverlay ? 1 : 0, null), initialPosition, false, 0f, Color.White)
                    {
                        interval = 9999f,
                        scale = 4f,
                        alphaFade = 0.045f,
                        layerDepth = (float)((tile.Y + 1) * 64) / 10000f + 0.000225f,
                        motion = new Vector2(0f, 0.3f),
                        acceleration = new Vector2(0f, 0.2f),
                        scaleChange = -0.05f,
                        color = ((coloredObj != null) ? coloredObj.color.Value : Color.White)
                    });
                }
            }
        }
    }
}