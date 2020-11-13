using System.Threading.Tasks;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IAcknowledgeMessageService
    {
        Task HandleAcknowledgeMessage(ISession session, AcknowledgeMessage message);
    }
}
