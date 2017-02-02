using System;
using System.Diagnostics;

namespace SynthApp
{
    public class AudioTrack
    {
        private readonly byte[] _audioData;
        private readonly uint _sampleRate;
        private readonly double _lengthInSeconds;

        public byte[] Data => _audioData;

        public uint Frequency => _sampleRate;

        public AudioTrack(double frequency, uint sampleRate, double lengthInSeconds, Shape shape)
        {
            _audioData = CreateSineAudioData(frequency, sampleRate, lengthInSeconds, shape);
            _sampleRate = sampleRate;
            _lengthInSeconds = lengthInSeconds;
        }

        private byte[] CreateSineAudioData(double frequency, uint sampleRate, double lengthInSeconds, Shape shape)
        {
            byte[] data = new byte[(int)Math.Ceiling(sampleRate * lengthInSeconds)];
            Func<double, double> waveFunc = GetWaveFunc(shape);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Normalize(waveFunc(i * frequency / (double)sampleRate));
            }

            return data;
        }

        private Func<double, double> GetWaveFunc(Shape shape)
        {
            switch (shape)
            {
                default:
                case Shape.Sine:
                    return t => Math.Sin(2 * Math.PI * t);
                case Shape.Saw:
                    {
                        double a = 1;
                        return t =>
                        {
                            return 2 * ((t / a) - Math.Floor(0.5 + (t / a)));
                        };
                    }
                case Shape.Square:
                    return t =>
                    {
                        return Math.Sign(Math.Sin(2 * Math.PI * t));
                    };
                case Shape.Triangle:
                    {
                        double a = 1;
                        return t =>
                        {
                            return Math.Abs(2 * ((t / a) - Math.Floor(0.5 + (t / a))));
                        };
                    }
            }
        }

        private byte Normalize(double d)
        {
            Debug.Assert(d >= -1 && d <= 1);
            return (byte)((d * 0.5 + 0.5) * 255.0);
        }

        public enum Shape
        {
            Sine,
            Saw,
            Square,
            Triangle
        }
    }
}
