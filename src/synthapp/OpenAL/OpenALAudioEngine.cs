using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;

namespace SynthApp.OpenAL
{
    public class OpenALAudioEngine : AudioEngine, IDisposable
    {
        private AudioContext _alAudioContext;
        private int _alBufferID;
        private int _alSourceID;

        public OpenALAudioEngine()
        {
            _alAudioContext = new AudioContext();
            _alAudioContext.MakeCurrent();
            _alBufferID = AL.GenBuffer();

            _alSourceID = AL.GenSource();
            AL.Source(_alSourceID, ALSourcef.Gain, 0.3f);
            AL.Source(_alSourceID, ALSource3f.Position, 0f, 0f, 0f);

            AL.Listener(ALListener3f.Position, 0f, 0f, 0f);

            AL.Source(_alSourceID, ALSourcei.SourceType, (int)ALSourceType.Streaming);
        }

        public void Dispose()
        {
            _alAudioContext.Dispose();
        }

        public StreamingAudioSource CreateStreamingAudioSource(StreamingDataProvider dataProvider, uint bufferedSamples)
        {
            return new OpenALStreamingAudioSource(dataProvider, bufferedSamples);
        }
    }
}
