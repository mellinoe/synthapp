namespace SynthApp
{
    public static class PatternTimeEx
    {
        public static double ToSamplesAuto(this PatternTime pt)
        {
            return pt.ToSamples(Globals.SampleRate, Globals.BeatsPerMinute);
        }
    }
}
