using System;
using System.Diagnostics;

namespace SynthApp
{
    public static class Util
    {
        /// <summary>
        /// Converts a double in the [-1, 1] range to a normalized ushort.
        /// </summary>
        /// <param name="value">The value to convert. Must be in the [-1, 1] range.</param>
        /// <returns>A normalized value.</returns>
        public static ushort Normalize(double value)
        {
            value = Clamp(value, -1, 1);
            return (ushort)((value * 0.5 + 0.5) * ushort.MaxValue);
        }

        /// <summary>
        /// Converts a double in the [-1, 1] range to a normalized short.
        /// </summary>
        /// <param name="value">The value to convert. Must be in the [-1, 1] range.</param>
        /// <returns>A normalized value.</returns>
        public static short DoubleToShort(double value)
        {
            value = Clamp(value, -1, 1);
            return (short)(value * short.MaxValue);
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value <= min)
            {
                return min;
            }
            else if (value >= max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }

        public static void Mix(float[] a, float[] b, float[] result)
        {
            Debug.Assert(a.Length == b.Length && a.Length == result.Length);
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] + b[i];
            }
        }

        public static uint Min(uint a, uint b)
        {
            if (a <= b)
            {
                return a;
            }
            else
            {
                return b;
            }
        }

        public static uint Max(uint a, uint b)
        {
            if (a > b)
            {
                return a;
            }
            else
            {
                return b;
            }
        }
    }
}
