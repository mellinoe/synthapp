namespace SynthApp
{
    public static class Globals
    {
        // NOTE: None of this stuff should be global, but I'm lazy.

        public static double BeatsPerMinute { get; set; } = 100;
        public static double SecondsPerBeat => 60 / BeatsPerMinute;
        public static double SamplesPerBeat => SampleRate * SecondsPerBeat;

        /// <summary>
        /// Samples per second.
        /// </summary>
        public static uint SampleRate { get; set; } = 44100;

        public static AudioEngine AudioEngine { get; } = new AudioEngine();
    }
}
