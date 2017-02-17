using System.Collections.Generic;

namespace SynthApp
{
    public class LiveNotePlayer : StreamingDataProvider
    {
        private uint _finalChunkGenerated;

        private Dictionary<Channel, NoteSequence> _sequences = new Dictionary<Channel, NoteSequence>();

        public uint SamplePlaybackLocation => _finalChunkGenerated;

        public void AddNote(Channel c, Note n)
        {
            NoteSequence ns = GetNoteSequence(c);
            ns.Notes.Add(n);
        }

        private NoteSequence GetNoteSequence(Channel c)
        {
            if (!_sequences.TryGetValue(c, out NoteSequence ns))
            {
                ns = new NoteSequence();
                _sequences.Add(c, ns);
            }

            return ns;
        }

        public short[] GetNextAudioChunk(uint numSamples)
        {
            float[] data = new float[numSamples];
            foreach (var kvp in _sequences)
            {
                Channel c = kvp.Key;
                NoteSequence ns = kvp.Value;

                float[] channelData = c.Play(ns, _finalChunkGenerated, numSamples);
                Util.Mix(data, channelData, data);
            }

            _finalChunkGenerated += numSamples;

            return Util.FloatToShortNormalized(data);
        }

        public void SeekTo(uint sample)
        {
            _finalChunkGenerated = 0;
            _sequences.Clear();
        }
    }
}
