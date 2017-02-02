using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;

namespace SynthApp
{
    public class AudioEngine : IDisposable
    {
        private AudioContext _alAudioContext;
        private AudioTrack _track;
        private int _alBufferID;
        private int _alSourceID;

        public AudioEngine()
        {
            _alAudioContext = new AudioContext();
            _alAudioContext.MakeCurrent();
            _alBufferID = AL.GenBuffer();

            _alSourceID = AL.GenSource();
            AL.Source(_alSourceID, ALSourcef.Gain, 1.0f);
            AL.Source(_alSourceID, ALSource3f.Position, 0f, 0f, 0f);

            AL.Listener(ALListener3f.Position, 0f, 0f, 0f);
        }

        public void Dispose()
        {
            _alAudioContext.Dispose();
        }

        public void SetAudioTrack(AudioTrack track)
        {
            Stop();
            _track = track;
            AL.BindBufferToSource(_alSourceID, 0);
            AL.BufferData(_alBufferID, ALFormat.Mono8, track.Data, track.Data.Length, (int)track.Frequency);
        }

        public void Play()
        {
            AL.BindBufferToSource(_alSourceID, _alBufferID);
            AL.SourcePlay(_alSourceID);
        }

        public void Stop()
        {
            AL.SourceStop(_alSourceID);
        }
    }
}
