using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace MapTokens
{
    internal class MapPropertyTile : PropertyTile
    {
        /*********
        ** Fields
        *********/

        public MapPropertyTile(int _which = 0, string _s = " ") : base(_which, _s)
        {
        }

        public override bool AllowsInput()
        {
            return true;
        }

        public override bool UpdateContext()
        {
            var changed = ModEntry.mapPropertiesChanged;
            ModEntry.mapPropertiesChanged = false;
            return changed;
        }

        /// <summary>Get whether the token is available for use.</summary>
        public override bool IsReady()
        {
            return true;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public override IEnumerable<string> GetValues(string input)
        {
            string output = null;
            if (input != null)
            {
                string[] args = input.Split(':');
                if (args.Length == 2 && ModEntry.mapPropertyDict.TryGetValue(args[0].Trim(), out var dict) && dict != null && dict.TryGetValue(args[1], out var point))
                {
                    output = GetString(point);
                }
            }
            yield return output;
        }
    }
}