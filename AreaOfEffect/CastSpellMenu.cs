using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AreaOfEffect
{
    public class CastSpellMenu : IClickableMenu
    {
        private Tool tool;
        private Vector2 lastPos;
        private List<Vector2> dots = new();
        private List<int> ends = new();
        private List<SpellDirection> directions = new();
        private SpellDirection currentDirection = SpellDirection.None;
        private Vector2 lastEnd;
        private Vector2 lastOnTrack;
        private int lastOnTrackDot;
        private bool spelling;

        public CastSpellMenu(Tool t)
        {
            tool = t;
            dots = new();
            ends = new();
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            if (directions.Any())
            {
                for (int i = 0; i < directions.Count; i++)
                {
                    //b.DrawString(Game1.dialogueFont, directions[i].ToString(), new Vector2(0, i * 64), Color.White);
                }
                var spell = GetSpell();
                if(spell != null)
                    b.DrawString(Game1.dialogueFont, spell, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height - 128), Color.White);
            }
            //b.DrawString(Game1.dialogueFont, currentDirection.ToString(), new Vector2(0, directions.Count * 64), Color.White);
            if (dots.Any())
            {
                int count = 0;
                for (int i = 0; i < dots.Count; i++)
                {
                    b.Draw(Game1.mouseCursors, dots[i] - new Vector2(60, 60), new Rectangle(88, 1779, 30, 30), GetColor(count), 0, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    if (ends.Count > count && ends[count] <= i)
                    {
                        count++;
                    }
                }
            }
            drawMouse(b);
        }

        private Color GetColor(int i)
        {
            var idx = directions.Count <= i ? (int)currentDirection :(int)directions[i];
            return idx switch
            {
                0 => Color.Red,
                1 => Color.Orange,
                2 => Color.Yellow,
                3 => Color.Lime,
                4 => Color.Cyan,
                5 => new Color(0, 100, 255),
                6 => new Color(152, 96, 255),
                7 => new Color(255, 100, 255),
                _ => Color.White,
            };
        }

        public override void update(GameTime time)
        {
            int minDot = 16;
            int minSegment = 256;
            if (ModEntry.SHelper.Input.IsDown(SButton.MouseLeft))
            {
                if (!spelling)
                {
                    spelling = true;
                    lastPos = Game1.getMousePosition().ToVector2();
                    dots.Clear();
                    ends.Clear();
                    directions.Clear();
                    dots.Add(lastPos);
                    lastEnd = lastPos;
                    lastOnTrackDot = 0;
                    currentDirection = SpellDirection.None;
                    return;
                }
            }
            else
            {
                if (spelling && directions.Any())
                {
                    if(currentDirection != directions.Last())
                    {
                        directions.Add(currentDirection);
                    }
                    spelling = false;
                    CheckSpell();
                }
                spelling = false;
                dots.Clear();
                ends.Clear();
                directions.Clear();
                lastOnTrackDot = 0;
                currentDirection = SpellDirection.None;
                return;
            }
            var newPos = Game1.getMousePosition().ToVector2();
            var distance = Vector2.Distance(lastPos, newPos);
            if (distance < minDot)
                return;
            lastPos = newPos;
            dots.Add(newPos);

            SpellDirection newDirection = GetDirection(newPos - dots[lastOnTrackDot]);
            if (newDirection == currentDirection)
            {
                lastOnTrackDot = dots.Count - 1;
                return;
            }
            distance = Vector2.Distance(dots[lastOnTrackDot], newPos);
            if (distance >= minSegment)
            {
                if(currentDirection != SpellDirection.None)
                {
                    Game1.playSound("cowboy_explosion");
                    directions.Add(currentDirection);
                    ends.Add(lastOnTrackDot);
                }
                lastEnd = dots[lastOnTrackDot];
                lastOnTrackDot = dots.Count - 1;
                currentDirection = newDirection;
            }
        }

        private SpellDirection GetDirection(Vector2 v)
        {
            var angle = Math.Atan2(v.X, -v.Y);
            if (angle < 0)
                angle += Math.PI * 2;
            if(angle < Math.PI / 8f)
            {
                return SpellDirection.Up;
            }
            if(angle < Math.PI * 3f / 8f)
            {
                return SpellDirection.UpRight;
            }
            if(angle < Math.PI * 5f / 8f)
            {
                return SpellDirection.Right;
            }
            if(angle < Math.PI * 7f / 8f)
            {
                return SpellDirection.DownRight;
            }
            if(angle < Math.PI * 9f / 8f)
            {
                return SpellDirection.Down;
            }
            if(angle < Math.PI * 11f / 8f)
            {
                return SpellDirection.DownLeft;
            }
            if(angle < Math.PI * 13f / 8f)
            {
                return SpellDirection.Left;
            }
            if(angle < Math.PI * 15f / 8f)
            {
                return SpellDirection.UpLeft;
            }
            return SpellDirection.Up;
        }

        private void CheckSpell()
        {
            if (!directions.Any())
                return;

            var spell = GetSpell();
            ModEntry.SetEffect(tool, spell);
            Game1.activeClickableMenu = null;
        }

        private string GetSpell()
        {
            var dir = directions.ToList();
            if (spelling)
            {
                dir.Add(currentDirection);
            }
            foreach (var kvp in ModEntry.SpellDict)
            {
                if (kvp.Value.Sequence.Count == dir.Count)
                {
                    for (int i = 0; i < dir.Count; i++)
                    {
                        if (kvp.Value.Sequence[i] != dir[i])
                        {
                            goto next;
                        }
                    }
                    return kvp.Value.Type;
                next:
                    continue;
                }
            }
            return null;
        }
    }
}