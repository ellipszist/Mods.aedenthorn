using System.Collections.Generic;

namespace Tutorials
{
    public interface ITutorialAddedFrames
    {
        public int Position { get; }
        public List<ITutorialFrame> Frames { get; }
    }
}