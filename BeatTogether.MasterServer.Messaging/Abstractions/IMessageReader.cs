using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Abstractions
{
    public interface IMessageReader<TMessageRegistry>
        where TMessageRegistry : class, IMessageRegistry
    {
        IMessage ReadFrom(SpanBufferReader bufferReader);
    }
}
