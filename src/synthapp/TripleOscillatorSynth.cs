using System;

namespace SynthApp
{
    public class TripleOscillatorSynth : Channel
    {
        private SimpleWaveformGenerator _generator1 = new SimpleWaveformGenerator(Globals.SampleRate) { Gain = 0.3f };
        private SimpleWaveformGenerator _generator2 = new SimpleWaveformGenerator(Globals.SampleRate) { Gain = 0.3f };
        private SimpleWaveformGenerator _generator3 = new SimpleWaveformGenerator(Globals.SampleRate) { Gain = 0.3f };

        public SimpleWaveformGenerator Generator1 => _generator1;
        public SimpleWaveformGenerator Generator2 => _generator2;
        public SimpleWaveformGenerator Generator3 => _generator3;

        public override float[] Play(MaterializedNoteSequence p, uint startSample, uint numSamples)
        {
            uint endSample = startSample + numSamples;
            float[] samples = new float[numSamples];
            if (!Muted)
            {
                foreach (MaterializedNote note in p.Notes)
                {
                    uint noteStartSample = note.StartSample;
                    uint noteDurationSamples = note.SampleCount;
                    uint noteEndSample = note.EndSample;

                    if ((noteEndSample >= startSample && noteEndSample <= endSample)
                        || (noteStartSample >= startSample && noteStartSample <= endSample)
                        || (noteStartSample <= startSample && noteEndSample >= endSample))
                    {
                        uint effectiveStartSample = Util.Max(startSample, noteStartSample);
                        uint effectiveEndSample = Util.Min(endSample, noteEndSample);
                        uint effectiveNoteDurationSamples = effectiveEndSample - effectiveStartSample;
                        uint effectivePhase = effectiveStartSample - noteStartSample;

                        AddNote(samples, note, effectiveStartSample - startSample, effectiveNoteDurationSamples, effectivePhase);
                    }
                }
            }

            return samples;
        }

        private void AddNote(float[] samples, MaterializedNote note, uint bufferStartIndex, uint numSamples, uint phaseStartSample)
        {
            Generator1.Frequency = note.Frequency;
            Generator1.Generate(samples, bufferStartIndex, numSamples, phaseStartSample, Gain * note.Velocity);

            Generator2.Frequency = note.Frequency;
            Generator2.Generate(samples, bufferStartIndex, numSamples, phaseStartSample, Gain * note.Velocity);

            Generator3.Frequency = note.Frequency;
            Generator3.Generate(samples, bufferStartIndex, numSamples, phaseStartSample, Gain * note.Velocity);
        }
    }
}
