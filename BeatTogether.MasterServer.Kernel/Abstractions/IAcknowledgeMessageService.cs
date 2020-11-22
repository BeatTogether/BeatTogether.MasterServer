using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Messaging.Messages;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IAcknowledgeMessageService
    {
        Task HandleAcknowledgeMessage(ISession session, AcknowledgeMessage message);
    }
}
