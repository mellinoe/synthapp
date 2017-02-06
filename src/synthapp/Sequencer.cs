using System;
using System.Collections.Generic;

namespace SynthApp
{
    public class Sequencer : StreamingDataProvider
    {
        private List<Channel> _channels = new List<Channel>();
        private List<Pattern> _patterns = new List<Pattern>();

        private uint _finalChunkGenerated;

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

            Pattern triPattern = new Pattern();
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
            triPattern.Duration = PatternTime.Steps(32);
            _patterns.Add(triPattern);

            Pattern sawPattern = new Pattern();
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
            sawPattern.Duration = PatternTime.Steps(32);
            _patterns.Add(sawPattern);

            Pattern sinePattern = new Pattern();
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
            sinePattern.Duration = PatternTime.Steps(32);
            _patterns.Add(sinePattern);

            Pattern kicks = new Pattern();
            kicks.Notes.Add(new Note(PatternTime.Steps(0), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(PatternTime.Steps(4), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(PatternTime.Steps(8), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(PatternTime.Steps(12), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(PatternTime.Steps(16), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(PatternTime.Steps(20), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(PatternTime.Steps(24), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Notes.Add(new Note(PatternTime.Steps(28), PatternTime.Steps(2), Pitch.MiddleC));
            kicks.Duration = PatternTime.Steps(32);
            _patterns.Add(kicks);
        }

        public short[] GetNextAudioChunk(uint numSamples)
        {
            uint start = _finalChunkGenerated;
            uint totalSamplesInPattern = (uint)(_patterns[0].Duration.TotalBeats * Globals.SamplesPerBeat);
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
                    Pattern pattern = _patterns[i];

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
                    Pattern pattern = _patterns[i];
                    float[] channelOut = channel.Play(pattern, start, numSamples);
                    Util.Mix(channelOut, total, total);
                }
            }

            short[] normalized = new short[total.Length];
            for (int i = 0; i < total.Length; i++)
            {
                normalized[i] = Util.DoubleToShort(total[i]);
            }

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
            return (uint)(_patterns[0].Duration.TotalBeats * Globals.SamplesPerBeat);
        }
    }
}
