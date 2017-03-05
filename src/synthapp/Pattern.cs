using System.Collections.Generic;

namespace SynthApp
{
    public class Pattern
    {
        public static readonly PatternTime DefaultPatternDuration = PatternTime.Beats(4);

        public List<NoteSequence> NoteSequences { get; set; }

        public PatternTime CalculateFinalNoteEndTime()
        {
            PatternTime latest = PatternTime.Zero;
            foreach (NoteSequence ns in NoteSequences)
            {
                PatternTime sequenceLatest = Util.CalculateFinalNoteEndTime(ns.Notes);
                if (sequenceLatest > latest)
                {
                    latest = sequenceLatest;
                }
            }

            return latest < DefaultPatternDuration ? DefaultPatternDuration : latest;
        }

        public Pattern(IReadOnlyList<Channel> channels)
        {
            NoteSequences = new List<NoteSequence>();
            for (int i = 0; i < channels.Count; i++)
            {
                NoteSequences.Add(new NoteSequence());
            }
        }

        public Pattern()
        {
        }
    }
}
