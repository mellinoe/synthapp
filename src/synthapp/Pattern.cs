using System.Collections.Generic;

namespace SynthApp
{
    public class Pattern
    {
        public List<Note> Notes { get; } = new List<Note>();

        public PatternTime Duration { get; set; }
    }
}
