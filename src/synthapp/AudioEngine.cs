using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SynthApp
{
    public class AudioEngine : IDisposable
    {
        private AudioContext _alAudioContext;
        private int _alBufferID;
        private int _alSourceID;

        public AudioEngine()
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

        public void Play()
        {
            AL.SourcePlay(_alSourceID);
    }

        public void Stop()
        {
            AL.SourceStop(_alSourceID);
        }

        public void PlayAudioData(short[] audio, uint frequency)
        {
            AL.BindBufferToSource(_alSourceID, 0);
            AL.BufferData(_alBufferID, ALFormat.Mono16, audio, audio.Length * sizeof(short), (int)frequency);
            AL.BindBufferToSource(_alSourceID, _alBufferID);
            Play();
        }

        public void PlayAudioData(byte[] audio, uint frequency)
        {
            AL.BindBufferToSource(_alSourceID, 0);
            AL.BufferData(_alBufferID, ALFormat.Mono8, audio, audio.Length * sizeof(byte), (int)frequency);
            AL.BindBufferToSource(_alSourceID, _alBufferID);
            Play();
        }
    }
}
