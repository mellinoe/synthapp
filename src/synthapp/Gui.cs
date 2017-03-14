using System;
using ImGuiNET;
using Veldrid.Graphics;
using System.Collections.Generic;
using System.IO;
using SynthApp.Widgets;
using System.Numerics;
using Veldrid.Platform;
using System.Reflection;
using System.Linq;

namespace SynthApp
{
    public class Gui
    {
        private readonly RenderContext _rc;

        private readonly HashSet<Channel> _channelWindowsOpen = new HashSet<Channel>();
        private readonly HashSet<Channel> _channelWindowsClosed = new HashSet<Channel>();
        private bool _patternEditorVisible = true;

        // Input fields
        private TextInputBuffer _projectPathInput = new TextInputBuffer(1024);

        public Sequencer Sequencer { get; private set; }
        public KeyboardLivePlayInput KeyboardInput { get; private set; }
        public LiveNotePlayer LivePlayer { get; private set; }
        public PianoRoll PianoRoll { get; }

        public Gui(RenderContext rc, Sequencer sequencer, KeyboardLivePlayInput keyboardInput, LiveNotePlayer livePlayer)
        {
            _rc = rc;
            foreach (var type in Util.GetTypesWithAttribute(typeof(Gui).GetTypeInfo().Assembly, typeof(WidgetAttribute)))
            {
                DrawerCache.AddDrawer((Drawer)Activator.CreateInstance(type));
            }

            Sequencer = sequencer;
            KeyboardInput = keyboardInput;
            LivePlayer = livePlayer;
            PianoRoll = new PianoRoll(LivePlayer);
        }

        public void DrawGui()
        {
            DrawMainMenu();

            foreach (Channel channel in _channelWindowsOpen)
            {
                DrawChannelWindow(channel);
            }

            Application appInstance = Application.Instance;
            DrawPattern(appInstance.Project.Patterns[0], appInstance.Project.Channels);

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
                        _rc.Window.Close();
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
                            if (ImGui.MenuItem("Simple Oscillator"))
                            {
                                AddChannel(new SimpleOscillatorSynth());
                            }
                            if (ImGui.MenuItem("Wave Sampler"))
                            {
                                AddChannel(new WaveSampler(string.Empty));
                            }
                            if (ImGui.MenuItem("3x Oscillator"))
                            {
                                AddChannel(new TripleOscillatorSynth());
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
                    if (ImGui.Button($"[Channel {i}] {channel.Name}"))
                    {
                        OpenChannelWindow(channel);
                    }
                    NoteSequence ns = Application.Instance.Project.Patterns[0].NoteSequences[i];
                    uint patternLength = Application.Instance.Project.Patterns[0].CalculateFinalNoteEndTime().Step;
                    if (ImGui.BeginPopupContextItem($"{i}_C", 1))
                    {
                        if (ImGui.Selectable($"Piano Roll###PR{i}"))
                        {
                            ns.UsesPianoRoll = true;
                            Application.Instance.SelectedChannelIndex = i;
                        }

                        ImGui.EndPopup();
                    }

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
                            PianoRoll.Focus();
                        }
                        ImGui.SameLine();
                        ImGui.InvisibleButton("FAKEBUTTON", DrumPatternSequencer.GetFrameSize(16));
                    }

                    ImGui.PopID();
                }
            }
            ImGui.EndWindow();
        }

        private void AddChannel(Channel channel)
        {
            Project project = Application.Instance.Project;
            project.Channels = project.Channels.Append(channel).ToArray();
            project.Patterns[0].NoteSequences.Add(new NoteSequence());
            Sequencer.AddNewChannelState();
        }

        private bool Draw<T>(string label, ref T obj)
        {
            object o = obj;
            if (DrawerCache.GetDrawer(typeof(T)).Draw(label, ref o, _rc))
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
                if (drawer.Draw($"{channel.Name}###Channel{channel.ID}", ref o, _rc))
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
