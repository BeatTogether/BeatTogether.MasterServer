using BeatTogether.MasterServer.Messaging.Abstractions.Messages;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMessageDispatcher
    {
        public void Send<T>(ISession session, T message)
            where T : class, IMessage;
    }
}
