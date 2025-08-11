﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;
using xTile;

namespace StardewOpenWorld
{
    public class Biome
    {
        public string MapPath;
        public Point MapPosition;
        public Dictionary<Vector2, ObjData> Objects = new();
    }
    public class ObjData
    {
        public string type;
        public Dictionary<string, object> fields;
        public Dictionary<string, object> properties;
    }
}