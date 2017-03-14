using System;
using System.Collections.Generic;
using System.Linq;

namespace SynthApp
{
    /// <summary>
    /// Stores which notes are currently active in a channel.
    /// </summary>
    public class ChannelState
    {
        public MaterializedNoteSequence Sequence { get; } = new MaterializedNoteSequence();

        /// <summary>
        /// Notes being played through the keyboard.
        /// </summary>
        public List<MaterializedNote> KeyboardActiveNotes { get; } = new List<MaterializedNote>();

        /// <summary>
        /// Add a full, completed note, not associated with the keyboard.
        /// </summary>
        /// <param name="n"></param>
        public void AddNote(MaterializedNote n)
        {
            Sequence.Notes.Add(n);
        }

        public void BeginKeyboardNote(Pitch pitch, uint currentSample)
        {
            double frequency = TuningSystem.EqualTemperament.GetFrequency(pitch);
            var note = KeyboardActiveNotes.FirstOrDefault(n => n.Frequency.Equals(frequency));
            if (note != null)
            {
                note.SampleCount = currentSample - note.StartSample;
                KeyboardActiveNotes.Remove(note);
                Sequence.Notes.Remove(note);
            }

            MaterializedNote newNote = new MaterializedNote(frequency, currentSample, uint.MaxValue / 2, 0.75f, 0.0);
            KeyboardActiveNotes.Add(newNote);
            Sequence.Notes.Add(newNote);
        }

        public void EndKeyboardNote(Pitch pitch, uint currentSample)
        {
            double frequency = TuningSystem.EqualTemperament.GetFrequency(pitch);
            MaterializedNote note = null;
            foreach (var activeNote in KeyboardActiveNotes)
            {
                if (activeNote.Frequency.Equals(frequency))
                {
                    note = activeNote;
                }
            }
            if (note != null)
            {
                note.SampleCount = currentSample - note.StartSample;
                KeyboardActiveNotes.Remove(note);
                Sequence.Notes.Remove(note);
            }
        }

        public void ClearNotesBefore(uint currentSample)
        {
            for (int i = 0; i < Sequence.Notes.Count; i++)
            {
                MaterializedNote n = Sequence.Notes[i];
                if (n.EndSample < currentSample)
                {
                    Sequence.Notes.RemoveAt(i);
                    i -= 1;
                }
            }
        }

        public void ClearAll()
        {
            Sequence.Notes.Clear();
            KeyboardActiveNotes.Clear();
        }
    }
}
