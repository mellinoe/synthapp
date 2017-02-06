using ImGuiNET;
using OpenTK.Audio.OpenAL;
using System;
using System.Reflection;
using Veldrid.Graphics;
using Veldrid.Graphics.OpenGL;
using Veldrid.Platform;
using System.Linq;

namespace SynthApp
{
    public class Program
    {
        private static OpenGLRenderContext s_rc;
        private static ImGuiRenderer s_imguiRenderer;
        private static int s_chunkBuffer = 4;

        private static StreamingAudioSource s_streamSource;

        public static Sequencer Sequencer { get; set; }

        public static void Main(string[] args)
        {
            var window = new DedicatedThreadWindow(960, 540, WindowState.Normal);
            s_rc = new OpenGLRenderContext(window, false);
            s_rc.ResourceFactory.AddShaderLoader(new EmbeddedResourceShaderLoader(typeof(Program).GetTypeInfo().Assembly));
            s_rc.ClearColor = RgbaFloat.Grey;
            window.Visible = true;
            s_imguiRenderer = new ImGuiRenderer(s_rc, window.NativeWindow);
            DateTime previousFrameTime = DateTime.Now;

            Sequencer = new Sequencer();
            s_streamSource = new StreamingAudioSource(Sequencer, 40000);

            while (window.Exists)
            {
                DateTime newFrameTime = DateTime.Now;
                float deltaSeconds = (float)(newFrameTime - previousFrameTime).TotalSeconds;
                InputSnapshot snapshot = window.GetInputSnapshot();
                s_rc.Viewport = new Viewport(0, 0, s_rc.Window.Width, s_rc.Window.Height);
                s_rc.ClearBuffer();
                Update(deltaSeconds, snapshot);
                Draw();
                s_rc.SwapBuffers();
            }
        }

        private static void Update(float deltaSeconds, InputSnapshot snapshot)
        {
            s_imguiRenderer.Update(deltaSeconds);
            s_imguiRenderer.OnInputUpdated(snapshot);

            if (ImGui.Button("Play the patterns"))
            {
                s_streamSource.DataProvider = Sequencer;
                s_streamSource.Play();
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
            if (ImGui.Button("Play simple sine provider"))
            {
                s_streamSource.DataProvider = new SimpleSineProvider();
                s_streamSource.Play();
            }
            if (s_streamSource.DataProvider is SimpleSineProvider ssi)
            {
                float freq = (float)ssi.Frequency;
                if (ImGui.DragFloat("Sine Frequency", ref freq, 10, 20000, 1f))
                {
                    ssi.Frequency = freq;
                }
            }
            if (ImGui.Button("Stop"))
            {
                Globals.AudioEngine.Stop();
                s_streamSource.Stop();
            }

            int chunkSize = (int)s_streamSource.BufferedSamples;
            if (ImGui.DragInt("Chunk Size", ref chunkSize, 33f, 100, 200000, $"Chunk Size: {chunkSize}"))
            {
                s_streamSource.BufferedSamples = (uint)chunkSize;
            }
            if (ImGui.DragInt("Chunk Buffer Count", ref s_chunkBuffer, 1f, 1, 100, $"Chunk Buffer: {s_chunkBuffer}"))
            { }

            float bpm = (float)Globals.BeatsPerMinute;
            if (ImGui.DragFloat($"Beats per minute", ref bpm, 10, 500, 1f))
            {
                Globals.BeatsPerMinute = bpm;
            }
        }

        private static void Draw()
        {
            s_imguiRenderer.Render(s_rc, "Standard");
        }
    }
}
