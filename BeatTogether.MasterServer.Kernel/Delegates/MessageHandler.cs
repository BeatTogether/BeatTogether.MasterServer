using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;

namespace BeatTogether.MasterServer.Kernel.Delegates
{
    public delegate Task MessageHandler<TService>(TService service, ISession session, IMessage message);
    public delegate Task MessageHandler<TService, TMessage>(TService service, ISession session, TMessage message)
        where TMessage : class, IMessage;
    public delegate Task<TResponse> MessageHandler<TService, TRequest, TResponse>(TService service, ISession session, TRequest message)
        where TRequest : class, IMessage
        where TResponse : class, IMessage;
}
