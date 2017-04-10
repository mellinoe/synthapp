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
        }
    }

    public class PlaylistEntry
    {
        public int PatternIndex { get; set; }
        public ulong SongStepOffset { get; set; }
        /// <summary>
        /// The row in the playlist that this entry appears. "0" is the top grid row. This is only used for display purposes.
        /// </summary>
        public uint GridRow { get; set; }
    }
}
