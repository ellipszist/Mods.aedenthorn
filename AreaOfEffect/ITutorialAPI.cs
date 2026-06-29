using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AreaOfEffect
{
    public interface ITutorialAPI
    {
        public void AddTutorialFrame(string key, string subtitle, string texture, int frames, int frameRate, float scale, Rectangle? startRect, string text);
        public void AddTutorialTrigger(string key, string tutorial, string category, List<string> categories);
        public void AddTutorial(string key, object indata);
    }
}