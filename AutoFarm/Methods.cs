using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using System.Collections.Generic;
using xTile.Tiles;

namespace AutoFarm
{
    public partial class ModEntry
    {
        public static bool TryGetAutoPlot(out AutoPlot plot, Vector2 tile)
        {
            plot = null;
            if (!Config.EnableMod || Game1.currentLocation == null)
                return false;
            if (!TryGetAutoPlots(out var list))
            {
                return false;
            }
            foreach (var p in list)
            {
                if (p.tiles.Contains(tile))
                {
                    plot = p;
                    return true;
                }
            }
            return false;
        }
        public static bool TryGetAutoPlots(out List<AutoPlot> list)
        {
            list = new List<AutoPlot>();
            if (!Config.EnableMod || Game1.currentLocation == null)
                return false;
            if (!locationDict.TryGetValue(Game1.currentLocation, out list))
            {
                if (!Game1.currentLocation.modData.TryGetValue(plotsKey, out var plotString))
                {
                    return false;
                }
                list = JsonConvert.DeserializeObject<List<AutoPlot>>(plotString); ;
                locationDict[Game1.currentLocation] = list;
            }
            return true;
        }
        public static void AddAutoPlot(Rectangle rect, Vector2 startTile)
        {
            HashSet<Vector2> tiles = new HashSet<Vector2>();
            for (int x = rect.X;  x < rect.Width; x++)
            {
                for (int y = rect.Y; y < rect.Height; y++)
                {
                        var tile = new Vector2(x, y);
                    if (!TryGetAutoPlot(out AutoPlot aPlot, tile))
                    {
                        tiles.Add(tile);
                    }
                }
            }
            bool exists = locationDict.TryGetValue(Game1.currentLocation, out var list);
            if (exists && TryGetAutoPlot(out AutoPlot plot, startTile))
            {
                plot.tiles.AddRange(tiles);
            }
            else
            {
                plot = new AutoPlot()
                {
                    tiles = tiles
                };
                if (!exists)
                {
                    if (Game1.currentLocation.modData.TryGetValue(plotsKey, out var plotString))
                    {
                        list = JsonConvert.DeserializeObject<List<AutoPlot>>(plotString);
                    }
                    else
                    {
                        list = new List<AutoPlot>();
                    }
                    locationDict[Game1.currentLocation] = list;
                }
                list.Add(plot);
            }
            Game1.currentLocation.modData[plotsKey] = JsonConvert.SerializeObject(list);
        }
    }
}