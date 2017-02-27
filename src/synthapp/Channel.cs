using System;

namespace SynthApp
{
    /// <summary>
    /// Represents a single instrument.
    /// </summary>
    public abstract class Channel
    {
        public int ID { get; } = Globals.GetNextGlobalID();

        public string Name { get; set; } = "New Channel";
        public float Gain { get; set; } = 0.7f;
        public bool Muted { get; set; } = false;

        public abstract void Play(float[] output, NoteSequence ns, uint startSample, uint numSamples);
    }
}
