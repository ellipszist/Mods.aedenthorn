using System;

namespace StardewOpenWorld
{
    public interface IStardewOpenWorldAPI
    {
        public void RegisterBiome(string id, Func<ulong, int, int, WorldChunk> func);
    }
    public class StardewOpenWorldAPI : IStardewOpenWorldAPI
    {
        public void RegisterBiome(string id, Func<ulong, int, int, WorldChunk> func)
        {
            ModEntry.biomeCodeDict[id] = func;
        }
    }
}