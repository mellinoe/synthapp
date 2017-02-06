using System;

namespace SynthApp
{
    /// <summary>
    /// Represents a single note within a pattern.
    /// </summary>
    public class Note
    {
        /// <summary>
        /// The starting time within a pattern.
        /// </summary>
        public PatternTime StartTime { get; set; }

        /// <summary>
        /// The duration of the note.
        /// </summary>
        public PatternTime Duration { get; set; }

        /// <summary>
        /// The pitch of the note.
        /// </summary>
        public Pitch Pitch { get; set; }

        /// <summary>
        /// The relative volume of the note, in the range [0, 1].
        /// </summary>
        public float Velocity { get; set; } = 1.0f;

        /// <summary>
        /// The left-right position of the note, ranging from -1.0 (100% left) to 0.0 (centered) to 1.0 (100% right).
        /// </summary>
        public double Pan { get; set; } = 0.0;

        public Note(PatternTime start, PatternTime duration, Pitch pitch)
        {
            StartTime = start;
            Duration = duration;
            Pitch = pitch;
        }

        public override string ToString()
        {
            return $"[{Pitch}] {StartTime}, {Duration}";
        }
    }

    public struct PatternTime : IComparable<PatternTime>
    {
        /// <summary>
        /// The step within the pattern. Generally, a step represents 1/4 of a beat.
        /// </summary>
        public readonly uint Step;
        /// <summary>
        /// The tick within the beat. Generally, a tick represents 1/24 of a step.
        /// </summary>
        public readonly uint Tick;

        public double TotalBeats => (double)Step / 4 + ((double)Tick / 24 / 4);

        public PatternTime(uint step, uint tick)
        {
            Step = step;
            Tick = tick;
        }

        public static readonly PatternTime Zero = new PatternTime(0, 0);

        public static PatternTime Steps(uint steps) => new PatternTime(steps, 0);

        public int CompareTo(PatternTime other)
        {
            int step = Step.CompareTo(other.Step);
            if (step != 0)
            {
                return step;
            }

            int tick = Tick.CompareTo(other.Tick);
            return tick;
        }

        public static bool operator >(PatternTime left, PatternTime right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(PatternTime left, PatternTime right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >=(PatternTime left, PatternTime right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator <=(PatternTime left, PatternTime right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator ==(PatternTime left, PatternTime right)
        {
            return left.CompareTo(right) == 0;
        }

        public static bool operator !=(PatternTime left, PatternTime right)
        {
            return left.CompareTo(right) != 0;
        }

        public override int GetHashCode()
        {
            return Step.GetHashCode() ^ Tick.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is PatternTime pt && CompareTo(pt) == 0);
        }

        public override string ToString()
        {
            return $"{Step}:{Tick} ({TotalBeats} beats)";
        }
    }
}
