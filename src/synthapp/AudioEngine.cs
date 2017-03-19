namespace SynthApp
{
    public interface AudioEngine
    {
        StreamingAudioSource CreateStreamingAudioSource(StreamingDataProvider dataProvider, uint bufferedSamples);
    }
}
