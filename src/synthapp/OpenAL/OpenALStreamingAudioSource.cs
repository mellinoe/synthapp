using System;
using OpenTK.Audio.OpenAL;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SynthApp.OpenAL
{
    public class OpenALStreamingAudioSource : StreamingAudioSource
    {
        private readonly int _sid;
        private bool _playing;
        private StreamingDataProvider _dataProvider;
        private uint _chunkSizeInSamples = 250;
        private readonly List<int> _cachedBufferIDs = new List<int>();
        private int _currentBufferSamplesProcessed;
        private uint _samplesProcessed;

        public uint SamplesProcessed => (uint)(_samplesProcessed + _currentBufferSamplesProcessed);

        public uint BufferedSamples { get; set; }

        public StreamingDataProvider DataProvider { get => _dataProvider; set => _dataProvider = value; }

        public OpenALStreamingAudioSource(StreamingDataProvider provider, uint bufferedSamples)
        {
            _sid = AL.GenSource();
            DataProvider = provider;
            BufferedSamples = bufferedSamples;
        }

        private unsafe void AudioFillLoop()
        {
            while (_playing)
            {
                AL.GetSource(_sid, ALGetSourcei.BuffersProcessed, out int buffersProcessed);
                if (buffersProcessed > 0)
                {
                    uint* processedIDs = stackalloc uint[buffersProcessed];
                    AL.SourceUnqueueBuffers((uint)_sid, buffersProcessed, processedIDs);
                    for (uint i = 0; i < buffersProcessed; i++)
                    {
                        int bufferID = (int)processedIDs[i];
                        RefillAndQueueBuffer(bufferID);
                    }

                    _samplesProcessed += (uint)(buffersProcessed * _chunkSizeInSamples);
                }

                AL.GetSource(_sid, ALGetSourcei.SampleOffset, out _currentBufferSamplesProcessed);

                if (_playing && AL.GetSourceState(_sid) != ALSourceState.Playing)
                {
                    AL.SourcePlay(_sid);
                }
            }

            _samplesProcessed = 0;
            _currentBufferSamplesProcessed = 0;
        }

        public void Play()
        {
            uint chunks = (uint)Math.Ceiling((double)BufferedSamples / _chunkSizeInSamples);
            for (uint i = 0; i < chunks; i++)
            {
                int bid = AL.GenBuffer();
                RefillAndQueueBuffer(bid);
            }

            _playing = true;
            AL.SourcePlay(_sid);
            Task.Factory.StartNew(() => AudioFillLoop(), TaskCreationOptions.LongRunning);
        }

        public unsafe void Stop()
        {
            // NOTE: This is never called. I think it is wrong.
            _playing = false;
            AL.SourceStop(_sid);
            AL.GetSource(_sid, ALGetSourcei.BuffersQueued, out int count);
            if (count > 0)
            {
                uint* bufferIDs = stackalloc uint[count];
                AL.SourceUnqueueBuffers(_sid, count);
                for (uint i = 0; i < count; i++)
                {
                    CacheBuffer(bufferIDs[i]);
                }
            }

            _dataProvider.SeekTo(0);
            _samplesProcessed = 0;
        }

        private void CacheBuffer(uint id)
        {
            _cachedBufferIDs.Add((int)id);
        }

        private void RefillAndQueueBuffer(int bufferID)
        {
            short[] data = GetNextAudioChunk();
            AL.BufferData(bufferID, ALFormat.Mono16, data, data.Length * sizeof(short), (int)Globals.SampleRate);
            AL.SourceQueueBuffer(_sid, bufferID);
        }

        private short[] GetNextAudioChunk()
        {
            return DataProvider.GetNextAudioChunk(_chunkSizeInSamples);
        }
    }
}
