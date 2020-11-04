using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Abstractions
{
    public interface IMessageWriter
    {
        void WriteTo<TMessage>(GrowingSpanBuffer buffer, TMessage message)
            where TMessage : class, IMessage;
    }
}
