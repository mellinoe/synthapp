using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Veldrid.Graphics;

namespace SynthApp
{
    public class PianoRoll
    {
        private readonly LiveNotePlayer _livePlayer;
        private float _stepWidth = 40f;
        private float _pitchHeight = 20f;
        private byte _topPitch = new Pitch(PitchClass.B, 10).Value;
        private uint _leftmostStep = 0;
        private float _pianoKeyWidth = 60f;
        private float _bottomPanelHeight = 150f;
        private Vector2 _viewOffset;

        private const float MinStepWidth = 5f;
        private const float MaxStepWidth = 120f;

        private HashSet<Note> _noteRemovals = new HashSet<Note>();
        private Note _interacting;
        private bool _dragging;
        private bool _resizing;
        private Vector2 _dragOffset;

        private bool _dragScrolling;
        private Vector2 _dragScrollPos;
        private PatternTime _newNoteDuration = PatternTime.Steps(2);
        private Pitch? _focusPitch;

        public PianoRoll(LiveNotePlayer livePlayer)
        {
            _livePlayer = livePlayer;

            byte c5Value = new Pitch(PitchClass.C, 5).Value;
            uint diff = (uint)(_topPitch - c5Value);
            _viewOffset = new Vector2(0, -diff * _pitchHeight);
        }

