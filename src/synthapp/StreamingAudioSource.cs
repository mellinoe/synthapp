namespace SynthApp
{
    public interface StreamingAudioSource
    {
        StreamingDataProvider DataProvider { get; set; }
        uint SamplesProcessed { get; }
        void Play();
    }
}
