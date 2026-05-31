using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace FurnitureRecolor
{
    public partial class ModEntry
    {
        public static void MakeColorList(List<Color> list, string str)
        {
            list.Clear();
            var split = str.Split(';');
            for (int i = 0; i < split.Length; i++)
            {
                var c = split[i].Split(',');
                list.Add(new Color(byte.Parse(c[0]), byte.Parse(c[1]), byte.Parse(c[2])));
            }
        }
        public static void CheckTexture(SpriteBatch b, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, Furniture f)
        {
            if (Config.ModEnabled && f.modData.TryGetValue(colorsKey, out var str))
            {
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(f.QualifiedItemId);
                string textureName = itemData.TextureName;
                if (texture != itemData.GetTexture())
                {
                    if (texture == SHelper.GameContent.Load<Texture2D>(textureName + "Front"))
                    {
                        textureName += "Front";
                    }
                    else
                    {
                        b.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
                        return;
                    }
                }
                if (tileSheetDict.TryGetValue(textureName.Replace("\\", "/"), out var dict))
                {
                    List<Color> lista = parsedFurniture[f.ItemId];
                    List<Color> list = new();
                    MakeColorList(list, str);
                    if (list.Any())
                    {
                        for(int i = 0; i < lista.Count; i++) 
                        {
                            if (dict.TryGetValue(lista[i], out var t))
                            {
                                b.Draw(t, position, sourceRectangle, list[i], rotation, origin, scale, effects, layerDepth);
                            }
                        }
                        return;
                    }
                }
            }
            b.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
            return;
        }
    }
}