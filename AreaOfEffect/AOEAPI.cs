using Microsoft.Xna.Framework;
using StardewValley;

namespace AreaOfEffect
{
    public interface IAOEAPI
    {
        
    }
    public class AOEAPI : IAOEAPI
    {
        public void ApplyAOEEffect(GameLocation l, Farmer who, Vector2 center, string type)
        {
            if(!ModEntry.ToolDict.TryGetValue(type, out var data))
            {
                ModEntry.SMonitor.Log($"AOE Effect for {type} not found!", StardewModdingAPI.LogLevel.Warn);
                return;
            }
            ModEntry.ApplyAOEEffect(l, who, center, data);
        }
    }
}