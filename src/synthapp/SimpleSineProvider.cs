using System;

namespace SynthApp
{
    public class SimpleSineProvider : StreamingDataProvider
    {
        private uint _currentSample;

        public double Frequency { get; set; } = 440.0;

        public short[] GetNextAudioChunk(uint numSamples)
        {
            short[] data = new short[numSamples];
            for (uint i = 0; i < numSamples; i++)
            {
                double sample = Math.Sin((i + _currentSample) * Frequency * 2 * Math.PI / Globals.SampleRate);
                data[i] = Util.DoubleToShort(sample);
            }

            _currentSample += numSamples;
            return data;
        }

        public uint GetTotalSamples()
        {
            return 9999999;
        }

        public void SeekTo(uint sample)
        {
            _currentSample = sample;
        }
    }
}
