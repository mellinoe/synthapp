using System;

namespace SynthApp
{
    public abstract class TuningSystem
    {
        public abstract double GetFrequency(Pitch pitch);

        public static EqualTemperamentSystem EqualTemperament { get; } = new EqualTemperamentSystem();
    }

    public class EqualTemperamentSystem : TuningSystem
    {
        private static readonly double s_twelfthRootOfTwo = Math.Pow(2.0, (1.0 / 12.0));

        public override double GetFrequency(Pitch pitch)
        {
            int diff = pitch.Value - 57;
            return 440 * Math.Pow(s_twelfthRootOfTwo, diff);
        }
    }
}
