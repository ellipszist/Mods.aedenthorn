using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using xTile.Display;
using xTile.Tiles;

namespace IndoorOutdoor
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), [typeof(Texture2D), typeof(Vector2), typeof(Color)])]
        public static class SpriteBatch_Draw_Patch1
        {
            public static bool Prefix(Texture2D texture, Vector2 position)
            {
                if (!Config.ModEnabled || !currentLocationIndoorMapDict.Value.Any())
                    return true;
                var rect = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
                return CheckScreenRect(rect);
            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), [typeof(Texture2D), typeof(Rectangle), typeof(Color)])]
        public static class SpriteBatch_Draw_Patch2
        {
            public static bool Prefix(Texture2D texture, Rectangle destinationRectangle)
            {
                if (!Config.ModEnabled || !currentLocationIndoorMapDict.Value.Any())
                    return true;
                return CheckScreenRect(destinationRectangle);
            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), [typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color)])]
        public static class SpriteBatch_Draw_Patch3
        {
            public static bool Prefix(Texture2D texture, Vector2 position, Rectangle? sourceRectangle)
            {
                if (!Config.ModEnabled || !renderingWorld)
                    return true;
                var rect = new Rectangle((int)position.X, (int)position.Y, (sourceRectangle is null ? texture.Width : sourceRectangle.Value.Width), (sourceRectangle is null ? texture.Height : sourceRectangle.Value.Height));
                return CheckScreenRect(rect);
            }
        }
        
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), [typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color)])]
        public static class SpriteBatch_Draw_Patch4
        {
            public static bool Prefix(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle)
            {
                if (!Config.ModEnabled || !renderingWorld)
                    return true;
                var rect = new Rectangle(destinationRectangle.X, destinationRectangle.Y, (sourceRectangle is null ? texture.Width : sourceRectangle.Value.Width), (sourceRectangle is null ? texture.Height : sourceRectangle.Value.Height));
                return CheckScreenRect(rect);
            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), [typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(SpriteEffects), typeof(float)])]
        public static class SpriteBatch_Draw_Patch5
        {
            public static bool Prefix(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Vector2 origin)
            {
                if (!Config.ModEnabled || !renderingWorld)
                    return true;
                var rect = new Rectangle(destinationRectangle.X - (int)origin.X, destinationRectangle.Y - (int)origin.Y, (sourceRectangle is null ? texture.Width : sourceRectangle.Value.Width), (sourceRectangle is null ? texture.Height : sourceRectangle.Value.Height));
                return CheckScreenRect(rect);
            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), [typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float)])]
        public static class SpriteBatch_Draw_Patch6
        {
            public static bool Prefix(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Vector2 scale, Vector2 origin)
            {
                if (!Config.ModEnabled || !renderingWorld)
                    return true;
                var rect = new Rectangle((int)(position.X - origin.X * scale.X), (int)(position.Y - origin.Y * scale.X), (int)((sourceRectangle is null ? texture.Width : sourceRectangle.Value.Width) * scale.X), (int)((sourceRectangle is null ? texture.Height : sourceRectangle.Value.Height) * scale.Y));
                bool ret = CheckScreenRect(rect);
                if (!ret && Environment.StackTrace.Contains("FarmerRenderer"))
                {
                    var x = true;
                }
                return ret;
            }
        }

        [HarmonyPatch(typeof(XnaDisplayDevice), nameof(XnaDisplayDevice.DrawTile))]
        public static class XnaDisplayDevice_DrawTile_Patch
        {
            public static bool Prefix(Tile tile, xTile.Dimensions.Location location)
            {
                if (!Config.ModEnabled || !currentLocationIndoorMapDict.Value.Any())
                    return true;
                Rectangle r = new Rectangle(location.X, location.Y, 64, 64);
                return CheckScreenRect(r);
            }
        }
        //[HarmonyPatch(typeof(Tree), nameof(Tree.draw))]
        //public static class Tree_draw_Patch
        //{
        //    public static bool Prefix(Tree __instance)
        //    {
        //        if (!Config.ModEnabled || !currentLocationIndoorMapDict.Value.Any())
        //            return true;
        //        var bb = ToXTile(__instance.getBoundingBox());
        //        return CheckBoundingBox(bb);
        //    }
        //}
        //[HarmonyPatch(typeof(Bush), nameof(Bush.draw), new Type[] { typeof(SpriteBatch) })]
        //public static class Bush_draw_Patch
        //{
        //    public static bool Prefix(Bush __instance)
        //    {
        //        if (!Config.ModEnabled || !currentLocationIndoorMapDict.Value.Any())
        //            return true;
        //        var bb = ToXTile(__instance.getBoundingBox());
        //        return CheckBoundingBox(bb);
        //    }
        //}
    }
}
