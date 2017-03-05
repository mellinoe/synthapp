using System.Collections.Generic;

namespace SynthApp
{
    public class NoteSequence
    {
        public List<Note> Notes { get; } = new List<Note>();
        public bool UsesPianoRoll { get; set; } = false;

        public NoteSequence() : this(new List<Note>()) { }
        public NoteSequence(List<Note> notes)
        {
            Notes = notes;
        }
    }
}
