using System;

namespace SynthApp
{
    /// <summary>
    /// Represents a single instrument.
    /// </summary>
    public abstract class Channel
    {
        public float Gain { get; set; } = 0.7f;

        public abstract float[] Play(Pattern p, uint startSample, uint numSamples);
    }
}
