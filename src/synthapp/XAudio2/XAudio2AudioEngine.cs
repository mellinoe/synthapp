using System;

namespace SynthApp.XAudio2
{
    public class XAudio2AudioEngine : AudioEngine, IDisposable
    {
        public XAudio2AudioEngine()
        {
            
        }

        public StreamingAudioSource CreateStreamingAudioSource(StreamingDataProvider dataProvider, uint bufferedSamples)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
