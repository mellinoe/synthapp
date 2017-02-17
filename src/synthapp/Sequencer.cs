using System;
using System.Collections.Generic;

namespace SynthApp
{
    public class Sequencer : StreamingDataProvider
    {
        private List<Channel> _channels = new List<Channel>();
        private Pattern _pattern;

        private uint _finalChunkGenerated;

        public Pattern Pattern => _pattern;

        public IReadOnlyList<Channel> Channels => _channels;

        public Sequencer()
        {
            var tri = new SimpleOscillatorSynth();
            tri.Generator.Type = SimpleWaveformGenerator.WaveformType.Square;
            _channels.Add(tri);

            var saw = new SimpleOscillatorSynth();
            saw.Generator.Type = SimpleWaveformGenerator.WaveformType.Sawtooth;
            _channels.Add(saw);

            var sine = new SimpleOscillatorSynth();
            sine.Generator.Type = SimpleWaveformGenerator.WaveformType.Sine;
            _channels.Add(sine);

            var kick = new WaveSampler(@"E:\Audio\vengeance essential club sounds-3\vengeance essential club sounds-3\VEC3 Bassdrums\VEC3 Clubby Kicks\VEC3 Bassdrums Clubby 001.wav");
            _channels.Add(kick);
            kick.Gain = 0.85f;

            _pattern = new Pattern(_channels);

            NoteSequence triPattern = new NoteSequence();
            triPattern.Notes.Add(new Note(PatternTime.Steps(2), PatternTime.Steps(2), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(6), PatternTime.Steps(2), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(10), PatternTime.Steps(2), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(14), PatternTime.Steps(1), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(15), PatternTime.Steps(1), new Pitch(PitchClass.A, 3)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(18), PatternTime.Steps(2), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(22), PatternTime.Steps(2), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(26), PatternTime.Steps(2), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(30), PatternTime.Steps(1), new Pitch(PitchClass.A, 2)));
            triPattern.Notes.Add(new Note(PatternTime.Steps(31), PatternTime.Steps(1), new Pitch(PitchClass.A, 3)));
            _pattern.NoteSequences[0] = triPattern;

            NoteSequence sawPattern = new NoteSequence();
            sawPattern.Notes.Add(new Note(PatternTime.Steps(0), PatternTime.Steps(2), new Pitch(PitchClass.CSharp, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(4), PatternTime.Steps(2), new Pitch(PitchClass.E, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(8), PatternTime.Steps(2), new Pitch(PitchClass.CSharp, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(12), PatternTime.Steps(2), new Pitch(PitchClass.E, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(14), PatternTime.Steps(1), new Pitch(PitchClass.FSharp, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(15), PatternTime.Steps(1), new Pitch(PitchClass.G, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(18), PatternTime.Steps(1), new Pitch(PitchClass.G, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(20), PatternTime.Steps(1), new Pitch(PitchClass.G, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(22), PatternTime.Steps(1), new Pitch(PitchClass.G, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(24), PatternTime.Steps(3), new Pitch(PitchClass.FSharp, 4)));
            sawPattern.Notes.Add(new Note(PatternTime.Steps(28), PatternTime.Steps(3), new Pitch(PitchClass.E, 4)));
            _pattern.NoteSequences[1] = sawPattern;

            NoteSequence sinePattern = new NoteSequence();
            sinePattern.Notes.Add(new Note(PatternTime.Steps(0), PatternTime.Steps(2), new Pitch(PitchClass.CSharp, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(4), PatternTime.Steps(2), new Pitch(PitchClass.E, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(8), PatternTime.Steps(2), new Pitch(PitchClass.CSharp, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(12), PatternTime.Steps(2), new Pitch(PitchClass.E, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(14), PatternTime.Steps(1), new Pitch(PitchClass.FSharp, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(15), PatternTime.Steps(1), new Pitch(PitchClass.G, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(18), PatternTime.Steps(1), new Pitch(PitchClass.G, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(20), PatternTime.Steps(1), new Pitch(PitchClass.G, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(22), PatternTime.Steps(1), new Pitch(PitchClass.G, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(24), PatternTime.Steps(3), new Pitch(PitchClass.A, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(28), PatternTime.Steps(1), new Pitch(PitchClass.E, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(29), PatternTime.Steps(1), new Pitch(PitchClass.D, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(30), PatternTime.Steps(1), new Pitch(PitchClass.CSharp, 6)));
            sinePattern.Notes.Add(new Note(PatternTime.Steps(31), PatternTime.Steps(1), new Pitch(PitchClass.B, 5)));
            _pattern.NoteSequences[2] = sinePattern;

            NoteSequence kicks = new NoteSequence();
            kicks.Notes.Add(new Note(new PatternTime(0, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(4, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(8, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(12, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(16, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(20, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(24, 0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(new PatternTime(28, 0), PatternTime.Steps(2), Pitch.MiddleC));
            _pattern.NoteSequences[3] = kicks;

            _pattern.Duration = PatternTime.Beats(8);
        }

        public short[] GetNextAudioChunk(uint numSamples)
        {
            uint start = _finalChunkGenerated;
            uint totalSamplesInPattern = (uint)(_pattern.Duration.TotalBeats * Globals.SamplesPerBeat);
            float[] total = new float[numSamples];
            bool wrapped = false;
            uint beginningSamples = 0;

            if (totalSamplesInPattern - start < numSamples)
            {
                // Need to wrap around.

                wrapped = true;
                uint endSamples = totalSamplesInPattern - start;
                beginningSamples = numSamples - endSamples;

                for (int i = 0; i < _channels.Count; i++)

                {
                    Channel channel = _channels[i];
                    NoteSequence pattern = _pattern.NoteSequences[i];

                    float[] channelOut_End = null;
                    channelOut_End = channel.Play(pattern, start, endSamples);

                    float[] channelOut_Beginning = null;
                    channelOut_Beginning = channel.Play(pattern, 0, beginningSamples);

                    float[] channelOut = new float[total.Length];
                    channelOut_End.CopyTo(channelOut, 0);
                    channelOut_Beginning.CopyTo(channelOut, channelOut_End.Length);
                    Util.Mix(channelOut, total, total);
                }
            }
            else
            {
                for (int i = 0; i < _channels.Count; i++)
                {
                    Channel channel = _channels[i];
                    NoteSequence pattern = _pattern.NoteSequences[i];
                    float[] channelOut = channel.Play(pattern, start, numSamples);
                    Util.Mix(channelOut, total, total);
                }
            }

            short[] normalized = Util.FloatToShortNormalized(total);

            _finalChunkGenerated = start + numSamples;
            if (wrapped)
            {
                _finalChunkGenerated = beginningSamples;
            }

            return normalized;
        }

        public void SeekTo(uint sample)
        {
            _finalChunkGenerated = sample;
        }

        public uint GetTotalSamples()
        {
            return (uint)(_pattern.Duration.TotalBeats * Globals.SamplesPerBeat);
        }
    }
}
