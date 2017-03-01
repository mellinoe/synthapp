using ImGuiNET;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Veldrid.Graphics;
using Veldrid.Graphics.Direct3D;
using Veldrid.Graphics.OpenGL;
using Veldrid.Platform;

namespace SynthApp
{
    /// <summary>
    /// Represents the top-level application object.
    /// </summary>
    public class Application
    {
        private readonly OpenTKWindow window;
        private readonly RenderContext s_rc;
        private readonly ImGuiRenderer s_imguiRenderer;
        private readonly LiveNotePlayer s_livePlayer;
        private readonly AudioStreamCombiner s_combiner;
        private readonly StreamingAudioSource s_streamSource;
        private readonly KeyboardLivePlayInput s_keyboardInput;

        public Sequencer Sequencer { get; }
        public Gui Gui { get; }

        public static Application Instance { get; private set; }

        public Application()
        {
            window = new DedicatedThreadWindow(960, 540, WindowState.Maximized);
            window.Title = "Synth";
            s_rc = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? (RenderContext)new D3DRenderContext(window)
                : new OpenGLRenderContext(window);
            s_rc.ResourceFactory.AddShaderLoader(new EmbeddedResourceShaderLoader(typeof(Application).GetTypeInfo().Assembly));
            s_rc.ClearColor = RgbaFloat.Grey;
            window.Visible = true;
            s_imguiRenderer = new ImGuiRenderer(s_rc, window.NativeWindow);
            CustomStyle.ActivateStyle2(true, 1f);

            Sequencer = new Sequencer();
            s_livePlayer = new LiveNotePlayer();
            s_combiner = new AudioStreamCombiner();
            s_combiner.Add(Sequencer);
            s_combiner.Add(s_livePlayer);
            s_streamSource = new StreamingAudioSource(s_combiner, 40000);
            s_streamSource.DataProvider = s_combiner;
            s_streamSource.Play();

            s_keyboardInput = new KeyboardLivePlayInput(s_livePlayer, s_streamSource);

            Gui = new Gui(s_rc, Sequencer, s_keyboardInput, s_livePlayer);

            Debug.Assert(Instance == null);
            Instance = this;
        }

        public void Run()
        {
            DateTime previousFrameTime = DateTime.Now;
            while (window.Exists)
            {
                DateTime newFrameTime = DateTime.Now;
                float deltaSeconds = (float)(newFrameTime - previousFrameTime).TotalSeconds;
                InputSnapshot snapshot = window.GetInputSnapshot();
                Globals.Input.UpdateFrameInput(snapshot);
                s_rc.Viewport = new Viewport(0, 0, s_rc.Window.Width, s_rc.Window.Height);
                s_rc.ClearBuffer();
                Update(deltaSeconds, snapshot);
                Draw();
                s_rc.SwapBuffers();
            }
        }

        private void Update(float deltaSeconds, InputSnapshot snapshot)
        {
            s_imguiRenderer.Update(deltaSeconds);
            s_imguiRenderer.OnInputUpdated(snapshot);
            Gui.DrawGui();

            if (ImGui.Button("Play the pattern"))
            {
                Sequencer.Playing = true;
            }
            if (ImGui.Button("Play sine wave at 440 Hz"))
            {
                short[] data = new short[3 * Globals.SampleRate];
                for (int i = 0; i < data.Length; i++)
                {
                    double t = i * 440 / (double)Globals.SampleRate;
                    t *= 2 * Math.PI;
                    double sample = Math.Sin(t);
                    data[i] = (short)(sample * short.MaxValue);
                }

                Globals.AudioEngine.PlayAudioData(data, Globals.SampleRate);
            }
            if (ImGui.Button("Stop"))
            {
                Sequencer.Playing = false;
            }

            float bpm = (float)Globals.BeatsPerMinute;
            if (ImGui.DragFloat($"Beats per minute", ref bpm, 10, 500, 1f))
            {
                Globals.BeatsPerMinute = bpm;
            }
        }

        private void Draw()
        {
            s_imguiRenderer.Render(s_rc, "Standard");
        }
    }
}
