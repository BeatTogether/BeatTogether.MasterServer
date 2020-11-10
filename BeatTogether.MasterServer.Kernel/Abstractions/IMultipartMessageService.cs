using System.Threading.Tasks;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMultipartMessageService
    {
        Task HandleMultipartMessage(ISession session, MultipartMessage multipartMessage);
    }
}
