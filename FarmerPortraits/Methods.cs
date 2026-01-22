using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.IO;
using System.Linq;

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
                    texture = GetAssetTexture(key+i);
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
                texture = GetAssetTexture(key);
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

        public static Texture2D GetAssetTexture(string v)
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

        private void SetGlobalPortrait(string arg1, string[] arg2)
        {
            var path = string.Join(' ', arg2);
            SetTexture($"portrait.png", path);
        }
        private void SetThisPortrait(string arg1, string[] arg2)
        {
            var path = string.Join(' ', arg2);
            SetTexture($"portrait_{Game1.player.Name}.png", path);
        }
        private void SetGlobalBackground(string arg1, string[] arg2)
        {
            var path = string.Join(' ', arg2);
            SetTexture($"background.png", path);
        }
        private void SetThisBackground(string arg1, string[] arg2)
        {
            var path = string.Join(' ', arg2);
            SetTexture($"background_{Game1.player.Name}.png", path);
        }

        private void SetTexture(string output, string path)
        {
            if (!path.EndsWith(".png"))
            {
                SMonitor.Log($"File {path} doesn't have the .png extension.");
                return;
            }
            if (!File.Exists(path))
            {
                SMonitor.Log($"File {path} doesn't exist or can't be accessed");
                return;
            }
            var dest = Path.Combine(SHelper.DirectoryPath, output);
            if (File.Exists(dest))
            {
                int ext = 0;
                while (File.Exists($"{dest}.bkp{(ext == 0 ? "" : ext)}"))
                {
                    ext++;
                }
                File.Move(dest, $"{dest}.bkp{(ext == 0 ? "" : ext)}");
            }
            File.Copy(path, dest);
            SMonitor.Log($"Copied {path} to {dest}");
            ReloadTextures();
        }
    }
}