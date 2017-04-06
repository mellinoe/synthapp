using ImGuiNET;
using System.Numerics;
using System.Diagnostics;
using System.Linq;
using Veldrid.Platform;
using Veldrid.Graphics;
using System;

namespace SynthApp.Widgets
{
    public class DrumPatternSequencer
    {
        public static readonly Vector2 ButtonSize = new Vector2(40, 60);
        public static readonly float Margin = 5f;

        public static bool DrawDrumSequencer(uint id, NoteSequence ns, uint steps, uint patternLength, bool repeat)
        {
            Debug.Assert(steps > 0);
            bool result = false;

            if (ImGui.BeginChildFrame(unchecked((uint)("DrumChild" + id).GetHashCode()), GetFrameSize(steps), WindowFlags.ShowBorders))
            {
                IO io = ImGui.GetIO();
                DrawList dl = DrawList.GetForCurrentWindow();
                Vector2 drawPos = ImGui.GetCursorScreenPos() + new Vector2(10, 10);
                for (uint i = 0; i < steps; i++)
                {
                    Vector4 color;
                    Vector4 baseColor = (i % 8 < 4) ? new Vector4(0.35f, 0.35f, 0.35f, 1f) : new Vector4(0.5f, 0.35f, 0.35f, 1f);
                    bool active = ns.Notes.Any(n => n.StartTime == PatternTime.Steps(i));
                    color = Vector4.Lerp(baseColor, new Vector4(1f, 1f, 1f, 1f), active ? 0.75f : 0f);
                    ImGui.PushID((int)i);

                    bool hovered = ImGui.IsMouseHoveringWindow() && ImGui.IsMouseHoveringRect(drawPos, drawPos + ButtonSize, true);
                    if (hovered)
                    {
                        color = Vector4.Lerp(color, RgbaFloat.White.ToVector4(), 0.6f);
                    }
                    dl.AddRectFilled(drawPos, drawPos + ButtonSize, Util.RgbaToArgb(color), 2f);
                    if (hovered)
                    {
                        if (Application.Instance.Input.GetMouseButton(MouseButton.Left))
                        {
                            if (!active)
                            {
                                AddNote(ns, i, steps, patternLength, repeat);
                                result = true;
                            }
                        }
                        else if (Application.Instance.Input.GetMouseButton(MouseButton.Right))
                        {
                            RemoveNote(ns, i, steps, patternLength, repeat);
                            result = true;
                        }
                    }
                    ImGui.PopID();
                    if (i != steps - 1)
                    {
                        ImGui.SameLine(0, Margin);
                    }

                    drawPos = drawPos + new Vector2(ButtonSize.X + Margin, 0);
                }
            }
            ImGui.EndChildFrame();

            return result;
        }

        private static void RemoveNote(NoteSequence ns, uint step, uint repeatLength, uint patternLength, bool repeat)
        {
            if (repeat && patternLength > repeatLength)
            {
                ns.Notes.RemoveAll(n => n.StartTime.Step % repeatLength == step);
            }
            else
            {
                Note note = ns.Notes.FirstOrDefault(n => n.StartTime.Step == step);
                if (note != null)
                {
                    ns.Notes.Remove(note);
                }
            }
        }

        private static void AddNote(NoteSequence ns, uint step, uint repeatLength, uint patternLength, bool repeat)
        {
            if (repeat && patternLength > repeatLength)
            {
                uint repeats = patternLength / repeatLength;
                for (uint i = 0; i < repeats; i++)
                {
                    Note newNote = new Note(PatternTime.Steps(step + (i * repeatLength)), PatternTime.Steps(1), Pitch.MiddleC);
                    ns.Notes.Add(newNote);
                }
            }
            else
            {
                Note newNote = new Note(PatternTime.Steps(step), PatternTime.Steps(1), Pitch.MiddleC);
                ns.Notes.Add(newNote);
            }
        }

        public static Vector2 GetFrameSize(uint steps)
        {
            return new Vector2((ButtonSize.X + Margin) * steps, ButtonSize.Y) + new Vector2(20, 20);
        }
    }
}
