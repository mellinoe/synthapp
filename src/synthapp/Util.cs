using System;
using System.Buffers;
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

        public static double Clamp(double value, double min, double max)
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

        public static float Clamp(float value, float min, float max)
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

        public static int Clamp(int value, int min, int max)
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

        public static uint Argb(float a, float r, float g, float b)
        {
            return
                unchecked((uint)(
                    (byte)(a * 255.0f) << 24
                    | (byte)(r * 255.0f) << 0
                    | (byte)(g * 255.0f) << 8
                    | (byte)(b * 255.0f) << 16)
                );
        }

        public static short[] FloatToShortNormalized(float[] total)
        {
            short[] normalized = new short[total.Length];
            FloatToShortNormalized(normalized, total);
            return normalized;
        }

        public static T[] Rent<T>(int count)
        {
            T[] ret = ArrayPool<T>.Shared.Rent(count);
            return ret;
        }

        public static T[] Rent<T>(uint count)
        {
            T[] ret = ArrayPool<T>.Shared.Rent((int)count);
            return ret;
        }

        public static void Return<T>(T[] array)
        {
            ArrayPool<T>.Shared.Return(array, clearArray: true);
        }

        public static void FloatToShortNormalized(short[] output, float[] total)
        {
            for (int i = 0; i < total.Length; i++)
            {
                output[i] = DoubleToShort(total[i]);
            }
        }

        public static void Clear<T>(T[] array)
        {
            Array.Clear(array, 0, array.Length);
        }
    }
}
