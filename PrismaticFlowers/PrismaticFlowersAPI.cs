using Microsoft.Xna.Framework;

namespace PrismaticFlowers
{
    public interface IPrismaticFlowersAPI
    {
        public Color GetPrismaticColor(object obj, Color fallback);
    }
    public class PrismaticFlowersAPI : IPrismaticFlowersAPI
    {
        Color IPrismaticFlowersAPI.GetPrismaticColor(object obj, Color fallback)
        {
            return ModEntry.GetPrismaticColor(fallback, obj);
        }
    }
}