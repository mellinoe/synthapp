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
        public NoteSequence Sequence { get; } = new NoteSequence();

        /// <summary>
        /// Notes being played through the keyboard.
        /// </summary>
        public List<Note> KeyboardActiveNotes { get; } = new List<Note>();

        /// <summary>
        /// Add a full, completed note, not associated with the keyboard.
        /// </summary>
        /// <param name="n"></param>
        public void AddNote(Note n)
        {
            Sequence.Notes.Add(n);
        }

        public void BeginKeyboardNote(Pitch pitch, PatternTime time)
        {
            var note = KeyboardActiveNotes.FirstOrDefault(n => n.Pitch.Equals(pitch));
            if (note != null)
            {
                note.Duration = time - note.StartTime;
                KeyboardActiveNotes.Remove(note);
                Sequence.Notes.Remove(note);
            }

            Note newNote = new Note(time, PatternTime.Beats(100), pitch);
            newNote.Velocity = 0.75f;
            KeyboardActiveNotes.Add(newNote);
            Sequence.Notes.Add(newNote);
        }

        public void EndKeyboardNote(Pitch pitch, PatternTime time)
        {
            Note note = null;
            foreach (var activeNote in KeyboardActiveNotes)
            {
                if (activeNote.Pitch.Equals(pitch))
                {
                    note = activeNote;
                }
            }
            if (note != null)
            {
                note.Duration = time - note.StartTime;
                KeyboardActiveNotes.Remove(note);
                Sequence.Notes.Remove(note);
            }
        }

        public void ClearNotesBefore(PatternTime time)
        {
            for (int i = 0; i < Sequence.Notes.Count; i++)
            {
                Note n = Sequence.Notes[i];
                if (n.StartTime + n.Duration <= time)
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
