using Newtonsoft.Json;
using System;

namespace SynthApp
{
    public struct Pitch : IEquatable<Pitch>, IComparable<Pitch>
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

        [JsonConstructor]
        public Pitch(byte value)
        {
            Value = value;
        }

        /// <summary>
        /// C4
        /// </summary>
        public static Pitch MiddleC { get; } = new Pitch(PitchClass.C, 4);

        public PitchClass PitchClass
        {
            get
            {
                int value = Value % 12;
                return (PitchClass)value;
            }
        }

        public uint Octave
        {
            get
            {
                return Value / 12u;
            }
        }

        public int CompareTo(Pitch other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(Pitch other)
        {
            return Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return $"{WithSymbol(PitchClass)}{Octave}";
        }

        private object WithSymbol(PitchClass pitchClass)
        {
            return pitchClass.ToString().Replace("Sharp", "#").Replace("Flat", "b");
        }
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
