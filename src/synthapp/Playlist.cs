using System.Collections.Generic;

namespace SynthApp
{
    /// <summary>
    /// Stores the sequence of patterns making up a song.
    /// </summary>
    public class Playlist
    {
        public List<PlaylistEntry> Entries { get; } = new List<PlaylistEntry>();

        public Playlist()
        {
            Entries.Add(new PlaylistEntry() { PatternIndex = 0, SongStepOffset = 0 });
        }
    }

    public class PlaylistEntry
    {
        public int PatternIndex { get; set; }
        public ulong SongStepOffset { get; set; }
    }
}
