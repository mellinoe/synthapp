using ImGuiNET;
using System;
using System.Reflection;
using Veldrid.Graphics;
using Veldrid.Graphics.OpenGL;
using Veldrid.Platform;

namespace SynthApp
{
    public class Program
    {
        private static OpenGLRenderContext s_rc;
        private static ImGuiRenderer s_imguiRenderer;

        public static void Main(string[] args)
        {
            var window = new DedicatedThreadWindow(960, 540, WindowState.Normal);
            s_rc = new OpenGLRenderContext(window);
            s_rc.ResourceFactory.AddShaderLoader(new EmbeddedResourceShaderLoader(typeof(Program).GetTypeInfo().Assembly));
            s_rc.ClearColor = RgbaFloat.Grey;
            window.Visible = true;
            s_imguiRenderer = new ImGuiRenderer(s_rc, window.NativeWindow);
            DateTime previousFrameTime = DateTime.UtcNow;
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
        }

        private static void Draw()
        {
            s_imguiRenderer.Render(s_rc, "Standard");
        }
    }
}
