using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SynthApp
{
    public class Pattern
    {
        public static readonly PatternTime DefaultPatternDuration = PatternTime.Beats(4);

        public List<NoteSequence> NoteSequences { get; set; }

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

        public void GetNextNotes(uint start, uint sampleCount, ChannelState channelState, uint channelIndex, bool wrap)
        {
            Debug.Assert(channelIndex < NoteSequences.Count);
            NoteSequence ns = NoteSequences[(int)channelIndex];
            uint sampleWrapPosition = (uint)CalculateFinalNoteEndTime().ToSamplesAuto();
            uint wrapOffset = 0;
            uint wrappedStart = start % sampleWrapPosition;
            if (wrappedStart != start)
            {
                wrapOffset = start - wrappedStart;
            }

            PatternTime startOffset = PatternTime.Samples(wrapOffset, Globals.SampleRate, Globals.BeatsPerMinute);
            foreach (var note in ns.Notes)
            {
                if (note.StartTime.ToSamplesAuto() >= wrappedStart
                    && note.StartTime.ToSamplesAuto() < wrappedStart + sampleCount)
                {
                    channelState.AddNote(new Note(note, startOffset));
                }
            }

            if (wrap)
            {
                uint endSample = wrappedStart + sampleCount;
                uint wrappedEnd = endSample % sampleWrapPosition;
                if (wrappedEnd != endSample)
                {
                    // TODO: This is awful
                    uint wraps = (uint)((double)((start + sampleCount) / sampleWrapPosition));
                    GetNextNotes(sampleWrapPosition * wraps, wrappedEnd, channelState, channelIndex, false);
                }
            }
        }
    }
}
