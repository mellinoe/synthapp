﻿using System;
using System.Diagnostics;

namespace SynthApp
{
    public struct PatternTime : IComparable<PatternTime>
    {
        /// <summary>
        /// The step within the pattern. Generally, a step represents 1/4 of a beat.
        /// </summary>
        public readonly uint Step;
        /// <summary>
        /// The tick within the beat. Generally, a tick represents 1/24 of a step.
        /// </summary>
        public readonly uint Tick;

        public double TotalBeats => (double)Step / 4 + ((double)Tick / 24 / 4);

        public PatternTime(uint step, uint tick)
        {
            Step = step;
            Tick = tick;
        }

        public static readonly PatternTime Zero = new PatternTime(0, 0);

        public static PatternTime Steps(uint steps) => new PatternTime(steps, 0);
        public static PatternTime Beats(uint beats) => new PatternTime(beats * 4, 0);
        public static PatternTime Beats(double beats) => new PatternTime((uint)(beats * 4), 0);

        public static PatternTime Samples(uint samples, uint sampleRate, double beatsPerMinute)
        {
            double SecondsPerBeat = 60 / beatsPerMinute;
            double SamplesPerBeat = sampleRate * SecondsPerBeat;
            double SamplesPerStep = SamplesPerBeat / 4;
            double totalSteps = samples / SamplesPerStep;
            double fractionalSteps = totalSteps - (uint)totalSteps;
            uint wholeSteps = (uint)totalSteps;
            uint ticks = (uint)Math.Ceiling(fractionalSteps * 24.0);

            return new PatternTime(wholeSteps, ticks);
        }

        public double ToSamples(uint sampleRate, double beatsPerMinute)
        {
            double SecondsPerBeat = 60 / beatsPerMinute;
            double SamplesPerBeat = sampleRate * SecondsPerBeat;
            return TotalBeats * SamplesPerBeat;
        }

        public int CompareTo(PatternTime other)
        {
            int step = Step.CompareTo(other.Step);
            if (step != 0)
            {
                return step;
            }

            int tick = Tick.CompareTo(other.Tick);
            return tick;
        }

        public static bool operator >(PatternTime left, PatternTime right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(PatternTime left, PatternTime right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >=(PatternTime left, PatternTime right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator <=(PatternTime left, PatternTime right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator ==(PatternTime left, PatternTime right)
        {
            return left.CompareTo(right) == 0;
        }

        internal static PatternTime RoundToUpperBar(PatternTime duration)
        {
            double beats = duration.TotalBeats;
            return PatternTime.Beats(4 * (uint)(Math.Ceiling(beats / 4.0)));
        }

        public static bool operator !=(PatternTime left, PatternTime right)
        {
            return left.CompareTo(right) != 0;
        }

        public static PatternTime operator -(PatternTime left, PatternTime right)
        {
            var diff = left.TotalBeats - right.TotalBeats;
            return Beats((uint)diff);
        }

        public static PatternTime operator +(PatternTime left, PatternTime right)
        {
            var sum = left.TotalBeats + right.TotalBeats;
            return Beats(sum);
        }

        public override int GetHashCode()
        {
            return Step.GetHashCode() ^ Tick.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is PatternTime pt && CompareTo(pt) == 0);
        }

        public override string ToString()
        {
            return $"{Step}:{Tick} ({TotalBeats} beats)";
        }
    }
}
