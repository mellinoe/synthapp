using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SynthApp
{
    public class LiveNotePlayer
    {
        private ConcurrentQueue<KeyboardNoteEvent> _events = new ConcurrentQueue<KeyboardNoteEvent>();

        private HashSet<double> _currentKeys = new HashSet<double>();
        private HashSet<double> _nextKeys = new HashSet<double>();

        public void AddKeyEvent(Channel c, Pitch p, bool isKeyDown)
        {
            _events.Enqueue(new KeyboardNoteEvent(c, p, isKeyDown));
        }

        public bool IsKeyPressed(Channel c, Pitch p)
        {
            double frequency = TuningSystem.EqualTemperament.GetFrequency(p);
            return _currentKeys.Contains(frequency);
        }

        internal void FlushKeyEvents(List<ChannelState> channelStates, uint currentSample)
        {
            while (_events.TryDequeue(out KeyboardNoteEvent kne))
            {
                HandleEvent(channelStates, kne, currentSample);
            }

            UpdateKeySets(channelStates);

        }

        private void UpdateKeySets(List<ChannelState> channelStates)
        {
            HashSet<double> next = _nextKeys;
            next.Clear();
            foreach (var channelState in channelStates)
            {
                foreach (var note in channelState.KeyboardActiveNotes)
                {
                    next.Add(note.Frequency);
                }
            }

            _nextKeys = Interlocked.Exchange(ref _currentKeys, next);
        }

        private void HandleEvent(List<ChannelState> channelStates, KeyboardNoteEvent kne, uint currentSample)
        {
            int channelIndex = Application.Instance.Project.GetChannelIndex(kne.Channel);
            if (kne.KeyDown)
            {
                channelStates[channelIndex].BeginKeyboardNote(kne.Pitch, currentSample);
            }
            else
            {
                channelStates[channelIndex].EndKeyboardNote(kne.Pitch, currentSample);
            }
        }
    }

    public struct KeyboardNoteEvent
    {
        public Channel Channel;
        public Pitch Pitch;
        public bool KeyDown;

        public KeyboardNoteEvent(Channel channel, Pitch pitch, bool down)
        {
            Channel = channel;
            Pitch = pitch;
            KeyDown = down;
        }
    }
}
