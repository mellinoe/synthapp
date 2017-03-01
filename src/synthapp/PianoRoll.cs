using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Graphics;

namespace SynthApp
{
    public class PianoRoll
    {
        private readonly LiveNotePlayer _livePlayer;
        private bool _opened = true;
        private float _stepWidth = 40f;
        private float _pitchHeight = 20f;
        private byte _topPitch = new Pitch(PitchClass.B, 10).Value;
        private uint _leftmostStep = 0;
        private float _pianoKeyWidth = 60f;
        private Vector2 _viewOffset;

        private const float MinStepWidth = 5f;
        private const float MaxStepWidth = 120f;

        private PatternTime _totalDuration = PatternTime.Steps(16);
        private Channel _selectedChannel;
        private List<Note> _notes = new List<Note>();
        private HashSet<Note> _noteRemovals = new HashSet<Note>();
        private Note _interacting;
        private bool _dragging;
        private bool _resizing;
        private Vector2 _dragOffset;

        private bool _dragScrolling;
        private Vector2 _dragScrollPos;
        private PatternTime _newNoteDuration = PatternTime.Steps(2);

        public void SetSelectedChannel(Channel channel)
        {
            _selectedChannel = channel;
        }

        public void SetNotes(List<Note> notes, PatternTime totalDuration)
        {
            _notes = notes;
            _totalDuration = totalDuration;
        }

        public PianoRoll(LiveNotePlayer livePlayer)
        {
            _livePlayer = livePlayer;

            _notes.Add(new Note(PatternTime.Steps(0), PatternTime.Steps(4), new Pitch(43)));
            _notes.Add(new Note(PatternTime.Steps(4), PatternTime.Steps(4), new Pitch(44)));
            _notes.Add(new Note(PatternTime.Steps(8), PatternTime.Steps(4), new Pitch(42)));
            _notes.Add(new Note(PatternTime.Steps(0), PatternTime.Steps(16), new Pitch(45)));

            byte c5Value = new Pitch(PitchClass.C, 5).Value;
            uint diff = (uint)(_topPitch - c5Value);
            _viewOffset = new Vector2(0, -diff * _pitchHeight);
        }

