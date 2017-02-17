using System;
using System.Collections.Generic;
using Veldrid.Platform;

namespace SynthApp
{
    public class KeyboardLivePlayInput
    {
        private uint _inputBufferInSamples = 5000;

        private readonly LiveNotePlayer _livePlayer;
        private readonly StreamingAudioSource _streamSource;
        private readonly Dictionary<Key, Pitch> _keyMap = new Dictionary<Key, Pitch>()
        {
            { Key.Q, new Pitch(PitchClass.C, 4) },
            { Key.W, new Pitch(PitchClass.D, 4) },
            { Key.E, new Pitch(PitchClass.E, 4) },
            { Key.R, new Pitch(PitchClass.F, 4) },
            { Key.T, new Pitch(PitchClass.G, 4) },
            { Key.Y, new Pitch(PitchClass.A, 4) },
            { Key.U, new Pitch(PitchClass.B, 4) },
            { Key.I, new Pitch(PitchClass.C, 5) },
            { Key.O, new Pitch(PitchClass.D, 5) },
            { Key.P, new Pitch(PitchClass.E, 5) },
        };

        public KeyboardLivePlayInput(LiveNotePlayer livePlayer, StreamingAudioSource streamSource)
        {
            _livePlayer = livePlayer;
            _streamSource = streamSource;
        }

        public void Play(Channel c)
        {
            foreach (var kvp in _keyMap)
            {
                if (Globals.Input.GetKeyDown(kvp.Key))
                {
                    uint startSamples = GetCurrentPlaybackSamples() + _inputBufferInSamples;
                    PatternTime start = PatternTime.Samples(startSamples, Globals.SampleRate, Globals.BeatsPerMinute);
                    PatternTime duration = PatternTime.Steps(2);
                    Note n = new Note(start, duration, kvp.Value);
                    Console.WriteLine("Adding a note at sample " + startSamples + ", which is step " + start);
                    _livePlayer.AddNote(c, n);
                }
            }
        }

        private uint GetCurrentPlaybackSamples()
        {
            return (uint)_streamSource.SamplesProcessed;
        }
    }
}
