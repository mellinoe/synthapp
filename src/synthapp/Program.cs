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
        private static AudioTrack s_audioTrack;
        private static float s_frequency = 440.0f;
        private static float s_duration = 1f;

        private static string[] s_shapeNames = Enum.GetNames(typeof(AudioTrack.Shape));
        private static int s_selectedShape = 0;

        public static AudioEngine AudioEngine { get; set; }

        public static void Main(string[] args)
        {
            var window = new DedicatedThreadWindow(960, 540, WindowState.Normal);
            s_rc = new OpenGLRenderContext(window);
            s_rc.ResourceFactory.AddShaderLoader(new EmbeddedResourceShaderLoader(typeof(Program).GetTypeInfo().Assembly));
            s_rc.ClearColor = RgbaFloat.Grey;
            window.Visible = true;
            s_imguiRenderer = new ImGuiRenderer(s_rc, window.NativeWindow);
            DateTime previousFrameTime = DateTime.UtcNow;
            AudioEngine = new AudioEngine();
            while (window.Exists)
            {
                DateTime newFrameTime = DateTime.UtcNow;
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
            ImGui.Text("Hello SynthApp");

            ImGui.DragFloat("Frequency", ref s_frequency, 20.0f, 20000f, 1f);
            ImGui.DragFloat("Duration", ref s_duration, 0.01f, 100f, 0.05f);
            ImGui.Combo("Shape", ref s_selectedShape, s_shapeNames);
            if (ImGui.Button("Change Track"))
            {
                s_audioTrack = new AudioTrack(s_frequency, 44100, s_duration, (AudioTrack.Shape)Enum.Parse(typeof(AudioTrack.Shape), s_shapeNames[s_selectedShape]));
                AudioEngine.SetAudioTrack(s_audioTrack);
            }

            if (ImGui.Button("Play"))
            {
                AudioEngine.Play();
            }
            if (ImGui.Button("Stop"))
            {
                AudioEngine.Stop();
            }
        }

        private static void Draw()
        {
            s_imguiRenderer.Render(s_rc, "Standard");
        }
    }
}
