using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LauncherDrawer
{
    public partial class ModEntry
    {
        public static bool ToggleMenu()
        {
            if (!Context.IsPlayerFree)
            {
                return false;
            }
            if (currentDrawerState.Value == DrawerState.Open)
            {
                ticks.Value = Config.DrawerSpeed;
                currentDrawerState.Value = DrawerState.Closing;
                Game1.playSound("bigDeSelect");
            }
            else if (currentDrawerState.Value == DrawerState.Closed)
            {
                ticks.Value = 0;
                currentDrawerState.Value = DrawerState.Opening;
                Game1.playSound("bigSelect");
            }
            else
            {
                return false;
            }
            return true;
        }
        public static Rectangle GetBounds(Vector2 position, int height, int count)
        {
            int xoff = (int)position.X + 48;
            int yoff = (int)position.Y + 172;
            int width = 220;
            return new(xoff, yoff + height * count, width, height);
        }
        public static void LaunchEntry(Dictionary<string, object> value)
        {
            try
            {
                if (value.TryGetValue("Keybind", out var obj))
                {
                    if(obj is IEnumerable list)
                    {
                        foreach (var s in list)
                        {
                            SHelper.Input.Press(Enum.Parse<SButton>(s.ToString()));
                        }
                    }
                }
                if (value.TryGetValue("Action", out var obj2))
                {
                    if(obj2 is Action a)
                        a.Invoke();
                    else if (obj2 is string s)
                    {
                        TriggerActionManager.TryRunAction(s, out _, out _);
                    }
                }
                else if (value.TryGetValue("Trigger", out var obj3) && obj2 is string s)
                {
                    TriggerActionManager.TryRunAction(s, out _, out _);
                }
            }
            catch { }
        }

        public static Vector2 GetPosition(Vector2 position)
        {
            return Config.CustomPosition ? new(Config.X < 0 ? position.X - 272 : Config.X, Config.Y < 0 ? position.Y - 168 : Config.Y) : position;
        }
    }
}