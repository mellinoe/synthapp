using System;
using System.Collections.Generic;

namespace SynthApp
{
    public class Sequencer : StreamingDataProvider
    {
        private uint _finalSampleGenerated;
        private readonly List<ChannelState> _channelStates;

        public bool Playing { get; set; }

        public Sequencer(int numChannels)
        {
            _channelStates = new List<ChannelState>(numChannels);
            for (int i = 0; i < numChannels; i++)
            {
                _channelStates.Add(new ChannelState());
            }
        }

        public void AddNewChannelState()
        {
            _channelStates.Add(new ChannelState());
        }

        public short[] GetNextAudioChunk(uint numSamples)
        {
            if (!Playing)
            {
                return new short[numSamples];
            }

            uint start = _finalSampleGenerated;
            float[] total = new float[numSamples];

            for (int i = 0; i < _channelStates.Count; i++)
            {
                ChannelState state = _channelStates[i];
                state.ClearNotesBefore(PatternTime.Samples(_finalSampleGenerated, Globals.SampleRate, Globals.BeatsPerMinute));
                Application.Instance.Project.Patterns[0].GetNextNotes(_finalSampleGenerated, numSamples, state, (uint)i, true);
                float[] channelOut = Application.Instance.Project.Channels[i].Play(state.Sequence, _finalSampleGenerated, numSamples);
                Util.Mix(total, channelOut, total);
            }

            short[] normalized = Util.FloatToShortNormalized(total);
            _finalSampleGenerated = start + numSamples;

            return normalized;
        }

        public void SeekTo(uint sample)
        {
            _finalSampleGenerated = sample;
        }

        public uint GetTotalSamples()
        {
            return (uint)(Application.Instance.Project.Patterns[0].CalculateFinalNoteEndTime().TotalBeats * Globals.SamplesPerBeat);
        }

        public void Stop()
        {
            Playing = false;
            SeekTo(0u);
            foreach (var cs in _channelStates)
            {
                cs.ClearAll();
            }
        }
    }
}
