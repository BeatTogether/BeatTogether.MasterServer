using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using BeatTogether.MasterServer.Messaging.Implementations;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MessageDispatcher<TMessageRegistry> : IMessageDispatcher
        where TMessageRegistry : class, IMessageRegistry
    {
        private readonly IMessageWriter _messageWriter;

        public MessageDispatcher(MessageWriter<TMessageRegistry> messageWriter)
        {
            _messageWriter = messageWriter;
        }

        #region Public Methods

        public void Send<T>(ISession session, T message)
            where T : class, IMessage
        {
            var buffer = new GrowingSpanBuffer(stackalloc byte[412]);
            _messageWriter.WriteTo(ref buffer, message);
            session.Send(buffer.Data);
        }

        public void SendEncrypted<T>(ISession session, T message)
            where T : class, IMessage
        {
            var buffer = new GrowingSpanBuffer(stackalloc byte[412]);
            _messageWriter.WriteTo(ref buffer, message);
            session.SendEncrypted(buffer.Data);
        }

        #endregion
    }
}
