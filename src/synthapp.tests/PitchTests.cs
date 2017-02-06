using SynthApp;
using System;
using Xunit;

namespace Tests
{
    public class PitchTests
    {
        [Fact]
        public void A4Is440Hz()
        {
            Pitch a4 = new Pitch(PitchClass.A, 4);
            Assert.Equal(57, a4.Value);
            var et = new EqualTemperamentSystem();
            Assert.Equal(440, et.GetFrequency(a4));
        }

        [Fact]
        public void A0()
        {
            Pitch a0 = new Pitch(PitchClass.A, 0);
            Assert.Equal(9, a0.Value);
            var et = new EqualTemperamentSystem();
            Assert.Equal(27.5, et.GetFrequency(a0), 7);
        }

        [Fact]
        public void C4()
        {
            Pitch c4 = new Pitch(PitchClass.C, 4);
            Assert.Equal(48, c4.Value);
            var et = new EqualTemperamentSystem();
            Assert.Equal(261.626, et.GetFrequency(c4), 3);
        }

        [Fact]
        public void Octaves()
        {
            var et = new EqualTemperamentSystem();
            foreach (PitchClass pc in Enum.GetValues(typeof(PitchClass)))
            {
                uint start = 0;
                Pitch p = new Pitch(pc, start);
                double frequency = et.GetFrequency(p);
                for (uint i = start + 1; i <= 10; i++)
                {
                    Pitch octave = new Pitch(pc, i);
                    double octaveFrequency = et.GetFrequency(octave);
                    Assert.Equal(2 * frequency, octaveFrequency, 3);
                    frequency = octaveFrequency;
                }
            }
        }
    }
}