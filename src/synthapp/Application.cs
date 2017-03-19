using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
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
        private Stopwatch _sw;
        private long _previousFrameTicks;
        private FrameTimeAverager _fta;

        public Sequencer Sequencer { get; }
        public Gui Gui { get; }
        public SerializationServices SerializationServices { get; }
        public ProjectContext ProjectContext { get; } = new ProjectContext();
        public Project Project { get; private set; }
        public AudioEngine AudioEngine { get; }
        public InputTracker Input { get; } = new InputTracker();
        public int SelectedChannelIndex { get; set; }
        public Channel SelectedChannel => Project.Channels[SelectedChannelIndex];

        public double DesiredFramerate { get; set; } = 60.0;
        public bool LimitFrameRate { get; set; } = true;

        public static Application Instance { get; private set; }

        public Application()
        {
            Debug.Assert(Instance == null);
            Instance = this;
            AudioEngine = CreateDefaultAudioEngine();
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

            SerializationServices = new SerializationServices();
            string latestProject = SynthAppPreferences.Instance.GetLastOpenedProject();
            if (latestProject != null)
            {
                LoadProject(latestProject);
            }
            else
            {
                Project = Project.CreateDefault();
            }

            s_livePlayer = new LiveNotePlayer();
            Sequencer = new Sequencer(s_livePlayer, Project.Channels.Count);
            s_combiner = new AudioStreamCombiner();
            s_combiner.Add(Sequencer);
            s_streamSource = AudioEngine.CreateStreamingAudioSource(s_combiner, 2000);
            s_streamSource.DataProvider = s_combiner;
            s_streamSource.Play();

            s_keyboardInput = new KeyboardLivePlayInput(s_livePlayer, s_streamSource);

            Gui = new Gui(s_rc, Sequencer, s_keyboardInput, s_livePlayer);

            Debug.Assert(Project != null);
        }

        private AudioEngine CreateDefaultAudioEngine()
        {
            return new OpenALAudioEngine();
        }

        public void Run()
        {
            _sw = Stopwatch.StartNew();
            _fta = new FrameTimeAverager(666.666);

            while (window.Exists)
            {
                double desiredFrameTime = 1000.0 / DesiredFramerate;
                long currentFrameTicks = _sw.ElapsedTicks;
                double deltaMilliseconds = (currentFrameTicks - _previousFrameTicks) * (1000.0 / Stopwatch.Frequency);

                while (LimitFrameRate && deltaMilliseconds < desiredFrameTime)
                {
                    Thread.Sleep(0);
                    currentFrameTicks = _sw.ElapsedTicks;
                    deltaMilliseconds = (currentFrameTicks - _previousFrameTicks) * (1000.0 / Stopwatch.Frequency);
                }
                _previousFrameTicks = currentFrameTicks;
                float deltaSeconds = (float)deltaMilliseconds / 1000.0f;
                _fta.AddTime(deltaMilliseconds);
                window.Title = "Synth (" + _fta.CurrentAverageFramesPerSecond.ToString("##.00") + " fps)";

                InputSnapshot snapshot = window.GetInputSnapshot();
                Input.UpdateFrameInput(snapshot);
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
            if (ImGui.Button("Stop"))
            {
                Sequencer.Stop();
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

        public void LoadProject(string path)
        {
            try
            {
                Project = SerializationServices.Load<Project>(path);
                ProjectContext.FullPath = path;
                SynthAppPreferences.Instance.SetLatestProject(path);
            }
            catch (JsonSerializationException)
            {
                Project = Project.CreateDefault();
            }
        }

        public void SaveCurrentProject()
        {
            Debug.Assert(ProjectContext.FullPath != null);
            SerializationServices.SaveTo(Project, ProjectContext.FullPath);
        }
    }
}
