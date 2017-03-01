using System.Collections.Generic;

namespace SynthApp
{
    public class NoteSequence
    {
        public List<Note> Notes { get; } = new List<Note>();
    }

    public class Pattern
    {
        private static readonly PatternTime DefaultPatternDuration = PatternTime.Beats(4);

        public List<NoteSequence> NoteSequences { get; set; }

        public PatternTime Duration { get; set; }

        public Pattern(IReadOnlyList<Channel> channels)
        {
            NoteSequences = new List<NoteSequence>();
            for (int i = 0; i < channels.Count; i++)
            {
                NoteSequences.Add(new NoteSequence());
            }

            Duration = DefaultPatternDuration;
        }

        public Pattern()
        {
        }
    }
}
