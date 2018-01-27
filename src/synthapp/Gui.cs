using System;
using ImGuiNET;
using System.Collections.Generic;
using System.IO;
using SynthApp.Widgets;
using System.Numerics;
using System.Reflection;
using System.Linq;
using Veldrid;
using Veldrid.ImageSharp;

namespace SynthApp
{
    public class Gui
    {
        private readonly GraphicsDevice _gd;

        private readonly HashSet<Channel> _channelWindowsOpen = new HashSet<Channel>();
        private readonly HashSet<Channel> _channelWindowsClosed = new HashSet<Channel>();
        private bool _patternEditorVisible = true;

        // Input fields
        private TextInputBuffer _projectPathInput = new TextInputBuffer(1024);
        private readonly Texture _playButtonTexture;
        private readonly Texture _stopButtonTexture;
        private readonly TextureView _playButtonTextureBinding;
        private readonly TextureView _stopButtonTextureBinding;
        private readonly ImGuiRenderer _imguiRenderer;

        public Sequencer Sequencer { get; private set; }
        public KeyboardLivePlayInput KeyboardInput { get; private set; }
        public LiveNotePlayer LivePlayer { get; private set; }
        public PianoRoll PianoRoll { get; }


        public Gui(GraphicsDevice gd, Sequencer sequencer, KeyboardLivePlayInput keyboardInput, LiveNotePlayer livePlayer, ImGuiRenderer imguiRenderer)
        {
            _gd = gd;
            _playButtonTexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "Assets", "Textures", "button_play.png")).CreateDeviceTexture(gd, gd.ResourceFactory);
            _playButtonTextureBinding = gd.ResourceFactory.CreateTextureView(_playButtonTexture);
            _stopButtonTexture = new ImageSharpTexture(Path.Combine(AppContext.BaseDirectory, "Assets", "Textures", "button_stop.png")).CreateDeviceTexture(gd, gd.ResourceFactory);
            _stopButtonTextureBinding = gd.ResourceFactory.CreateTextureView(_stopButtonTexture);
            foreach (var type in Util.GetTypesWithAttribute(typeof(Gui).GetTypeInfo().Assembly, typeof(WidgetAttribute)))
            {
                DrawerCache.AddDrawer((Drawer)Activator.CreateInstance(type));
            }

