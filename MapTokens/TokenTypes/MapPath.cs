using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MapTokens
{
    public class MapPath
    {


        /*********
        ** Public methods
        *********/
        /****
        ** Metadata
        ****/
        /// <summary>Get whether the token allows input arguments (e.g. an NPC name for a relationship token).</summary>
        public virtual bool AllowsInput()
        {
            return true;
        }

        /// <summary>Whether the token may return multiple values for the given input.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public virtual bool CanHaveMultipleValues(string input = null)
        {
            return false;
        }

        /****
        ** State
        ****/
        /// <summary>Update the values when the context changes.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        public virtual bool UpdateContext()
        {
            bool changed = false;
            foreach(var l in Game1.locations)
            {
                if(!ModEntry.mapPathDict.TryGetValue(l.NameOrUniqueName, out var path) || path != l.mapPath.Value)
                {
                    ModEntry.mapPathDict[l.NameOrUniqueName] = l.mapPath.Value; 
                    changed = true;
                }
            }
            return changed;
        }


        /// <summary>Get whether the token is available for use.</summary>
        public virtual bool IsReady()
        {
            return true;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public virtual IEnumerable<string> GetValues(string input)
        {
            string mapPath;
            ModEntry.mapPathDict.TryGetValue(input, out mapPath);
            yield return mapPath;
        }
    }
}