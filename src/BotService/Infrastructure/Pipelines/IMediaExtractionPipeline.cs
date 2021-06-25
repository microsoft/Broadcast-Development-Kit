using Gst;

namespace BotService.Infrastructure.Pipelines
{
    public interface IMediaExtractionPipeline
    {
        Bus Bus { get; set; }

        StateChangeReturn Play();

        StateChangeReturn Stop();

        void PushAudioBuffer(byte[] buffer, long timestamp);

        void PushVideoBuffer(byte[] buffer, long timestamp, int width, int height);
    }
}
