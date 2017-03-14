namespace SynthApp
{
    public class MaterializedNote
    {
        public double Frequency { get; set; }
        public uint StartSample { get; set; }
        public uint SampleCount { get; set; }
        public float Velocity { get; set; }
        public double Pan { get; set; }
        public uint EndSample => StartSample + SampleCount;

        public MaterializedNote(double frequency, uint startSample, uint sampleCount, float velocity, double pan)
        {
            Frequency = frequency;
            StartSample = startSample;
            SampleCount = sampleCount;
            Velocity = velocity;
            Pan = pan;
        }
    }
}
