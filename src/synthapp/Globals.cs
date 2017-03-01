using System;
using System.Threading;

namespace SynthApp
{
    public static class Globals
    {
        // NOTE: None of this stuff should be global, but I'm lazy.

        public static double BeatsPerMinute { get; set; } = 100;

        private static int s_nextGlobalID;
        public static int GetNextGlobalID()
        {
            return Interlocked.Increment(ref s_nextGlobalID);
        }

        public static double SecondsPerBeat => 60 / BeatsPerMinute;
        public static double SamplesPerBeat => SampleRate * SecondsPerBeat;
        public static double SamplesPerStep => SamplesPerBeat / 4;

        /// <summary>
        /// Samples per second.
        /// </summary>
        public static uint SampleRate { get; set; } = 44100;
    }
}
