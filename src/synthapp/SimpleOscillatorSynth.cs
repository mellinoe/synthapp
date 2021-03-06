﻿using System;

namespace SynthApp
{
    public class SimpleOscillatorSynth : Channel
    {
        private SimpleWaveformGenerator _generator = new SimpleWaveformGenerator(Globals.SampleRate);

        public SimpleWaveformGenerator Generator => _generator;

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
            Generator.Frequency = note.Frequency;
            Generator.Generate(samples, bufferStartIndex, numSamples, phaseStartSample, Gain * note.Velocity);
        }
    }
}
