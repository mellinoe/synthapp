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
            NoteSequences = new List<NoteSequence>();
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

        public void GetNextNotes(uint start, uint sampleCount, uint sequencerPlaybackOffset, uint patternSampleOffset, ChannelState channelState, uint channelIndex, bool wrap)
        {
            Debug.Assert(channelIndex < NoteSequences.Count);
            NoteSequence ns = NoteSequences[(int)channelIndex];
            uint sampleWrapPosition = wrap ? (uint)CalculateFinalNoteEndTime().ToSamplesAuto() : uint.MaxValue;
            uint wrapOffset = 0;
            uint wrappedStart = start % sampleWrapPosition;
            if (wrappedStart != start)
            {
                wrapOffset = start - wrappedStart;
            }

            foreach (var note in ns.Notes)
            {
                if ((note.StartTime.ToSamplesAuto() + patternSampleOffset) >= wrappedStart
                    && (note.StartTime.ToSamplesAuto() + patternSampleOffset) < wrappedStart + sampleCount)
                {
                    uint noteSampleStart = (uint)Math.Round(note.StartTime.ToSamplesAuto() + patternSampleOffset);
                    uint noteSampleCount = (uint)Math.Round(note.Duration.ToSamplesAuto());

                    MaterializedNote mn = new MaterializedNote(
                        TuningSystem.EqualTemperament.GetFrequency(note.Pitch),
                        wrapOffset + sequencerPlaybackOffset + noteSampleStart,
                        noteSampleCount,
                        note.Velocity,
                        note.Pan);
                    channelState.AddNote(mn);
                }
            }

            if (wrap)
            {
                uint endSample = wrappedStart + sampleCount;
                uint wrappedRemainder = endSample % sampleWrapPosition;
                if (wrappedRemainder != endSample)
                {
                    // TODO: This is awful
                    uint wrapCount = (uint)((double)(start + sampleCount) / sampleWrapPosition);
                    GetNextNotes(sampleWrapPosition * wrapCount, wrappedRemainder, sequencerPlaybackOffset, patternSampleOffset, channelState, channelIndex, true);
                }
            }
        }
    }
}
