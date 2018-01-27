using System.Collections.Generic;
using Veldrid;

namespace SynthApp
{
    public class KeyboardLivePlayInput
    {
        private readonly LiveNotePlayer _livePlayer;
        private readonly StreamingAudioSource _streamSource;
        private readonly Dictionary<Key, Pitch> _keyMap = new Dictionary<Key, Pitch>()
        {
            { Key.Z, new Pitch(PitchClass.C, 3) },
            { Key.S, new Pitch(PitchClass.CSharp, 3) },
            { Key.X, new Pitch(PitchClass.D, 3) },
            { Key.D, new Pitch(PitchClass.DSharp, 3) },
            { Key.C, new Pitch(PitchClass.E, 3) },
            { Key.V, new Pitch(PitchClass.F, 3) },
            { Key.G, new Pitch(PitchClass.FSharp, 3) },
            { Key.B, new Pitch(PitchClass.G, 3) },
            { Key.H, new Pitch(PitchClass.GSharp, 3) },
            { Key.N, new Pitch(PitchClass.A, 3) },
            { Key.J, new Pitch(PitchClass.ASharp, 3) },
            { Key.M, new Pitch(PitchClass.B, 3) },
            { Key.Comma, new Pitch(PitchClass.C, 4) },
            { Key.L, new Pitch(PitchClass.CSharp, 4) },
            { Key.Period, new Pitch(PitchClass.D, 4) },
            { Key.Semicolon, new Pitch(PitchClass.DSharp, 4) },
            { Key.Slash, new Pitch(PitchClass.E, 4) },
            { Key.Q, new Pitch(PitchClass.C, 4) },
            { Key.Number2, new Pitch(PitchClass.CSharp, 4) },
            { Key.W, new Pitch(PitchClass.D, 4) },
            { Key.Number3, new Pitch(PitchClass.DSharp, 4) },
            { Key.E, new Pitch(PitchClass.E, 4) },
            { Key.R, new Pitch(PitchClass.F, 4) },
            { Key.Number5, new Pitch(PitchClass.FSharp, 4) },
            { Key.T, new Pitch(PitchClass.G, 4) },
            { Key.Number6, new Pitch(PitchClass.GSharp, 4) },
            { Key.Y, new Pitch(PitchClass.A, 4) },
            { Key.Number7, new Pitch(PitchClass.ASharp, 4) },
            { Key.U, new Pitch(PitchClass.B, 4) },
            { Key.I, new Pitch(PitchClass.C, 5) },
            { Key.Number9, new Pitch(PitchClass.CSharp, 5) },
            { Key.O, new Pitch(PitchClass.D, 5) },
            { Key.Number0, new Pitch(PitchClass.DSharp, 5) },
            { Key.P, new Pitch(PitchClass.E, 5) },
            { Key.BracketLeft, new Pitch(PitchClass.F, 5) },
            { Key.Plus, new Pitch(PitchClass.FSharp, 5) },
            { Key.BracketRight, new Pitch(PitchClass.G, 5) },
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
                if (Application.Instance.Input.GetKeyDown(kvp.Key))
                {
                    Pitch pitch = kvp.Value;
                    _livePlayer.AddKeyEvent(c, pitch, true);
                }
                if (Application.Instance.Input.GetKeyReleased(kvp.Key))
                {
                    Pitch pitch = kvp.Value;
                    _livePlayer.AddKeyEvent(c, pitch, false);
                }
            }
        }

        private uint GetCurrentPlaybackSamples()
        {
            return _streamSource.SamplesProcessed;
        }
    }
}
