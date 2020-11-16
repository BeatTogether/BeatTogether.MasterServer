using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMultipartMessageService
    {
        Task<IMessage> HandleMultipartMessage(ISession session, MultipartMessage message);
    }
}
