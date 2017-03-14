using System;

namespace SynthApp
{
    public abstract class SignalGenerator
    {
        public uint SampleRate { get; }

        public double Frequency { get; set; }

        public SignalGenerator(uint sampleRate)
        {
            SampleRate = sampleRate;
            Frequency = 440.0;
        }

        public abstract void Generate(float[] data, uint bufferStartIndex, uint numSamples, uint phaseStartSample, float gain);
    }

    public class SimpleWaveformGenerator : SignalGenerator
    {
        public WaveformType Type { get; set; } = WaveformType.Sine;

        public double PhaseOffset { get; set; } = 0.0;

        public float Gain { get; set; } = 1f;

        public double PitchScale { get; set; } = 1.0;

        public SimpleWaveformGenerator(uint sampleRate) : base(sampleRate)
        {
        }

        public sealed override void Generate(float[] data, uint bufferStartIndex, uint numSamples, uint phaseStartSample, float gain)
        {
            for (int i = 0; i < numSamples; i++)
            {
                double t = (i + phaseStartSample) * (Frequency * PitchScale) / SampleRate;
                t += PhaseOffset;
                data[i + bufferStartIndex] += Sample(t) * gain * Gain;
            };
        }

        private float Sample(double t)
        {
            switch (Type)
            {
                case WaveformType.Sine:
                    {
                        return (float)Math.Sin(2 * Math.PI * t);
                    }
                case WaveformType.Triangle:
                    {
                        double a = 1;
                        double saw = 2 * ((t / a) - Math.Floor(0.5 + (t / a)));
                        return (float)((Math.Abs(saw) * 2) - 1);
                    }
                case WaveformType.Square:
                    {
                        return (float)Math.Sign(Math.Sin(2 * Math.PI * t));
                    }
                case WaveformType.Sawtooth:
                    {
                        double a = 1;
                        return (float)(2 * ((t / a) - Math.Floor(0.5 + (t / a))));
                    }
                default:
                    throw new InvalidOperationException();
            }
        }

        public enum WaveformType
        {
            Sine,
            Triangle,
            Square,
            Sawtooth
        }
    }
}
