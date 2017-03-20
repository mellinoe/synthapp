namespace SynthApp
{
    public interface StreamingDataProvider
    {
        short[] GetNextAudioChunk(uint numSamples);
        void SeekTo(uint sample);
    }
}
