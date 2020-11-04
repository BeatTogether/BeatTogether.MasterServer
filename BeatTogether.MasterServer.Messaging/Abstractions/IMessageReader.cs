using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Abstractions
{
    public interface IMessageReader
    {
        IMessage ReadFrom(SpanBufferReader bufferReader);
    }
}
