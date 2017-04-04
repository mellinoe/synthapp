using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SynthApp
{
    public class Sequencer : StreamingDataProvider
    {
        private uint _finalSampleGenerated;
        private uint _patternPlaybackPosition;
        private Task<float[]>[] _tasks;
        private readonly List<ChannelState> _channelStates;
        private readonly LiveNotePlayer _liveNotePlayer;

        public bool Playing { get; set; }

        public uint PlaybackPositionSamples => _patternPlaybackPosition;

        public Sequencer(LiveNotePlayer lnp, int numChannels)
        {
            _liveNotePlayer = lnp;
            _channelStates = new List<ChannelState>(numChannels);
            _tasks = new Task<float[]>[numChannels];
            for (int i = 0; i < numChannels; i++)
            {
                _channelStates.Add(new ChannelState());
            }
        }

        public void AddNewChannelState()
        {
            _channelStates.Add(new ChannelState());
            Array.Resize(ref _tasks, _tasks.Length + 1);
        }

        public void RemoveChannelState(int i)
        {
            _channelStates.RemoveAt(i);
            Array.Resize(ref _tasks, _tasks.Length - 1);
        }

        public short[] GetNextAudioChunk(uint numSamples)
        {
            float[] total = new float[numSamples];

            _liveNotePlayer.FlushKeyEvents(_channelStates, _finalSampleGenerated);

            for (int i = 0; i < _channelStates.Count; i++)
            {
                int localI = i;
                _tasks[localI] = Task.Run((() =>
                {
                    ChannelState state = _channelStates[localI];
                    if (Playing)
                    {
                        state.ClearNotesBefore(_patternPlaybackPosition);
                        Application.Instance.Project.Patterns[0].GetNextNotes(
                            _patternPlaybackPosition,
                            numSamples,
                            _finalSampleGenerated - _patternPlaybackPosition,
                            state,
                            (uint)localI,
                            true);
                    }
                    return Application.Instance.Project.Channels[localI].Play(state.Sequence, _finalSampleGenerated, numSamples);
                }));
            }
            Task.WaitAll(_tasks);
            foreach (var task in _tasks)
            {
                Util.Mix(task.Result, total, total);
            }

            short[] normalized = Util.FloatToShortNormalized(total);
            _finalSampleGenerated += numSamples;
            if (Playing)
            {
                _patternPlaybackPosition += numSamples;
            }

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
