using System.Collections;
using System.Collections.Generic;

namespace Tutorials
{
    public interface ITutorialAPI
    {
        public void AddTutorial(string key, object indata);
    }
    public class TutorialAPI : ITutorialAPI
    {
        public void AddTutorial(string key, object indata)
        {
            var data = new TutorialData();
            foreach (var p in indata.GetType().GetProperties())
            {
                if (p.Name == "Frames")
                {
                    var inframes = (IEnumerable<object>)p.GetValue(indata);
                    foreach(var inframe in inframes)
                    {
                        var frame = new TutorialFrame();
                        foreach (var p2 in inframe.GetType().GetProperties())
                        {
                            foreach (var p3 in frame.GetType().GetProperties())
                            {
                                if (p2.Name == p3.Name)
                                {
                                    p3.SetValue(frame, p2.GetValue(inframe));
                                }
                            }
                        }
                        data.Frames.Add(frame);
                    }
                    continue;
                }
                foreach (var p2 in data.GetType().GetProperties())
                {
                    if(p2.Name == p.Name)
                    {
                        p2.SetValue(data, p.GetValue(indata));
                        break;
                    }
                }
            }
            ModEntry.TutorialDict[key] = data;
        }
    }
}