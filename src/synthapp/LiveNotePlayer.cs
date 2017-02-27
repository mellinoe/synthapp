using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SynthApp
{
    public class LiveNotePlayer : StreamingDataProvider
    {
        private uint _finalChunkGenerated;
        private ConcurrentQueue<KeyboardNoteEvent> _events = new ConcurrentQueue<KeyboardNoteEvent>();
        private Dictionary<Channel, LivePlayerChannelState> _sequences = new Dictionary<Channel, LivePlayerChannelState>();

        public uint SamplePlaybackLocation => _finalChunkGenerated;

        public void AddKeyEvent(Channel c, Pitch p, bool isKeyDown)
        {
            _events.Enqueue(new KeyboardNoteEvent(c, p, isKeyDown));
        }

        private LivePlayerChannelState GetPlayerState(Channel c)
        {
            if (!_sequences.TryGetValue(c, out LivePlayerChannelState ns))
            {
                ns = new LivePlayerChannelState();
                _sequences.Add(c, ns);
            }

            return ns;
        }

        public short[] GetNextAudioChunk(uint numSamples)
        {
            while (_events.TryDequeue(out KeyboardNoteEvent kne))
            {
                HandleEvent(kne, PatternTime.Samples(_finalChunkGenerated, Globals.SampleRate, Globals.BeatsPerMinute));
            }

            bool empty = true;
            foreach (var kvp in _sequences)
            {
                if (kvp.Value.ActiveNotes.Count != 0)
                {
                    empty = false;
                    break;
                }
            }
            if (empty)
            {
                if (_finalChunkGenerated > 0)
                {
                    _finalChunkGenerated = 0;
                    foreach (var kvp in _sequences)
                    {
                        kvp.Value.ClearAll();
                    }
                }
                return new short[numSamples];
            }

            float[] data = new float[numSamples];
            foreach (var kvp in _sequences)
            {
                Channel c = kvp.Key;
                LivePlayerChannelState lpcs = kvp.Value;

                float[] channelData = c.Play(lpcs.Sequence, _finalChunkGenerated, numSamples);
                Util.Mix(data, channelData, data);
            }

            _finalChunkGenerated += numSamples;

            return Util.FloatToShortNormalized(data);
        }

        private void HandleEvent(KeyboardNoteEvent kne, PatternTime currentTime)
        {
            var sequence = GetPlayerState(kne.Channel);
            if (kne.KeyDown)
            {
                sequence.BeginNote(kne.Pitch, currentTime);
            }
            else
            {
                sequence.EndNode(kne.Pitch, currentTime);
            }
        }

        public void SeekTo(uint sample)
        {
            _finalChunkGenerated = 0;
            _sequences.Clear();
        }
    }

    public class LivePlayerChannelState
    {
        public List<Note> ActiveNotes { get; } = new List<Note>();
        public NoteSequence Sequence { get; } = new NoteSequence();

        public void BeginNote(Pitch pitch, PatternTime time)
        {
            var note = ActiveNotes.FirstOrDefault(n => n.Pitch.Equals(pitch));
            if (note != null)
            {
                note.Duration = time - note.StartTime;
                ActiveNotes.Remove(note);
                Sequence.Notes.Remove(note);
            }

            Note newNote = new Note(time, PatternTime.Beats(100), pitch);
            ActiveNotes.Add(newNote);
            Sequence.Notes.Add(newNote);
        }

        public void EndNode(Pitch pitch, PatternTime time)
        {
            Note note = null;
            foreach (var activeNote in ActiveNotes)
            {
                if (activeNote.Pitch.Equals(pitch))
                {
                    note = activeNote;
                }
            }
            if (note != null)
            {
                note.Duration = time - note.StartTime;
                ActiveNotes.Remove(note);
                Sequence.Notes.Remove(note);
            }
        }

        public void ClearAll()
        {
            Sequence.Notes.Clear();
            ActiveNotes.Clear();
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
