using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace AreaOfEffect
{
    public partial class ModEntry
    {

        public static List<Vector2> GetTiles(Vector2 tileLocation, int radius)
        {
            Vector2 start = tileLocation + new Vector2(-1, -1) * radius;
            Vector2 position = tileLocation;
            List<Vector2> list = new();
            var diameter = (radius) * 2;
            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    Vector2 tile = start + new Vector2(x, y);
                    var distance = (int)Math.Round(Vector2.Distance(position, tile));
                    if (distance <= radius)
                        list.Add(tile);
                }
            }
            return list;

        }

        public static Vector2 GetTargetTile(Farmer f, AOEEffectData t, int maxDistance)
        {
            var target = Game1.currentCursorTile;
            int d = maxDistance;
            float m = Vector2.Distance(target, f.Tile);
            if(m > d)
            {
                target = Vector2.Lerp(f.Tile, target, d / m);
                target = new Vector2((int)Math.Round(target.X), (int)Math.Round(target.Y));
            }
            return target;
        }

        public static int GetCurrentCharges(Tool tool, int max)
        {
            if (!tool.modData.TryGetValue(chargesKey, out var str))
            {
                SetCurrentCharges(tool, max);
                return max;
            }
            return int.Parse(str);
        }
        public static void SetCurrentCharges(Tool tool, int value)
        {
            tool.modData[chargesKey] = value.ToString();
        }

        public static void DestroyAt(GameLocation l, Vector2 tile, object o)
        {
            if (o is Object)
            {
                l.Objects.Remove(tile);
            }
            else if (o is TerrainFeature tf)
            {
                if (tf is Tree t)
                {
                    if (t.growthStage.Value >= 5)
                    {
                        t.health.Value = 0;
                        AccessTools.Method(typeof(Tree), "performTreeFall").Invoke(t, new object[] { null, 1, tile });
                    }
                    else
                    {
                        l.terrainFeatures.Remove(tile);
                    }
                }
                else if(tf is Grass g)
                {
                    g.reduceBy(4, true);
                }
            }
        }

        public static bool TryGetTool(Tool __instance, out AOEToolData data)
        {
            var key = __instance.ItemId;
            if(!ToolDict.TryGetValue(__instance.ItemId, out data))
            {
                data = null;
                return false;
            }
            return true;
        }
        public static bool TryGetEffect(Tool __instance, out AOEEffectData data)
        {
            var key = __instance.ItemId;
            if (__instance.modData.TryGetValue(effectKey, out key))
            {
            }
            else if(TryGetTool(__instance, out var tdata) && tdata.Type != null)
            {
                key = tdata.Type;
            }
            else
            {
                data = null;
                return false;
            }
            if (!EffectDict.TryGetValue(key, out data))
            {
                data = null;
                return false;
            }
            return true;
        }

        public static void SetEffect(Tool tool, string type)
        {
            Game1.playSound(Config.SetEffectSound);
            tool.modData[effectKey] = type;
        }
    }
}