using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace FarmerPortraits
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {
        private static void ReloadTextures()
        {


            portraitTextures.Value.Clear();
            foreach(var key in new string[] { "background", "portrait" })
            {
                Texture2D texture = null;
                int i = 0;
                for ( ; ; )
                {
                    texture = GetTexture(key+i);
                    if (texture != null)
                    {
                        portraitTextures.Value[key + i] = texture;
                    }
                    else if(i > 5)
                    {
                        break;
                    }
                    i++;
                }
                texture = GetTexture(key);
                if (texture != null)
                {
                    portraitTextures.Value[key] = texture;
                }
            }
            
        }

        public static Texture2D GetCachedTexture(string v, int which)
        {
            if(which > -1 && portraitTextures.Value.TryGetValue(v + which, out var tex))
            {
                return tex;
            }
            if(portraitTextures.Value.TryGetValue(v, out tex))
            {
                return tex;
            }
            return null;
        }

        public static Texture2D GetTexture(string v)
        {
            if (SHelper.ModContent.DoesAssetExist<Texture2D>($"{v}_{Game1.player.Name}.png"))
            {
                return SHelper.ModContent.Load<Texture2D>($"{v}_{Game1.player.Name}.png");
            }
            if (SHelper.ModContent.DoesAssetExist<Texture2D>($"{v}.png"))
            {
                return SHelper.ModContent.Load<Texture2D>($"{v}.png");
            }

            if (SHelper.GameContent.DoesAssetExist<Texture2D>(SHelper.GameContent.ParseAssetName($"aedenthorn.FarmerPortraits/{v}_{Game1.player.Name}")))
            {
                return SHelper.GameContent.Load<Texture2D>($"aedenthorn.FarmerPortraits/{v}_{Game1.player.Name}");
            }
            if (SHelper.GameContent.DoesAssetExist<Texture2D>(SHelper.GameContent.ParseAssetName($"aedenthorn.FarmerPortraits/{v}")))
            {
                return SHelper.GameContent.Load<Texture2D>($"aedenthorn.FarmerPortraits/{v}");
            }
            return null;
        }

    }
}