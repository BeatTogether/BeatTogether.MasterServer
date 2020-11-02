using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Abstractions
{
    public interface IMessageWriter<TMessageRegistry>
        where TMessageRegistry : class, IMessageRegistry
    {
        void WriteTo<TMessage>(GrowingSpanBuffer buffer, TMessage message)
            where TMessage : class, IMessage;
    }
}
