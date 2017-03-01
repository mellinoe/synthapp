using System;
using ImGuiNET;
using Veldrid.Graphics;
using System.Collections.Generic;
using System.IO;

namespace SynthApp
{
    public class Gui
    {
        private readonly RenderContext _rc;
        private Channel _editedChannel;

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
            DrawerCache.AddDrawer(new NoteSequenceDrawer());
            DrawerCache.AddDrawer(new PatternTimeDrawer());
            DrawerCache.AddDrawer(new PitchDrawer());

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

            DrawPattern(Sequencer.Pattern, Sequencer.Channels);

            if (_editedChannel != null)
            {
                KeyboardInput.Play(_editedChannel);
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
                            SaveProjectTo(Application.Instance.ProjectContext.FullPath);
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
                        Application.Instance.ProjectContext.FullPath = path;
                        OpenProjectAt(path);
                    }
                }
                ImGui.EndPopup();
            }
        }

        private void SaveProjectTo(string fullPath)
        {
            Project project = Application.Instance.GetProject();
            Application.Instance.SerializationServices.SaveTo(project, fullPath);
        }

        private void OpenProjectAt(string fullPath)
        {
            Project project = Application.Instance.SerializationServices.LoadFrom<Project>(fullPath);
            Application.Instance.LoadProject(project);
        }

        public void DrawPattern(Pattern pattern, IReadOnlyList<Channel> channels)
        {
            if (ImGui.BeginWindow(
                "Pattern Editor",
                ref _patternEditorVisible,
                WindowFlags.NoCollapse | WindowFlags.MenuBar))
            {
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("Add"))
                    {
                        if (ImGui.BeginMenu("New Channel"))
                        {
                            if (ImGui.MenuItem("Simple Oscillator"))
                            {
                            }
                            if (ImGui.MenuItem("Wave Sampler"))
                            {
                            }
                            ImGui.EndMenu();
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenuBar();
                }

                ImGui.PushID("ChannelList");
                for (int i = 0; i < channels.Count; i++)
                {
                    Channel channel = channels[i];
                    if (ImGui.Button($"[Channel {i}] {channel.Name}"))
                    {
                        _editedChannel = channel;
                        OpenChannelWindow(channel);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Piano Roll###PR{i}"))
                    {
                        PianoRoll.SetSelectedChannel(channel);
                        PianoRoll.SetNotes(Sequencer.Pattern.NoteSequences[i].Notes, Sequencer.Pattern.Duration);
                        _editedChannel = channel;
                    }
                }
                ImGui.PopID();
            }
            ImGui.EndWindow();
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
            if (ImGui.BeginWindow($"{channel.Name}###ChannelWindow{channel.ID}", ref opened, WindowFlags.Default))
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