        public unsafe void Draw()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 400), SetCondition.FirstUseEver);
            if (ImGui.BeginWindow("Piano Roll", ref _opened, 1.0f, WindowFlags.NoScrollWithMouse | WindowFlags.NoScrollbar | WindowFlags.MenuBar))
            {
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.MenuItem("Clear"))
                    {
                        _notes.Clear();
                    }
                    ImGui.EndMenuBar();
                }
                IO io = ImGui.GetIO();
                DrawList dl = DrawList.GetForCurrentWindow();
                Vector2 windowPos = ImGui.GetCursorScreenPos();
                Vector2 gridPos = windowPos + new Vector2(_pianoKeyWidth, 0);
                Vector2 gridSize = ImGui.GetWindowSize() - new Vector2(_pianoKeyWidth, 0f);
                ImGui.BeginChild("NoMoveChild", false, WindowFlags.NoMove | WindowFlags.NoScrollbar);
                Vector2 totalSize = new Vector2(_totalDuration.Step * _stepWidth, (_topPitch + 1) * _pitchHeight);
                DrawPianoKeys(dl, windowPos, totalSize);

                uint step = 0;
                for (float x = _stepWidth; x <= totalSize.X; x += _stepWidth)
                {
                    float alpha = (step % 4) == 3 ? 0.65f : 0.15f;
                    float thickness = 1f;
                    if ((step % 16) == 15)
                    {
                        thickness = 4f;
                        alpha = 0.85f;
                    }
                    dl.AddLine(
                        gridPos + new Vector2(x, 0) + _viewOffset,
                        gridPos + new Vector2(x, totalSize.Y) + _viewOffset,
                        Util.Argb(alpha, 1f, 1f, 1f),
                        thickness);
                    step += 1;
                }

                byte pitch = _topPitch;
                for (float y = 0; y < totalSize.Y; y += _pitchHeight)
                {
                    float alpha = 0.15f;
                    Pitch p = new Pitch(pitch);
                    if (p.PitchClass == PitchClass.B)
                    {
                        alpha = 0.85f;
                    }
                    dl.AddLine(
                        gridPos + new Vector2(0, y) + _viewOffset,
                        gridPos + new Vector2(gridSize.X, y) + _viewOffset,
                        Util.Argb(alpha, 1f, 1f, 1f),
                        1.0f);
                    if (pitch != 0)
                    {
                        pitch -= 1;
                    }
                }

                foreach (Note note in _notes)
                {
                    Vector2 pos = GetNoteStartPosition(note) + _viewOffset;
                    Vector2 size = new Vector2(note.Duration.Step * _stepWidth, _pitchHeight);
                    bool hovering = ImGui.IsMouseHoveringRect(pos + gridPos, pos + gridPos + size, true);
                    uint color = hovering ? Util.Argb(0.95f, 1.0f, 0.4f, 0.4f) : Util.Argb(0.75f, 1.0f, 0.2f, 0.2f);
                    dl.AddRectFilled(pos + gridPos, pos + gridPos + size, color, 5f);
                    ImGui.SetCursorScreenPos(pos + gridPos);
                    ImGui.Text(note.Pitch.ToString());
                    if (hovering)
                    {
                        if (io.MouseDown[0] && !_resizing && !_dragging)
                        {
                            _interacting = note;
                            _dragging = true;
                            _dragOffset = io.MousePosition + _viewOffset - gridPos - pos;
                            _dragOffset.Y = 0;
                            _newNoteDuration = note.Duration;
                        }
                        else if (io.MouseDown[1] && !_resizing && !_dragging)
                        {
                            RemoveNote(note);
                        }
                    }
                    dl.AddRectFilled(
                        pos + gridPos + new Vector2(size.X, 0),
                        pos + gridPos + new Vector2(size.X + 10f, size.Y),
                        Util.Argb(1f, 1f, 1f, 1f),
                        0f);
                    if (ImGui.IsMouseHoveringRect(
                        pos + gridPos + new Vector2(size.X, 0),
                        pos + gridPos + new Vector2(size.X + 10f, size.Y),
                        true))
                    {
                        if (io.MouseDown[0] && !_resizing && !_dragging)
                        {
                            _interacting = note;
                            _resizing = true;
                            _newNoteDuration = note.Duration;
                        }
                    }
                }

                _notes.RemoveAll(_noteRemovals.Contains);
                _noteRemovals.Clear();

                if (ImGui.IsMouseHoveringWindow() /* && ImGuiNative.igIsWindowFocused() */ )
                {
                    if (io.MouseWheel != 0f)
                    {
                        if (io.CtrlPressed)
                        {
                            _stepWidth += 2f * io.MouseWheel;
                            _stepWidth = Util.Clamp(_stepWidth, MinStepWidth, MaxStepWidth);
                        }
                        else
                        {
                            _viewOffset += Vector2.UnitY * io.MouseWheel * 10;
                            ClampViewPos();
                        }
                    }

                    if (_dragging)
                    {
                        if (!io.MouseDown[0])
                        {
                            _dragging = false;
                            _interacting = null;
                        }
                        else
                        {
                            byte newPitchValue = GetPitchValue(io.MousePosition - _viewOffset - gridPos - _dragOffset);
                            if (!_interacting.Pitch.Equals(newPitchValue))
                            {
                                _interacting.Pitch = new Pitch(newPitchValue);
                            }

                            uint newStep = GetStep(io.MousePosition - _viewOffset - gridPos - _dragOffset);
                            if (_interacting.StartTime.Step != newStep)
                            {
                                _interacting.StartTime = PatternTime.Steps(newStep);
                            }
                        }
                    }
                    else if (_resizing)
                    {
                        if (!io.MouseDown[0])
                        {
                            _resizing = false;
                            _interacting = null;
                        }
                        else
                        {
                            Vector2 start = GetNoteStartPosition(_interacting);
                            Vector2 end = io.MousePosition - gridPos;
                            float xDiff = end.X - start.X;
                            if (xDiff > 0)
                            {
                                uint newSteps = (uint)(xDiff / _stepWidth);
                                newSteps = Math.Max(1u, newSteps);
                                if (_interacting.Duration.Step != newSteps)
                                {
                                    _interacting.Duration = PatternTime.Steps(newSteps);
                                }
                            }
                        }
                    }
                    else if (_dragScrolling)
                    {
                        if (!io.MouseDown[2])
                        {
                            _dragScrolling = false;
                        }
                        else
                        {
                            Vector2 newDragScrollPos = io.MousePosition;
                            Vector2 delta = newDragScrollPos - _dragScrollPos;
                            _viewOffset += delta;
                            _dragScrollPos = newDragScrollPos;
                            ClampViewPos();
                        }
                    }
                    else
                    {
                        if (io.MouseDown[2]) // Middle Mouse
                        {
                            _dragScrolling = true;
                            _dragScrollPos = io.MousePosition;
                        }
                        if (io.MouseDown[0]) // Left Mouse
                        {
                            AddNote(io.MousePosition - _viewOffset - gridPos);
                        }
                    }
                }
                ImGui.EndChild();
            }
            ImGui.EndWindow();
        }

        private void RemoveNote(Note note)
        {
            _noteRemovals.Add(note); ;
        }

        private void AddNote(Vector2 gridPos)
        {
            Pitch pitch = new Pitch(GetPitchValue(gridPos));
            uint step = GetStep(gridPos);
            Note n = new Note(PatternTime.Steps(step), _newNoteDuration, pitch);
            _interacting = n;
            _dragging = true;
            _dragOffset = gridPos - GetNoteStartPosition(n);
            _dragOffset.Y = 0;
            _notes.Add(n);
        }

        private unsafe void ClampViewPos()
        {
            _viewOffset = Vector2.Min(Vector2.Zero, _viewOffset);
        }

        private void DrawPianoKeys(DrawList dl, Vector2 windowPos, Vector2 totalSize)
        {
            byte pitch = _topPitch;
            for (float y = 0; y < totalSize.Y; y += _pitchHeight)
            {
                uint leftCol = GetColor(pitch, true);
                uint rightCol = GetColor(pitch, false);
                dl.AddRectFilledMultiColor(
                    windowPos + new Vector2(0, y) + _viewOffset,
                    windowPos + new Vector2(0, y) + _viewOffset + new Vector2(_pianoKeyWidth, _pitchHeight),
                    leftCol,
                    rightCol,
                    rightCol,
                    leftCol);
                dl.AddLine(
                    windowPos + new Vector2(0, y + _pitchHeight) + _viewOffset,
                    windowPos + new Vector2(0, y + _pitchHeight) + _viewOffset + new Vector2(_pianoKeyWidth, _pitchHeight),
                    Util.Argb(1f, 0.6f, 0.6f, 0.6f),
                    2f);
                ImGui.SetCursorScreenPos(windowPos + new Vector2(0, y) + _viewOffset);
                Pitch p = new Pitch(pitch);
                ImGui.PushStyleColor(ColorTarget.Text, GetPitchTextColor(p));
                ImGui.Text(p.ToString());
                ImGui.PopStyleColor();
                if (pitch == 0)
                {
                    return;
                }
                pitch -= 1;
            }
        }

        private Vector4 GetPitchTextColor(Pitch p)
        {
            switch (p.PitchClass)
            {
                case PitchClass.CSharp:
                case PitchClass.DSharp:
                case PitchClass.FSharp:
                case PitchClass.GSharp:
                case PitchClass.ASharp:
                    return RgbaFloat.White.ToVector4();
                case PitchClass.C:
                case PitchClass.D:
                case PitchClass.E:
                case PitchClass.F:
                case PitchClass.G:
                case PitchClass.A:
                case PitchClass.B:
                default:
                    return RgbaFloat.Black.ToVector4();
            }
        }

        private uint GetColor(byte pitchValue, bool left)
        {
            Pitch p = new Pitch(pitchValue);
            
            if (_selectedChannel != null && _livePlayer.IsKeyPressed(_selectedChannel, p))
            {
                return Util.Argb(1f, 0.95f, 0.4f, 0.4f);
            }

            switch (p.PitchClass)
            {
                case PitchClass.CSharp:
                case PitchClass.DSharp:
                case PitchClass.FSharp:
                case PitchClass.GSharp:
                case PitchClass.ASharp:
                    return left ? Util.Argb(1f, 0.1f, 0.1f, 0.1f) : Util.Argb(1f, 0.3f, 0.3f, 0.3f);
                case PitchClass.C:
                case PitchClass.D:
                case PitchClass.E:
                case PitchClass.F:
                case PitchClass.G:
                case PitchClass.A:
                case PitchClass.B:
                default:
                    return left ? Util.Argb(1f, 0.75f, 0.75f, 0.75f) : Util.Argb(1f, 1f, 1f, 1f);
            }
        }

        private Vector2 GetNoteStartPosition(Note note)
        {
            int stepDiff = (int)(note.StartTime.Step - _leftmostStep);
            float x = stepDiff * _stepWidth;

            int pitchDiff = _topPitch - note.Pitch.Value;
            float y = pitchDiff * _pitchHeight;

            return new Vector2(x, y);
        }

        private byte GetPitchValue(Vector2 pos)
        {
            uint notes = (uint)(pos.Y / _pitchHeight);
            return (byte)(_topPitch - notes);
        }

        private uint GetStep(Vector2 pos)
        {
            float x = Math.Max(0, pos.X);
            uint steps = (uint)(x / _stepWidth);
            return _leftmostStep + steps;
        }
    }
}
