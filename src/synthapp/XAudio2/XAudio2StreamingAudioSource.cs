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
        private readonly uint _chunks;

        private AutoResetEvent _bufferFinishedEvent = new AutoResetEvent(false);

        private AudioBuffer[] _audioBuffers;
        private DataStream[] _dataStreams;
        private int _nextBufferIndex = 0;

        public XAudio2StreamingAudioSource(SharpDX.XAudio2.XAudio2 xa2, StreamingDataProvider dataProvider, uint bufferedSamples)
        {
            _xa2 = xa2;
            DataProvider = dataProvider;
            _bufferedSamples = bufferedSamples;
            _chunks = (uint)Math.Ceiling((double)_bufferedSamples / _chunkSizeInSamples);
            WaveFormat format = new WaveFormat((int)Globals.SampleRate, 1);
            _sourceVoice = new SourceVoice(xa2, format, VoiceFlags.None, true);
            _sourceVoice.BufferEnd += OnSourceVoiceBufferEnd;

            _audioBuffers = new AudioBuffer[_chunks];
            _dataStreams = new DataStream[_chunks];
            for (int i = 0; i < _chunks; i++)
            {
                _audioBuffers[i] = new AudioBuffer();
                _dataStreams[i] = new DataStream((int)(_bufferedSamples * sizeof(short)), true, true);
            }
        }

        public StreamingDataProvider DataProvider { get; set; }

        public uint SamplesProcessed => throw new NotImplementedException();

        public void Play()
        {
            for (uint i = 0; i < _chunks; i++)
            {
                RefillAndQueueNextBuffer();
            }

            _playing = true;
            _sourceVoice.Start();
            Task.Factory.StartNew(() => AudioFillLoop(), TaskCreationOptions.LongRunning);
        }

        private void RefillAndQueueNextBuffer()
        {
            int nextBufferIndex = _nextBufferIndex;
            _nextBufferIndex = (_nextBufferIndex + 1) % (int)_chunks;
            AudioBuffer buffer = _audioBuffers[nextBufferIndex];
            DataStream ds = _dataStreams[nextBufferIndex];
            ds.Position = 0;
            RefillAndQueueBuffer(buffer, ds);
        }

        private unsafe void AudioFillLoop()
        {
            while (_playing)
            {
                _bufferFinishedEvent.WaitOne();
                RefillAndQueueNextBuffer();
            }
        }

        private void OnSourceVoiceBufferEnd(IntPtr buffer)
        {
            _bufferFinishedEvent.Set();
        }

        private unsafe void RefillAndQueueBuffer(AudioBuffer buffer, DataStream ds)
        {
            uint totalSampleBytes = (uint)(_chunkSizeInSamples * sizeof(short));
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