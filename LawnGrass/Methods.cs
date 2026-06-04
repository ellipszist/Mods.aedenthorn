using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace LawnGrass
{
    public partial class ModEntry
    {
        public static Rectangle? SetSourceRect(Rectangle? sourceRect, Grass grass)
        {
            if (!Config.ModEnabled || grass.numberOfWeeds.Value == 4)
                return sourceRect;
            var height = (int)Math.Round(sourceRect.Value.Height * (grass.numberOfWeeds.Value / 4f));
            return new Rectangle(sourceRect.Value.X, sourceRect.Value.Y, sourceRect.Value.Width, height);
        }
        public static Vector2 SetPos(Vector2 pos, Grass grass)
        {
            if (!Config.ModEnabled || grass.numberOfWeeds.Value == 4)
                return pos;
            pos.Y += 20 * (4 - grass.numberOfWeeds.Value);
            return pos;
        }
        public static string GetWhichSeason(GameLocation location)
        {
            string season = location.GetSeasonKey();
            if(!location.IsOutdoors && season == "winter")
            {
                season = "spring";
            }
            return season;
        }
        public static void OnAdded(Grass grass, GameLocation loc, Vector2 tilePos)
        {
            grass.Location = loc;
            grass.Tile = tilePos;
            UpdateNeighbors(grass);
        }

        public static void OnRemoved(Grass grass, GameLocation location)
        {
            if (grass.Location == null)
            {
                grass.Location = location;
            }
            List<Neighbor> list = GatherNeighbors(grass);
            grass.modData[maskKey] = "0";
            foreach (Neighbor i in list)
            {
                OnNeighborRemoved(i.feature, i.invDirection);
                UpdateDrawSums(i.feature);
            }
            UpdateDrawSums(grass);
        }

        public static void UpdateNeighbors(Grass grass)
        {
            List<Neighbor> list = GatherNeighbors(grass);
            byte neighborMask = 0;
            foreach (Neighbor i in list)
            {
                neighborMask |= i.direction;
                OnNeighborAdded(i.feature, i.invDirection);
                UpdateDrawSums(i.feature);
            }
            grass.modData[maskKey] = neighborMask.ToString();
            UpdateDrawSums(grass);
        }
        public static void OnNeighborAdded(Grass grass, byte direction)
        {
            if (!grass.modData.TryGetValue(maskKey, out var str) || !byte.TryParse(str, out byte neighborMask))
                neighborMask = 0;
            neighborMask |= direction;
            grass.modData[maskKey] = neighborMask.ToString();
        }
        public static void OnNeighborRemoved(Grass grass, byte direction)
        {
            if (!grass.modData.TryGetValue(maskKey, out var str) || !byte.TryParse(str, out byte neighborMask))
                neighborMask = 0;
            neighborMask &= (byte)~direction;
            grass.modData[maskKey] = neighborMask.ToString();

        }
        private static List<Neighbor> GatherNeighbors(Grass grass)
        {
            List<Neighbor> results = new();
            GameLocation location = grass.Location;
            Vector2 tilePos = grass.Tile;
            NetVector2Dictionary<TerrainFeature,NetRef<TerrainFeature>> terrainFeatures = location.terrainFeatures;
            foreach (NeighborLoc item in _offsets)
            {
                Vector2 tile = tilePos + item.Offset;
                TerrainFeature feature;
                if (terrainFeatures.TryGetValue(tile, out feature) && feature is Grass g)
                {
                    Neighbor i = new(g, item.Direction, item.InvDirection);
                    results.Add(i);
                }
            }
            return results;
        }
        public static void UpdateDrawSums(Grass grass)
        {
            if (!grass.modData.TryGetValue(maskKey, out var str) || !byte.TryParse(str, out byte neighborMask))
                neighborMask = 0;
            byte drawSum = (byte)(neighborMask & 15);
            if (drawGuide is null)
                PopulateDrawGuide();
            grass.modData[posKey] = drawGuide[drawSum].ToString();
        }
        private struct NeighborLoc
        {
            public NeighborLoc(Vector2 a, byte b, byte c)
            {
                Offset = a;
                Direction = b;
                InvDirection = c;
            }

            public readonly Vector2 Offset;

            public readonly byte Direction;

            public readonly byte InvDirection;
        }
        private struct Neighbor
        {
            public Neighbor(Grass a, byte b, byte c)
            {
                feature = a;
                direction = b;
                invDirection = c;
            }

            public readonly Grass feature;

            public readonly byte direction;

            public readonly byte invDirection;
        }
        private static readonly NeighborLoc[] _offsets = new NeighborLoc[]
        {
            new (HoeDirt.N_Offset, 1, 4),
            new (HoeDirt.S_Offset, 4, 1),
            new (HoeDirt.E_Offset, 2, 8),
            new (HoeDirt.W_Offset, 8, 2)
        };
        public static Dictionary<byte, int> drawGuide;
        public static void PopulateDrawGuide()
        {
            Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
            dictionary[0] = 0;
            dictionary[8] = 15;
            dictionary[2] = 13;
            dictionary[1] = 12;
            dictionary[4] = 4;
            dictionary[9] = 11;
            dictionary[3] = 9;
            dictionary[5] = 8;
            dictionary[6] = 1;
            dictionary[12] = 3;
            dictionary[10] = 14;
            dictionary[7] = 5;
            dictionary[15] = 6;
            dictionary[13] = 7;
            dictionary[11] = 10;
            dictionary[14] = 2;
            drawGuide = dictionary;
        }
    }
}