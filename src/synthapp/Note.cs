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
        public float Velocity { get; set; } = 0.7f;

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
}
