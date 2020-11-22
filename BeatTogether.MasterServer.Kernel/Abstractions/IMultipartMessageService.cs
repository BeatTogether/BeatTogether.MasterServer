using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Messaging.Messages;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMultipartMessageService
    {
        Task<IMessage> HandleMultipartMessage(ISession session, MultipartMessage message);
    }
}
