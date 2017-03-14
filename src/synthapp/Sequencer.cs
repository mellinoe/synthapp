using System;
using System.Collections.Generic;

namespace SynthApp
{
    public class Sequencer : StreamingDataProvider
    {
        private uint _finalSampleGenerated;
        private uint _patternPlaybackPosition;
        private readonly List<ChannelState> _channelStates;
        private readonly LiveNotePlayer _liveNotePlayer;

        public bool Playing { get; set; }

        public Sequencer(LiveNotePlayer lnp, int numChannels)
        {
            _liveNotePlayer = lnp;
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
            float[] total = new float[numSamples];

            _liveNotePlayer.FlushKeyEvents(_channelStates, _finalSampleGenerated);
            if (Playing)
            {
                for (int i = 0; i < _channelStates.Count; i++)
                {
                    ChannelState state = _channelStates[i];
                    state.ClearNotesBefore(_patternPlaybackPosition);
                    Application.Instance.Project.Patterns[0].GetNextNotes(_patternPlaybackPosition, numSamples, _finalSampleGenerated - _patternPlaybackPosition, state, (uint)i, true);
                }

                _patternPlaybackPosition += numSamples;
            }

            for (int i = 0; i < _channelStates.Count; i++)
            {
                ChannelState state = _channelStates[i];
                float[] channelOut = Application.Instance.Project.Channels[i].Play(state.Sequence, _finalSampleGenerated, numSamples);
                Util.Mix(total, channelOut, total);
            }

            short[] normalized = Util.FloatToShortNormalized(total);
            _finalSampleGenerated += numSamples;

            return normalized;
        }

        public void SeekTo(uint sample)
        {
            _patternPlaybackPosition = sample;
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
