using Newtonsoft.Json;
using SynthApp.OpenAL;
using SynthApp.XAudio2;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Veldrid;
using Veldrid.Graphics;
using Veldrid.Platform;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace SynthApp
{
    /// <summary>
    /// Represents the top-level application object.
    /// </summary>
    public class Application
    {
        private readonly Sdl2Window _window;
        private readonly RenderContext _rc;
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
        public int SelectedPatternIndex { get; set; }
        public Channel SelectedChannel => Project.Channels[SelectedChannelIndex];
        public Pattern SelectedPattern => Project.GetOrCreatePattern(SelectedPatternIndex);

        public AudioStreamCombiner MasterCombiner => s_combiner;

        public double DesiredFramerate { get; set; } = 60.0;
        public bool LimitFrameRate { get; set; } = true;

        public static Application Instance { get; private set; }
        public Sdl2Window Window => _window;

        public Application()
        {
            Debug.Assert(Instance == null);
            Instance = this;
            WindowCreateInfo wci = new WindowCreateInfo();
            wci.X = 50;
            wci.WindowWidth = 960;
            wci.WindowHeight = 540;
            wci.WindowInitialState = WindowState.Maximized;
            RenderContextCreateInfo rcci = new RenderContextCreateInfo();

            VeldridStartup.CreateWindowAndRenderContext(ref wci, ref rcci, out _window, out _rc);
            _window.Title = "Synth";
            _rc.ClearColor = RgbaFloat.Grey;
            _window.Visible = true;
            s_imguiRenderer = new ImGuiRenderer(_rc, _window.Width, _window.Height);
            _window.Resized += () => s_imguiRenderer.WindowResized(_window.Width, _window.Height);
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

            AudioEngine = CreateDefaultAudioEngine();
            s_livePlayer = new LiveNotePlayer();
            Sequencer = new Sequencer(s_livePlayer, Project.Channels.Count);
            s_combiner = new AudioStreamCombiner();
            s_combiner.Gain = 0.5f;
            s_combiner.Add(Sequencer);
            s_streamSource = AudioEngine.CreateStreamingAudioSource(s_combiner, 5000);
            s_streamSource.DataProvider = s_combiner;
            s_streamSource.Play();

            s_keyboardInput = new KeyboardLivePlayInput(s_livePlayer, s_streamSource);

            Gui = new Gui(_rc, Sequencer, s_keyboardInput, s_livePlayer);

            Debug.Assert(Project != null);
        }

        private AudioEngine CreateDefaultAudioEngine()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? (AudioEngine)new XAudio2AudioEngine() : new OpenALAudioEngine();
        }

        public void Run()
        {
            _sw = Stopwatch.StartNew();
            _fta = new FrameTimeAverager(666.666);

            while (_window.Exists)
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
                _window.Title = "Synth (" + _fta.CurrentAverageFramesPerSecond.ToString("##.00") + " fps)";

                InputSnapshot snapshot = _window.PumpEvents();
                Input.UpdateFrameInput(snapshot);
                _rc.Viewport = new Viewport(0, 0, _window.Width, _window.Height);
                _rc.ClearBuffer();
                Update(deltaSeconds, snapshot);
                Draw();
                _rc.SwapBuffers();
            }
        }

        private void Update(float deltaSeconds, InputSnapshot snapshot)
        {
            s_imguiRenderer.Update(deltaSeconds);
            s_imguiRenderer.OnInputUpdated(snapshot);
            Gui.DrawGui();
        }

        private void Draw()
        {
            s_imguiRenderer.Render(_rc);
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
