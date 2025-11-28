using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BuildingShift
{
    public class BuildingOffsetData
    {
        public Vector2 buildingOffset;
        public Dictionary<string, Vector2> layerOffsets = new Dictionary<string, Vector2>();
    }
}