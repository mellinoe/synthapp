namespace SynthApp
{
    public class Sequencer : StreamingDataProvider
    {
        private uint _finalChunkGenerated;

        public bool Playing { get; set; }

        public Sequencer()
        {
        }

        public short[] GetNextAudioChunk(uint numSamples)
        {
            if (!Playing)
            {
                return new short[numSamples];
            }

            uint start = _finalChunkGenerated;
            uint totalSamplesInPattern = (uint)(Application.Instance.Project.Patterns[0].CalculateFinalNoteEndTime().TotalBeats * Globals.SamplesPerBeat);
            if (start > totalSamplesInPattern)
            {
                start = start % totalSamplesInPattern;
            }
            float[] total = new float[numSamples];
            bool wrapped = false;
            uint beginningSamples = 0;

            if (totalSamplesInPattern - start < numSamples)
            {
                // Need to wrap around.

                wrapped = true;
                uint endSamples = totalSamplesInPattern - start;
                beginningSamples = numSamples - endSamples;

                for (int i = 0; i < Application.Instance.Project.Channels.Length; i++)

                {
                    Channel channel = Application.Instance.Project.Channels[i];
                    NoteSequence pattern = Application.Instance.Project.Patterns[0].NoteSequences[i];

                    float[] channelOut_End = null;
                    channelOut_End = channel.Play(pattern, start, endSamples);

                    float[] channelOut_Beginning = null;
                    channelOut_Beginning = channel.Play(pattern, 0, beginningSamples);

                    float[] channelOut = new float[total.Length];
                    channelOut_End.CopyTo(channelOut, 0);
                    channelOut_Beginning.CopyTo(channelOut, channelOut_End.Length);
                    Util.Mix(channelOut, total, total);
                }
            }
            else
            {
                for (int i = 0; i < Application.Instance.Project.Channels.Length; i++)
                {
                    Channel channel = Application.Instance.Project.Channels[i];
                    NoteSequence pattern = Application.Instance.Project.Patterns[0].NoteSequences[i];
                    float[] channelOut = channel.Play(pattern, start, numSamples);
                    Util.Mix(channelOut, total, total);
                }
            }

            short[] normalized = Util.FloatToShortNormalized(total);

            _finalChunkGenerated = start + numSamples;
            if (wrapped)
            {
                _finalChunkGenerated = beginningSamples;
            }

            return normalized;
        }

        public void SeekTo(uint sample)
        {
            _finalChunkGenerated = sample;
        }

        public uint GetTotalSamples()
        {
            return (uint)(Application.Instance.Project.Patterns[0].CalculateFinalNoteEndTime().TotalBeats * Globals.SamplesPerBeat);
        }
    }
}
