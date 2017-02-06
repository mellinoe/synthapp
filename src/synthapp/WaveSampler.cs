using System;
using System.IO;

namespace SynthApp
{
    public class WaveSampler : Channel
    {
        private WaveFile _waveFile;

        public WaveSampler(string waveFilePath)
        {
            LoadNewWaveFile(waveFilePath);
        }

        public void LoadNewWaveFile(string waveFilePath)
        {
            using (FileStream fs = File.OpenRead(waveFilePath))
            {
                _waveFile = new WaveFile(fs);
            }

            if ((uint)_waveFile.Frequency != Globals.SampleRate)
            {
                throw new InvalidOperationException("Sample rate of " + _waveFile.Frequency + " does not match one in use.");
            }
        }

        public uint TotalSamples
        {
            get
            {
                byte[] data = _waveFile.Data;
                switch (_waveFile.Format)
                {
                    case OpenTK.Audio.OpenAL.ALFormat.Mono8:
                        return (uint)data.Length;
                    case OpenTK.Audio.OpenAL.ALFormat.Mono16:
                        return (uint)(data.Length / 2);
                    case OpenTK.Audio.OpenAL.ALFormat.Stereo8:
                        return (uint)(data.Length / 2);
                    case OpenTK.Audio.OpenAL.ALFormat.Stereo16:
                        return (uint)(data.Length / 4);
                    default:
                        throw new NotImplementedException($"This audio format ({_waveFile.Format}) is not supported.");
                }
            }
        }

        public override float[] Play(Pattern p, uint startSample, uint numSamples)
        {
            uint endSample = startSample + numSamples;
            float[] samples = new float[numSamples];

            foreach (Note note in p.Notes)
            {
                uint noteStartSample = (uint)(note.StartTime.TotalBeats * Globals.SamplesPerBeat);
                uint noteDurationSamples = TotalSamples;
                uint noteEndSample = noteStartSample + noteDurationSamples;

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

            return samples;
        }

        private void AddNote(float[] samples, Note note, uint bufferStartIndex, uint numSamples, uint phaseStartSample)
        {
            for (uint i = 0; i < numSamples; i++)
            {
                float sample = GetSample(phaseStartSample + i);
                samples[bufferStartIndex + i] += sample * Gain * note.Velocity;
            }
        }

        private float GetSample(uint sampleNum)
        {
            byte[] data = _waveFile.Data;
            switch (_waveFile.Format)
            {
                case OpenTK.Audio.OpenAL.ALFormat.Mono8:
                    return (float)data[sampleNum] / byte.MaxValue;
                case OpenTK.Audio.OpenAL.ALFormat.Mono16:
                    {
                        unchecked
                        {
                            byte a = data[sampleNum * 2];
                            byte b = data[sampleNum * 2 + 1];

                            short value = (short)((b << 8) | a);
                            return (float)value / short.MaxValue;
                        }
                    }
                case OpenTK.Audio.OpenAL.ALFormat.Stereo8:
                    return (float)data[sampleNum * 2] / byte.MaxValue;
                case OpenTK.Audio.OpenAL.ALFormat.Stereo16:
                    {
                        unchecked
                        {
                            byte a = data[sampleNum * 4];
                            byte b = data[sampleNum * 4 + 1];
                            short value = (short)((b << 8) | a);
                            return (float)value / short.MaxValue;
                        }
                    }

                default:
                    throw new NotImplementedException($"This audio format ({_waveFile.Format}) is not supported.");
            }
        }
    }
}
