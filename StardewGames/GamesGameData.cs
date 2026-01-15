using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewGames
{
    public class GamesGameData
    {
        public Action clickAction;
        public Action<SpriteBatch, Rectangle> drawAction;

        public GamesGameData(Action clickAction, Action<SpriteBatch, Rectangle> drawAction)
        {
            this.clickAction = clickAction;
            this.drawAction = drawAction;
        }
    }
}