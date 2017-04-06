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

        private readonly List<ActivePatternItem> _activePatterns = new List<ActivePatternItem>();

        public bool Playing { get; set; }

        public uint PlaybackPositionSamples => _patternPlaybackPosition;

        public PlaybackMode PlaybackMode { get; set; } = PlaybackMode.Pattern;

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

            UpdateActivePatterns(_patternPlaybackPosition, numSamples);
            for (int i = 0; i < _channelStates.Count; i++)
            {
                int localI = i;
                _tasks[localI] = Task.Run((() =>
                {
                    ChannelState state = _channelStates[localI];
                    if (Playing)
                    {
                        state.ClearNotesBefore(_patternPlaybackPosition);
                        foreach (ActivePatternItem api in _activePatterns)
                        {
                            api.Pattern.GetNextNotes(
                                _patternPlaybackPosition,
                                numSamples,
                                _finalSampleGenerated - _patternPlaybackPosition,
                                PlaybackMode == PlaybackMode.Song ? api.SampleStart : 0u,
                                state,
                                (uint)localI,
                                PlaybackMode == PlaybackMode.Pattern);
                        }
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
            return (uint)(Application.Instance.SelectedPattern.CalculateFinalNoteEndTime().TotalBeats * Globals.SamplesPerBeat);
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

        private void UpdateActivePatterns(uint startSample, uint numSamples)
        {
            _activePatterns.Clear();
            if (PlaybackMode == PlaybackMode.Pattern)
            {
                _activePatterns.Add(new ActivePatternItem() { Pattern = Application.Instance.SelectedPattern });
            }
            else
            {
                Playlist playlist = Application.Instance.Project.SongPlaylist;
                ulong chunkStartSamples = startSample;
                ulong chunkEndSamples = chunkStartSamples + numSamples;
                foreach (PlaylistEntry entry in playlist.Entries)
                {
                    Pattern pattern = Application.Instance.Project.GetOrCreatePattern(entry.PatternIndex);
                    uint patternSampleOffset = (uint)(entry.SongStepOffset * Globals.SamplesPerStep);
                    ulong patternStartSamples = patternSampleOffset;
                    ulong patternEndSamples = patternStartSamples + (ulong)(pattern.CalculateFinalNoteEndTime().ToSamplesAuto());
                    if ((chunkStartSamples >= patternStartSamples && chunkStartSamples < patternEndSamples)
                        || (chunkEndSamples >= patternStartSamples && chunkEndSamples < patternEndSamples)
                        || (chunkStartSamples <= patternStartSamples && chunkEndSamples >= patternEndSamples))
                    {
                        _activePatterns.Add(new ActivePatternItem() { Pattern = pattern, SampleStart = patternSampleOffset });
                    }
                }
            }
        }

        private struct ActivePatternItem
        {
            public Pattern Pattern;
            public uint SampleStart;
        }
    }

    public enum PlaybackMode
    {
        Pattern = 0,
        Song = 1,
    }
}
