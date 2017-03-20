using System;
using SharpDX.XAudio2;
using SharpDX.Mathematics.Interop;

namespace SynthApp.XAudio2
{
    public class XAudio2AudioEngine : AudioEngine, IDisposable
    {
        private readonly SharpDX.XAudio2.XAudio2 _xa2;
        private readonly MasteringVoice _masteringVoice;

        public XAudio2AudioEngine()
        {
            _xa2 = new SharpDX.XAudio2.XAudio2(XAudio2Flags.DebugEngine, ProcessorSpecifier.DefaultProcessor);
            _xa2.CriticalError += OnCriticalError;
            _xa2.StartEngine();
            DebugConfiguration debugConfig = new DebugConfiguration();
            debugConfig.BreakMask = (int)LogType.Warnings;
            debugConfig.TraceMask = (int)
                (LogType.Errors | LogType.Warnings | LogType.Information | LogType.Detail | LogType.ApiCalls
                | LogType.FunctionCalls | LogType.Timing | LogType.Locks | LogType.Memory | LogType.Streaming);
            debugConfig.LogThreadID = new RawBool(true);
            debugConfig.LogFileline = new RawBool(true);
            debugConfig.LogFunctionName = new RawBool(true);
            debugConfig.LogTiming = new RawBool(true);
            _xa2.SetDebugConfiguration(debugConfig, IntPtr.Zero);
            _masteringVoice = new MasteringVoice(_xa2);
        }

        private void OnCriticalError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("XAudio2 critical error: " + e.ToString());
        }

        public StreamingAudioSource CreateStreamingAudioSource(StreamingDataProvider dataProvider, uint bufferedSamples)
        {
            return new XAudio2StreamingAudioSource(_xa2, dataProvider, bufferedSamples);
        }

        public void Dispose()
        {
            _xa2.Dispose();
        }
    }
}
