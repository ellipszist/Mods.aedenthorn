using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SGJigsaw
{
    public  class PuzzleBoxInfo
    {
        public string mapPath;
        public int pieceSize;
        public float zoom;
        public Vector2 offset;
        public Point puzzleSize;
        public List<PuzzlePiece> pieces;
    }
}