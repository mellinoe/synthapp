using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SynthApp
{
    public class AudioStreamCombiner : StreamingDataProvider
    {
        private List<StreamingDataProvider> _dataProviders = new List<StreamingDataProvider>(2);

        public void Add(StreamingDataProvider provider)
        {
            _dataProviders.Add(provider);
        }

        public bool Remove(StreamingDataProvider provider)
        {
            return _dataProviders.Remove(provider);
        }

        public void GetNextAudioChunk(short[] data, uint numSamples)
        {
            short[] providerData = Util.Rent<short>(numSamples);
            foreach (StreamingDataProvider provider in _dataProviders)
            {
                provider.GetNextAudioChunk(providerData, numSamples);
                MixClamped(data, providerData);
            }
            Util.Return(providerData);
        }

        private void MixClamped(short[] dest, short[] added)
        {
            Debug.Assert(dest.Length == added.Length);
            for (int i = 0; i < dest.Length; i++)
            {
                int value = dest[i] + added[i];
                dest[i] = (short)Util.Clamp(value, short.MinValue, short.MaxValue);
            }
        }

        public void SeekTo(uint sample)
        {
            foreach (var provider in _dataProviders)
            {
                provider.SeekTo(sample);
            }
        }
    }
}