            Sequencer = sequencer;
            KeyboardInput = keyboardInput;
            LivePlayer = livePlayer;
            PianoRoll = new PianoRoll(LivePlayer);
            _imguiRenderer = imguiRenderer;
        }

        public void DrawGui()
        {
            DrawMainMenu();

            DrawTopLevelFrame();

            foreach (Channel channel in _channelWindowsOpen)
            {
                DrawChannelWindow(channel);
            }

            Application appInstance = Application.Instance;
            DrawPattern(appInstance.SelectedPattern, appInstance.Project.Channels);

            if (appInstance.SelectedChannel != null)
            {
                KeyboardInput.Play(appInstance.SelectedChannel);
            }

            // Cleanup
            foreach (Channel channel in _channelWindowsClosed)
            {
                _channelWindowsOpen.Remove(channel);
            }
            _channelWindowsClosed.Clear();

            PianoRoll.Draw();
        }

        private void DrawMainMenu()
        {
            string openPopup = null;
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open Project"))
                    {
                        openPopup = "###OpenProjectPopup";
                    }
                    if (ImGui.MenuItem("Save Project"))
                    {
                        if (!string.IsNullOrEmpty(Application.Instance.ProjectContext.FullPath))
                        {
                            Application.Instance.SaveCurrentProject();
                        }
                        else
                        {
                            openPopup = "###SaveProjectPopup";
                        }
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Exit"))
                    {
                        Application.Instance.Window.Close();
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Window"))
                {
                    if (ImGui.MenuItem("Pattern Editor"))
                    {
                        if (_patternEditorVisible)
                        {
                            ImGuiNative.igSetWindowFocus2("Pattern Editor");
                        }
                        else
                        {
                            _patternEditorVisible = true;
                        }
                    }
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }

            if (openPopup != null)
            {
                ImGui.OpenPopup(openPopup);
            }

            if (ImGui.BeginPopup("###SaveProjectPopup"))
            {
                bool submitted = false;
                _projectPathInput.StringValue = Application.Instance.ProjectContext.FullPath ?? string.Empty;
                if (ImGui.InputText("Project File Path", _projectPathInput.Buffer, _projectPathInput.Length, InputTextFlags.EnterReturnsTrue, null))
                {
                    submitted = true;
                    Console.WriteLine("Submitted with value " + _projectPathInput.StringValue);
                }
                ImGui.SameLine();
                if (ImGui.Button("Save"))
                {
                    submitted = true;
                }
                if (submitted)
                {
                    string path = _projectPathInput.StringValue;
                    if (!string.IsNullOrEmpty(path))
                    {
                        Application.Instance.ProjectContext.FullPath = path;
                        SaveProjectTo(path);
                    }
                }
                ImGui.EndPopup();
            }

            if (ImGui.BeginPopup("###OpenProjectPopup"))
            {
                bool submitted = false;
                _projectPathInput.StringValue = Application.Instance.ProjectContext.FullPath ?? string.Empty;
                if (ImGui.InputText("Project File Path", _projectPathInput.Buffer, _projectPathInput.Length, InputTextFlags.EnterReturnsTrue, null))
                {
                    submitted = true;
                }
                ImGui.SameLine();
                if (ImGui.Button("Open"))
                {
                    submitted = true;
                }
                if (submitted)
                {
                    string path = _projectPathInput.StringValue;
                    if (File.Exists(path))
                    {
                        Application.Instance.LoadProject(path);
                    }
                }
                ImGui.EndPopup();
            }

            var input = Application.Instance.Input;
            if (input.GetKeyDown(Key.ControlLeft) && input.GetKeyDown(Key.S))
            {
                Application.Instance.SaveCurrentProject();
            }
        }

        private void DrawTopLevelFrame()
        {
            ImGui.PushStyleVar(StyleVar.WindowRounding, 0f);
            var io = ImGui.GetIO();
            ImGui.SetNextWindowSize(new Vector2(io.DisplaySize.X, 60), Condition.Always);
            ImGui.SetNextWindowPos(new Vector2(0, 20f), Condition.Always, Vector2.Zero);
            ImGui.BeginWindow("TopFrame", WindowFlags.NoTitleBar | WindowFlags.NoResize | WindowFlags.NoCollapse | WindowFlags.NoMove);

            float gain = Application.Instance.MasterCombiner.Gain;
            ImGui.PushItemWidth(80f);
            if (ImGui.DragFloat($"Gain", ref gain, 0f, 2f, dragSpeed: .01f))
            {
                Application.Instance.MasterCombiner.Gain = gain;
            }
            ImGui.PopItemWidth();
            ImGui.SameLine();

            if (ImGui.ImageButton(
                _imguiRenderer.GetOrCreateImGuiBinding(_gd.ResourceFactory, _playButtonTextureBinding),
                new Vector2(35, 35), Vector2.Zero, Vector2.One, 0, Vector4.Zero, Vector4.One))
            {
                Sequencer.Playing = true;
            }
            ImGui.SameLine();
            if (ImGui.ImageButton(
                _imguiRenderer.GetOrCreateImGuiBinding(_gd.ResourceFactory, _stopButtonTextureBinding),
                new Vector2(35, 35), Vector2.Zero, Vector2.One, 0, Vector4.Zero, Vector4.One))
            {
                Sequencer.Stop();
            }

            float bpm = (float)Globals.BeatsPerMinute;
            ImGui.SameLine();
            ImGui.PushItemWidth(80f);
            if (ImGui.DragFloat($"BPM", ref bpm, 10, 500, 1f))
            {
                Globals.BeatsPerMinute = bpm;
            }
            ImGui.PopItemWidth();

            ImGui.SameLine();
            ImGui.Separator();

            ImGui.SameLine();
            int patternIndex = Application.Instance.SelectedPatternIndex;
            ImGui.PushItemWidth(80f);
            if (ImGui.DragInt("Pattern", ref patternIndex, 0.01f, 0, Application.Instance.Project.Patterns.Count, null))
            {
                Application.Instance.SelectedPatternIndex = patternIndex;
            }
            ImGui.PopItemWidth();

            ImGui.SameLine();
            if (ImGui.Button("-"))
            {
                Application.Instance.SelectedPatternIndex = Math.Max(0, patternIndex - 1);
            }
            ImGui.SameLine();
            if (ImGui.Button("+"))
            {
                Application.Instance.SelectedPatternIndex = patternIndex + 1;
            }

            ImGui.PushID(1);
            ImGui.SameLine();
            string text = Sequencer.PlaybackMode == PlaybackMode.Pattern ? "Pattern" : "Song";
            if (ImGui.Button(text))
            {
                Sequencer.PlaybackMode = Sequencer.PlaybackMode == PlaybackMode.Pattern ? PlaybackMode.Song : PlaybackMode.Pattern;
            }
            ImGui.PopID();

            ImGui.SameLine();
            ImGui.Spacing();

            ImGui.EndWindow();
            ImGui.PopStyleVar(); // Window rounding
        }

        private void SaveProjectTo(string fullPath)
        {
            Project project = Application.Instance.Project;
            Application.Instance.SerializationServices.SaveTo(project, fullPath);
            Application.Instance.ProjectContext.FullPath = fullPath;
        }

        public void DrawPattern(Pattern pattern, IReadOnlyList<Channel> channels)
        {
            if (ImGui.BeginWindow(
                "Pattern Editor",
                ref _patternEditorVisible,
                WindowFlags.NoCollapse | WindowFlags.MenuBar | WindowFlags.AlwaysAutoResize))
            {
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("Add"))
                    {
                        if (ImGui.BeginMenu("New Channel"))
                        {
                            if (DrawChannelOptionMenuItems(out Channel newChannel))
                            {
                                AddChannel(newChannel);
                            }
                            ImGui.EndMenu();
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenuBar();
                }

                for (int i = 0; i < channels.Count; i++)
                {
                    ImGui.PushID("ChannelList" + i);
                    Channel channel = channels[i];
                    // Left-side pane for channel info and common controls
                    ImGui.BeginChildFrame(unchecked((uint)$"Left{i}".GetHashCode()), new Vector2(180, DrumPatternSequencer.GetFrameSize(16).Y), WindowFlags.Default);
                    bool muted = channel.Muted;
                    if (ImGui.Checkbox("Mute", ref muted))
                    {
                        channel.Muted = muted;
                    }
                    if (ImGui.BeginPopupContextItem($"{i}_MC", 1))
                    {
                        if (ImGui.MenuItem("Solo"))
                        {
                            ToggleSolo(channels, i);
                        }

                        ImGui.EndPopup();
                    }

                    ImGui.SameLine();
                    if (ImGui.Button($"{channel.Name}"))
                    {
                        Application.Instance.SelectedChannelIndex = i;
                        OpenChannelWindow(channel);
                    }
                    NoteSequence ns = Application.Instance.SelectedPattern.NoteSequences[i];
                    uint patternLength = Application.Instance.SelectedPattern.CalculateFinalNoteEndTime().Step;
                    if (ImGui.BeginPopupContextItem($"{i}_C", 1))
                    {
                        if (ImGui.MenuItem($"Piano Roll###PR{i}"))
                        {
                            ns.UsesPianoRoll = true;
                            Application.Instance.SelectedChannelIndex = i;
                        }
                        if (ImGui.MenuItem("Delete Channel"))
                        {
                            Application.Instance.Project.Channels.RemoveAt(i);
                            Sequencer.RemoveChannelState(i);
                        }
                        if (ImGui.BeginMenu("Replace Channel"))
                        {
                            if (DrawChannelOptionMenuItems(out Channel newChannel))
                            {
                                Application.Instance.Project.Channels[i] = newChannel;
                            }
                            ImGui.EndMenu();
                        }
                        ImGui.EndPopup();
                    }
                    float gain = channel.Gain;
                    if (ImGui.DragFloat("Gain", ref gain, 0f, 2f, dragSpeed: 0.01f))
                    {
                        channel.Gain = gain;
                    }
                    ImGui.EndChildFrame();

                    if (ns.Notes.Count == 0)
                    {
                        ns.UsesPianoRoll = false;
                    }

                    if (!ns.UsesPianoRoll)
                    {
                        ImGui.SameLine();
                        if (DrumPatternSequencer.DrawDrumSequencer((uint)i, ns, 16, patternLength, true))
                        {
                            Application.Instance.SelectedChannelIndex = i;
                        }
                    }
                    else
                    {
                        ImGui.SameLine();
                        if (PianoRoll.DrawPreviewOnly(ns, patternLength, DrumPatternSequencer.GetFrameSize(16), false))
                        {
                            Application.Instance.SelectedChannelIndex = i;
                            PianoRoll.Focus(pattern.NoteSequences[i].Notes.FirstOrDefault()?.Pitch ?? Pitch.MiddleC);
                        }
                        ImGui.SameLine();
                        ImGui.InvisibleButton("FAKEBUTTON", DrumPatternSequencer.GetFrameSize(16));
                    }

                    ImGui.PopID();
                }
            }
            ImGui.EndWindow();
        }

        private static void ToggleSolo(IReadOnlyList<Channel> channels, int channelIndex)
        {
            bool alreadySolo = false;

            for (int i = 0; i < channels.Count; i++)
            {
                if (i != channelIndex && !channels[i].Muted)
                {
                    alreadySolo = false;
                    break;
                }
                else if (i == channelIndex && !channels[i].Muted)
                {
                    alreadySolo = true;
                }
            }

            if (alreadySolo)
            {
                // Enable every channel.
                for (int i = 0; i < channels.Count; i++)
                {
                    channels[i].Muted = false;
                }
            }
            else
            {
                // Disable every channel except the selected.
                for (int i = 0; i < channels.Count; i++)
                {
                    if (i != channelIndex)
                    {
                        channels[i].Muted = true;
                    }
                }

                channels[channelIndex].Muted = false;
            }
        }

        private bool DrawChannelOptionMenuItems(out Channel channel)
        {
            channel = null;

            if (ImGui.MenuItem("Simple Oscillator"))
            {
                channel = new SimpleOscillatorSynth();
            }
            if (ImGui.MenuItem("Wave Sampler"))
            {
                channel = new WaveSampler(string.Empty);
            }
            if (ImGui.MenuItem("3x Oscillator"))
            {
                channel = new TripleOscillatorSynth();
            }

            return channel != null;
        }

        private void AddChannel(Channel channel)
        {
            Project project = Application.Instance.Project;
            project.Channels.Add(channel);
            foreach (var pattern in project.Patterns)
            {
                pattern.NoteSequences.Add(new NoteSequence());
            }

            Sequencer.AddNewChannelState();
        }

        private bool Draw<T>(string label, ref T obj)
        {
            object o = obj;
            if (DrawerCache.GetDrawer(typeof(T)).Draw(label, ref o, _gd))
            {
                obj = (T)o;
                return true;
            }

            return false;
        }

        private void OpenChannelWindow(Channel channel)
        {
            _channelWindowsOpen.Add(channel);
        }

        private void DrawChannelWindow(Channel channel)
        {
            bool opened = true;
            if (ImGui.BeginWindow($"{channel.Name}###ChannelWindow{channel.ID}", ref opened, WindowFlags.AlwaysAutoResize | WindowFlags.NoCollapse))
            {
                var drawer = DrawerCache.GetDrawer(channel.GetType());
                object o = channel;
                if (drawer.Draw($"{channel.Name}###Channel{channel.ID}", ref o, _gd))
                {
                    throw new NotImplementedException();
                }

            }
            ImGui.EndWindow();

            if (!opened)
            {
                _channelWindowsClosed.Add(channel);
            }
        }
    }
}
