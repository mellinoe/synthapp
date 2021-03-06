﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SynthApp
{
    public class AudioStreamCombiner : StreamingDataProvider
    {
        private List<StreamingDataProvider> _dataProviders = new List<StreamingDataProvider>(2);

        public float Gain { get; set; } = 1f;

        public void Add(StreamingDataProvider provider)
        {
            _dataProviders.Add(provider);
        }

        public bool Remove(StreamingDataProvider provider)
        {
            return _dataProviders.Remove(provider);
        }

        public short[] GetNextAudioChunk(uint numSamples)
        {
            short[] data = new short[numSamples];
            foreach (StreamingDataProvider provider in _dataProviders)
            {
                short[] providerData = provider.GetNextAudioChunk(numSamples);
                MixClamped(data, providerData, Gain);
            }

            return data;
        }

        private void MixClamped(short[] dest, short[] added, float gain)
        {
            Debug.Assert(dest.Length == added.Length);
            for (int i = 0; i < dest.Length; i++)
            {
                int value = (int)Math.Round((dest[i] + added[i]) * Gain);
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
