using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Tutorials
{
    public interface ITutorialData
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public List<string> Triggers { get; set; }
        public List<ITutorialFrame> Frames { get; set; }
    }

    public interface ITutorialFrame
    {
        public string Subtitle { get; set; }
        public string Texture { get; set; }
        public int Frames { get; set; }
        public int FrameRate { get; set; }
        public float Scale { get; set; }
        public Rectangle? StartRect { get; set; }
        public string Text { get; set; }
    }
    public class TutorialData : ITutorialData
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public List<string> Triggers { get; set; } = new();
        public List<ITutorialFrame> Frames { get; set; } = new();
    }

    public class TutorialFrame : ITutorialFrame
    {
        public string Subtitle { get; set; }
        public string Texture { get; set; }
        public int Frames { get; set; } = 1;
        public int FrameRate { get; set; }
        public float Scale { get; set; } = 4;
        public Rectangle? StartRect { get; set; }
        public string Text { get; set; }
    }

}