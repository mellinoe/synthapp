using System;

namespace SynthApp
{
    public struct Pitch
    {
        /// <summary>
        /// A value representing the pitch, where 0 is equivalent to the pitch A0.
        /// </summary>
        public readonly byte Value;

        public Pitch(PitchClass pitchClass, uint octave)
        {
            if (octave > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(octave));
            }

            Value = (byte)(octave * 12 + (byte)pitchClass);
        }

        public Pitch(byte value)
        {
            Value = value;
        }

        public static Pitch MiddleC { get; } = new Pitch(PitchClass.C, 4);
    }

    public enum PitchClass
    {
        C = 0,
        CSharp = 1,
        DFlat = 1,
        D = 2,
        DSharp = 3,
        EFlat = 3,
        E = 4,
        F = 5,
        FSharp = 6,
        GFlat = 6,
        G = 7,
        GSharp = 8,
        AFlat = 8,
        A = 9,
        ASharp = 10,
        BFlat = 10,
        B = 11
    }
}
