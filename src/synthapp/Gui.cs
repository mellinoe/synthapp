using System;
using ImGuiNET;
using Veldrid.Graphics;
using System.Collections.Generic;

namespace SynthApp
{
    public class Gui
    {
        private readonly RenderContext _rc;
        private Channel _editedChannel;

        private readonly HashSet<Channel> _channelWindowsOpen = new HashSet<Channel>();
        private readonly HashSet<Channel> _channelWindowsClosed = new HashSet<Channel>();
        private bool _patternEditorVisible = true;

        public Gui(RenderContext rc)
        {
            _rc = rc;
            DrawerCache.AddDrawer(new NoteSequenceDrawer());
            DrawerCache.AddDrawer(new PatternTimeDrawer());
            DrawerCache.AddDrawer(new PitchDrawer());
        }

        public void DrawGui()
        {
            DrawMainMenu();

            foreach (Channel channel in _channelWindowsOpen)
            {
                DrawChannelWindow(channel);
            }

            // Cleanup
            foreach (Channel channel in _channelWindowsClosed)
            {
                _channelWindowsOpen.Remove(channel);
            }
            _channelWindowsClosed.Clear();
        }

        private void DrawMainMenu()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open Project"))
                    {

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
        }

        public void DrawPattern(Pattern pattern, IReadOnlyList<Channel> channels)
        {
            if (ImGui.BeginWindow(
                "Pattern Editor",
                ref _patternEditorVisible,
                WindowFlags.NoCollapse | WindowFlags.ShowBorders | WindowFlags.MenuBar))
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
                    if (ImGui.BeginChild(i.ToString(), true, WindowFlags.ShowBorders | WindowFlags.HorizontalScrollbar))
                    {
                        NoteSequence notes = pattern.NoteSequences[i];
                        Draw(string.Empty, ref notes);
                        ImGui.EndChild();
                    }
                    if (ImGui.Button($"[Channel {i}] {channel.Name}"))
                    {
                        _editedChannel = channel;
                        OpenChannelWindow(channel);
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
