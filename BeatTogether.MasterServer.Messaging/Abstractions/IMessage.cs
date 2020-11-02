using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Abstractions
{
    public interface IMessage
    {
        void WriteTo(GrowingSpanBuffer buffer);
        void ReadFrom(SpanBufferReader bufferReader);
    }
}
