using System;
using SharpDX.XAudio2;
using SharpDX.Multimedia;
using System.Threading.Tasks;
using System.Threading;
using SharpDX;
using System.Runtime.CompilerServices;

namespace SynthApp.XAudio2
{
    internal class XAudio2StreamingAudioSource : StreamingAudioSource
    {
        private readonly SharpDX.XAudio2.XAudio2 _xa2;
        private readonly uint _bufferedSamples;
        private readonly SourceVoice _sourceVoice;
        private readonly uint _chunkSizeInSamples = 250;
        private bool _playing;
        private int _bufferQueuesNeeded;
        private readonly uint _chunks;

        public XAudio2StreamingAudioSource(SharpDX.XAudio2.XAudio2 xa2, StreamingDataProvider dataProvider, uint bufferedSamples)
        {
            _xa2 = xa2;
            DataProvider = dataProvider;
            _bufferedSamples = bufferedSamples;
            _chunks = (uint)Math.Ceiling((double)_bufferedSamples / _chunkSizeInSamples);
            WaveFormat format = new WaveFormat((int)Globals.SampleRate, 1);
            _sourceVoice = new SourceVoice(xa2, format, VoiceFlags.None, true);
            _sourceVoice.BufferEnd += OnSourceVoiceBufferEnd;
        }

        public StreamingDataProvider DataProvider { get; set; }

        public uint SamplesProcessed => throw new NotImplementedException();

        public void Play()
        {
            for (uint i = 0; i < _chunks; i++)
            {
                var audioBuffer = new AudioBuffer();
                RefillAndQueueBuffer(audioBuffer);
            }

            _playing = true;
            _sourceVoice.Start();
            Task.Factory.StartNew(() => AudioFillLoop(), TaskCreationOptions.LongRunning);
        }

        private unsafe void AudioFillLoop()
        {
            while (_playing)
            {
                int queued = _sourceVoice.State.BuffersQueued;
                int needed = (int)(_chunks - queued);
                if (needed > 0)
                {
                    while (needed != 0)
                    {
                        needed--;
                        AudioBuffer buffer = new AudioBuffer();
                        RefillAndQueueBuffer(buffer);
                    }
                }

                _sourceVoice.Start();
            }
        }

        private void OnSourceVoiceBufferEnd(IntPtr buffer)
        {
            Interlocked.Increment(ref _bufferQueuesNeeded);
        }

        private unsafe void RefillAndQueueBuffer(AudioBuffer buffer)
        {
            uint totalSampleBytes = (uint)(_chunkSizeInSamples * sizeof(short));
            DataStream ds = new DataStream((int)totalSampleBytes, true, true);
            short[] data = GetNextAudioChunk();
            fixed (short* dataPtr = &data[0])
            {
                Unsafe.CopyBlock(ds.DataPointer.ToPointer(), dataPtr, totalSampleBytes);
            }
            buffer.Stream = ds;
            buffer.AudioBytes = (int)(_chunkSizeInSamples * sizeof(short));
            _sourceVoice.SubmitSourceBuffer(buffer, null);
        }

        private short[] GetNextAudioChunk()
        {
            return DataProvider.GetNextAudioChunk(_chunkSizeInSamples);
        }
    }
}