using StardewValley.Characters;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Monsters;
using Microsoft.Xna.Framework;
using xTile.Dimensions;

namespace RoastingMarshmallows
{
    public partial class ModEntry
    {
        public static int GetRoastProgress(Farmer farmer, bool inc, bool start)
        {
            if (farmer.IsSitting())
            {
                var t = farmer.Tile;
                switch (farmer.FacingDirection)
                {
                    case 0:
                        t += new Vector2(0,-1);
                        break;
                    case 1:
                        t += new Vector2(1, 0);
                        break;
                    case 2:
                        t += new Vector2(0, 1);
                        break;
                    case 3:
                        t += new Vector2(-1, 0);
                        break;
                }

                if (farmer.currentLocation.Objects.TryGetValue(t, out var obj))
                {
                    if (obj is Torch && obj.QualifiedItemId == "(BC)146" && (obj as Torch).IsOn)
                    {
                        if (farmer.modData.TryGetValue(modKey, out var pstring))
                        {
                            var percent = int.Parse(pstring);
                            if (inc)
                            {
                                percent++;
                                if (percent >= Config.RoastFrames)
                                {
                                    SMonitor.Log("Burnt marshmallow", StardewModdingAPI.LogLevel.Debug);
                                    farmer.currentLocation.playSound("fireball");
                                    farmer.modData.Remove(modKey);
                                    return -1;
                                }
                                farmer.modData[modKey] = percent + "";
                            }
                            return percent;
                        }
                        else if (start)
                        {
                            farmer.currentLocation.playSound("grassyStep");
                            farmer.modData[modKey] = "1";
                            return 1;
                        }
                    }
                }
            }

            farmer.modData.Remove(modKey);
            return -1;
        }
    }
}