using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace DoorKnock
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public KeybindList KnockButton { get; set; } = new KeybindList(SButton.K);
        public string KnockSound { get; set; } = "button_tap";
        public int KnockNumber { get; set; } = 3;
        public int KnockInterval { get; set; } = 200;
        public int AnswerDelay { get; set; } = 300;
        public int WaitTime { get; set; } = 300;
    }
}
