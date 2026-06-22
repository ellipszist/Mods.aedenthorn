using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Tutorials
{
    public class TutorialData
    {
        public string Title { get; set; }
        public List<TutorialFrame> Frames { get; set; } = new();
    }

    public class TutorialFrame
    {
        public string Subtitle { get; set; }
        public List<string> Textures { get; set; }
        public int FrameRate;
        public float Scale { get; set; } = 4;
        public Rectangle? SourceRect { get; set; }
        public string Text { get; set; }
    }
}