using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SynthApp
{
    public class PianoRoll
    {
        private bool _opened = true;

        private float _stepWidth = 40f;
        private float _pitchHeight = 30f;
        private byte _topPitch = 50;
        private uint _leftmostStep = 0;

        private const float MinStepWidth = 5f;
        private const float MaxStepWidth = 120f;

        private List<Note> _notes = new List<Note>();
        private Note _dragging;
        private Vector2 _dragOffset;

        public PianoRoll()
        {
            _notes.Add(new Note(PatternTime.Steps(0), PatternTime.Steps(4), new Pitch(43)));
            _notes.Add(new Note(PatternTime.Steps(4), PatternTime.Steps(4), new Pitch(44)));
            _notes.Add(new Note(PatternTime.Steps(8), PatternTime.Steps(4), new Pitch(42)));
            _notes.Add(new Note(PatternTime.Steps(0), PatternTime.Steps(16), new Pitch(45)));
        }

        public unsafe void Draw()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 400), SetCondition.FirstUseEver);
            if (ImGui.BeginWindow("Piano Roll", ref _opened, 1.0f, WindowFlags.NoMove))
            {
                IO io = ImGui.GetIO();
                ImGui.Text(io.MousePosition.ToString());
                ImGui.Text(ImGui.GetWindowPosition().ToString());
                ImGui.Text("Drag offset: " + _dragOffset);
                DrawList dl = DrawList.GetForCurrentWindow();
                Vector2 windowPos = ImGui.GetCursorScreenPos();
                Vector2 windowSize = ImGui.GetWindowSize();
                int step = 0;
                for (float x = _stepWidth; x < windowSize.X; x += _stepWidth)
                {
                    float alpha = (step % 4) == 3 ? 0.65f : 0.15f;
                    float thickness = 1f;
                    if ((step % 16) == 15)
                    {
                        thickness = 4f;
                        alpha = 0.85f;
                    }
                    dl.AddLine(windowPos + new Vector2(x, 0), windowPos + new Vector2(x, windowSize.Y), Util.Argb(alpha, 1f, 1f, 1f), thickness);
                    step += 1;
                }
                for (float y = _pitchHeight; y < windowSize.Y; y += _pitchHeight)
                {
                    dl.AddLine(windowPos + new Vector2(0, y), windowPos + new Vector2(windowSize.X, y), Util.Argb(0.15f, 1f, 1f, 1f), 1.0f);
                }

                foreach (Note note in _notes)
                {
                    Vector2 pos = GetNotePosition(note);
                    Vector2 size = new Vector2(note.Duration.Step * _stepWidth, _pitchHeight);
                    dl.AddRectFilled(pos + windowPos, pos + windowPos + size, Util.Argb(0.75f, 1.0f, 0.2f, 0.2f), 5f);
                    if (ImGuiNative.igIsMouseHoveringRect(pos + windowPos, pos + windowPos + size, true))
                    {
                        if (io.MouseDown[0] && _dragging == null)
                        {
                            _dragging = note;
                            _dragOffset = io.MousePosition - windowPos - pos;
                            _dragOffset.Y = 0;
                        }
                    }
                }

                if (ImGui.IsMouseHoveringWindow() && ImGuiNative.igIsWindowFocused())
                {
                    if (io.CtrlPressed)
                    {
                        if (io.MouseWheel != 0f)
                        {
                            _stepWidth += 2f * io.MouseWheel;
                            _stepWidth = Util.Clamp(_stepWidth, MinStepWidth, MaxStepWidth);
                        }
                    }

                    if (_dragging != null)
                    {
                        if (!io.MouseDown[0])
                        {
                            _dragging = null;
                        }
                        else
                        {
                            byte newPitchValue = GetPitch(io.MousePosition - windowPos - _dragOffset);
                            if (!_dragging.Pitch.Equals(newPitchValue))
                            {
                                _dragging.Pitch = new Pitch(newPitchValue);
                                Console.WriteLine("Got a new pitch: " + newPitchValue);
                            }

                            uint newStep = GetStep(io.MousePosition - windowPos - _dragOffset);
                            if (_dragging.StartTime.Step != newStep)
                            {
                                _dragging.StartTime = PatternTime.Steps(newStep);
                            }
                        }
                    }
                }
            }
            ImGui.EndWindow();
        }

        private Vector2 GetNotePosition(Note note)
        {
            int stepDiff = (int)(note.StartTime.Step - _leftmostStep);
            float x = stepDiff * _stepWidth;

            int pitchDiff = _topPitch - note.Pitch.Value;
            float y = pitchDiff * _pitchHeight;

            return new Vector2(x, y);
        }

        private byte GetPitch(Vector2 pos)
        {
            uint notes = (uint)(pos.Y / _pitchHeight);
            return (byte)(_topPitch - notes);
        }

        private uint GetStep(Vector2 pos)
        {
            uint steps = (uint)(pos.X / _stepWidth);
            return _leftmostStep + steps;
        }
    }
}
