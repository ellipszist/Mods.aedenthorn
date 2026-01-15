using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SGJigsaw
{
    public interface ISGJigsawAPI
    {
        public void AddGame(string name, Action clickAction, Action<SpriteBatch, Rectangle> drawAction);
    }
}