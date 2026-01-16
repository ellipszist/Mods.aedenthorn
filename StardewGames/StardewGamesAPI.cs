using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using System;

namespace StardewGames
{
    public interface IStardewGamesAPI
    {
        public void AddGame(string name, Action clickAction, Action<SpriteBatch, Rectangle> drawAction);
        public void ReturnToMenu();
    }
    public class StardewGamesAPI: IStardewGamesAPI
    {
        public void AddGame(string name, Action clickAction, Action<SpriteBatch, Rectangle> drawAction)
        {
            ModEntry.gameDataDict[name] = new GamesGameData(clickAction, drawAction);
        }
        public void ReturnToMenu()
        {
            TitleMenu.subMenu = new GamesLoadGameMenu();
        }
    }
}