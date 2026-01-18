using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewGames
{
    public class GamesGameData
    {
        public Action<int, int> clickAction;
        public Action<SpriteBatch, Rectangle> drawAction;

        public GamesGameData(Action<int, int> clickAction, Action<SpriteBatch, Rectangle> drawAction)
        {
            this.clickAction = clickAction;
            this.drawAction = drawAction;
        }
    }
}