        public unsafe void Draw()
        {
            ImGui.SetNextWindowSize(new Vector2(600, 400), SetCondition.FirstUseEver);
            bool opened = Application.Instance.SelectedChannel != null;
            if (opened)
            {
                if (_focusPitch != null)
                {
                    ImGui.SetNextWindowFocus();
                }
                if (ImGui.BeginWindow("Piano Roll", ref opened, 1.0f, WindowFlags.NoScrollWithMouse | WindowFlags.NoScrollbar | WindowFlags.MenuBar))
                {
                    NoteSequence notes = GetActiveNoteSequence();
                    if (ImGui.BeginMenuBar())
                    {
                        if (ImGui.BeginMenu("Edit"))
                        {
                            if (ImGui.MenuItem("Clear"))
                            {
                                notes.Notes.Clear();
                            }
                            ImGui.EndMenu();
                        }

                        ImGui.EndMenuBar();
                    }
                    IO io = ImGui.GetIO();
                    DrawList dl = DrawList.GetForCurrentWindow();
                    Vector2 windowPos = ImGui.GetCursorScreenPos();
                    Vector2 gridPos = windowPos + new Vector2(_pianoKeyWidth, 0);
                    PatternTime duration = GetActivePattern().CalculateFinalNoteEndTime();
                    duration = PatternTime.RoundToUpperBar(duration);
                    if (duration < Pattern.DefaultPatternDuration)
                    {
                        duration = Pattern.DefaultPatternDuration;
                    }
                    float gridWidth = (float)(Math.Ceiling(duration.TotalBeats) * (_stepWidth * 4));
                    ImGui.BeginChild("NoMoveChild", false, WindowFlags.NoMove | WindowFlags.NoScrollbar);
                    Vector2 totalSize = new Vector2(gridWidth, (_topPitch + 1) * _pitchHeight);
                    Vector2 gridSize = ImGui.GetWindowSize() - new Vector2(_pianoKeyWidth, _bottomPanelHeight);

                    if (_focusPitch != null)
                    {
                        float topY = GetPitchStartY(_focusPitch.Value);
                        _viewOffset = new Vector2(0, -topY + gridSize.Y / 2f);
                        _focusPitch = null;
                    }

                    // Draw note grid first
                    dl.PushClipRect(gridPos, gridPos + gridSize, true);
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
                            gridPos + new Vector2(gridWidth, y) + _viewOffset,
                            Util.Argb(alpha, 1f, 1f, 1f),
                            1.0f);
                        if (pitch != 0)
                        {
                            pitch -= 1;
                        }
                    }

                    foreach (Note note in notes.Notes)
                    {
                        Vector2 pos = GetNoteStartPosition(note) + _viewOffset;
                        Vector2 size = new Vector2(note.Duration.Step * _stepWidth, _pitchHeight);
                        bool hovering = ImGui.IsMouseHoveringWindow() && ImGui.IsMouseHoveringRect(pos + gridPos, pos + gridPos + size, true);
                        uint color = hovering ? Util.Argb(0.95f, 1.0f, 0.4f, 0.4f) : Util.Argb(0.75f, 1.0f, 0.2f, 0.2f);
                        dl.AddRectFilled(pos + gridPos, pos + gridPos + size, color, 5f);
                        dl.AddText(pos + gridPos + new Vector2(5, 0), note.Pitch.ToString(), Util.Argb(1, 1, 1, 1));
                        if (hovering)
                        {
                            if (io.MouseDown[0] && !_resizing && !_dragging)
                            {
                                _interacting = note;
                                _dragging = true;
                                _dragOffset = io.MousePosition - gridPos - pos;
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
                        if (ImGui.IsMouseHoveringWindow() && ImGui.IsMouseHoveringRect(
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

                    // Draw playback position indicator
                    if (Application.Instance.Sequencer.Playing)
                    {
                        double samplePos = Application.Instance.Sequencer.PlaybackPositionSamples;
                        double totalPatternSamples = GetActivePattern().CalculateFinalNoteEndTime().ToSamplesAuto();
                        samplePos = samplePos % totalPatternSamples;
                        PatternTime time = PatternTime.Samples((uint)samplePos, Globals.SampleRate, Globals.BeatsPerMinute);
                        double totalBeats = time.TotalBeats;
                        float x = (float)(totalBeats * _stepWidth * 4);
                        float thickness = 12f;
                        dl.AddLine(
                            gridPos + new Vector2(x, 0) + _viewOffset,
                            gridPos + new Vector2(x, totalSize.Y) + _viewOffset,
                            Util.Argb(0.3f, 1f, 1f, 1f),
                            thickness);
                    }

                    dl.PopClipRect();

                    Vector2 pianoPaneSize = new Vector2(_pianoKeyWidth, ImGui.GetWindowHeight() - _bottomPanelHeight);
                    DrawPianoKeys(dl, windowPos, pianoPaneSize, totalSize);

                    Vector2 bottomPaneStart = windowPos + new Vector2(_pianoKeyWidth, ImGui.GetWindowHeight() - _bottomPanelHeight);
                    Vector2 bottomPaneSize = new Vector2(ImGui.GetWindowWidth() - _pianoKeyWidth, _bottomPanelHeight);
                    DrawBottomPane(dl, notes, bottomPaneStart, bottomPaneSize);

                    // Drawing finished.
                    // Handle removals
                    notes.Notes.RemoveAll(_noteRemovals.Contains);
                    _noteRemovals.Clear();

                    if (ImGui.IsMouseHoveringWindow() && ImGui.IsMouseHoveringRect(gridPos, gridPos + gridSize, true))
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
                                Vector2 end = io.MousePosition - _viewOffset - gridPos;
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
        }

        private static Pattern GetActivePattern()
        {
            return Application.Instance.Project.Patterns[0];
        }

        public bool DrawPreviewOnly(NoteSequence notes, uint totalPatternSteps, Vector2 size, bool drawGrid)
        {
            DrawList dl = DrawList.GetForCurrentWindow();
            Vector2 drawPos = ImGui.GetCursorScreenPos();
            float stepWidth = size.X / totalPatternSteps;
            byte topPitch = notes.Notes.Max(n => n.Pitch.Value);
            byte bottomPitch = notes.Notes.Min(n => n.Pitch.Value);
            Debug.Assert(topPitch >= bottomPitch);
            float pitchRange = topPitch - bottomPitch + 1;
            float pitchHeight = size.Y / pitchRange;
            foreach (var note in notes.Notes)
            {
                float left = note.StartTime.Step * stepWidth;
                float right = left + (note.Duration.Step * stepWidth);
                float top = ((topPitch - note.Pitch.Value) / pitchRange) * size.Y;
                float bottom = top + pitchHeight;
                dl.AddRectFilled(drawPos + new Vector2(left, top), drawPos + new Vector2(right, bottom), Util.Argb(1f, 1f, 0f, 0f), 1f);
            }
            if (ImGui.IsMouseHoveringWindow() && ImGui.IsMouseHoveringRect(drawPos, drawPos + size, true) && ImGui.IsMouseClicked(0))
            {
                return true;
            }

            return false;
        }

        public void Focus(Pitch pitch)
        {
            _focusPitch = pitch;
        }

        private static unsafe NoteSequence GetActiveNoteSequence()
        {
            return Application.Instance.Project.Patterns[0].NoteSequences[Application.Instance.SelectedChannelIndex];
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
            GetActiveNoteSequence().Notes.Add(n);
        }

        private unsafe void ClampViewPos()
        {
            _viewOffset = Vector2.Min(Vector2.Zero, _viewOffset);
        }

        private void DrawPianoKeys(DrawList dl, Vector2 startPos, Vector2 size, Vector2 totalSize)
        {
            dl.PushClipRect(startPos, startPos + size, false);
            byte pitch = _topPitch;
            for (float y = 0; y < totalSize.Y; y += _pitchHeight)
            {
                uint leftCol = GetColor(pitch, true);
                uint rightCol = GetColor(pitch, false);
                dl.AddRectFilledMultiColor(
                    startPos + new Vector2(0, y + _viewOffset.Y),
                    startPos + new Vector2(0, y + _viewOffset.Y) + new Vector2(_pianoKeyWidth, _pitchHeight),
                    leftCol,
                    rightCol,
                    rightCol,
                    leftCol);
                dl.AddLine(
                    startPos + new Vector2(0, y + _pitchHeight + _viewOffset.Y),
                    startPos + new Vector2(0, y + _pitchHeight + _viewOffset.Y) + new Vector2(_pianoKeyWidth, _pitchHeight),
                    Util.Argb(1f, 0.6f, 0.6f, 0.6f),
                    2f);
                Pitch p = new Pitch(pitch);
                dl.AddText(startPos + new Vector2(0, y + _viewOffset.Y), p.ToString(), Util.RgbaToArgb(GetPitchTextColor(p)));
                if (pitch == 0)
                {
                    return;
                }
                pitch -= 1;
            }
            dl.PopClipRect();
        }

        private void DrawBottomPane(DrawList dl, NoteSequence ns, Vector2 startPos, Vector2 size)
        {
            float bottomY = startPos.Y + size.Y;
            float minX = 0f;
            float maxX = size.X;
            dl.PushClipRect(startPos, startPos + size, false);

            foreach (var note in ns.Notes)
            {
                Vector2 noteStart = GetNoteStartPosition(note) + _viewOffset;
                Vector2 noteEnd = noteStart + new Vector2(note.Duration.Step * _stepWidth, 0) + _viewOffset;
                if ((noteStart.X >= minX && noteStart.X <= maxX) || (noteEnd.X <= maxX && noteEnd.X >= minX) || (noteStart.X <= minX && noteEnd.X >= maxX))
                {
                    DrawNoteAttributeBar(note, noteStart.X + startPos.X);
                }
            }

            void DrawNoteAttributeBar(Note n, float startX)
            {
                float maxHeight = size.Y - 10f;
                float actualHeight = (n.Velocity / 1.0f) * maxHeight;
                Vector2 lineTop = new Vector2(startX, bottomY - actualHeight);
                Vector2 lineBottom = new Vector2(startX, bottomY - 5);
                dl.AddLine(lineBottom, lineTop, Util.Argb(1, 1, 0, 0), 6f);
                dl.AddRectFilled(lineTop - new Vector2(3, 3), lineTop + new Vector2(3, 3), Util.Argb(1, 1, 1, 1), 1f);
                Vector2 clickSize = new Vector2(5, 5);
                if (ImGui.IsMouseHoveringRect(new Vector2(startX, bottomY - maxHeight) - clickSize, new Vector2(startX, bottomY - 5) + clickSize, true))
                {
                    ImGui.SetTooltip("Velocity: " + n.Velocity.ToString());
                    if (ImGui.IsMouseDown(0))
                    {
                        float mousePosY = ImGui.GetMousePos().Y;
                        float diff = bottomY - mousePosY;
                        float newVelocity = Util.Clamp(diff / maxHeight, 0f, 1f);
                        n.Velocity = newVelocity;
                    }
                }
            }

            dl.PopClipRect();
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

            if (Application.Instance.SelectedChannel != null && _livePlayer.IsKeyPressed(Application.Instance.SelectedChannel, p))
            {
                return Util.Argb(1f, 1f, 0.58f, 0f);
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
            float y = GetPitchStartY(note.Pitch);

            return new Vector2(x, y);
        }

        private float GetPitchStartY(Pitch pitch)
        {
            int pitchDiff = _topPitch - pitch.Value;
            float y = pitchDiff * _pitchHeight;
            return y;
        }

        private byte GetPitchValue(Vector2 pos)
        {
            uint notes = (uint)(pos.Y / _pitchHeight);
            if (notes > _topPitch)
            {
                return 0;
            }
            else
            {
                return (byte)(_topPitch - notes);
            }
        }

        private uint GetStep(Vector2 pos)
        {
            float x = Math.Max(0, pos.X);
            uint steps = (uint)(x / _stepWidth);
            return _leftmostStep + steps;
        }
    }
}
