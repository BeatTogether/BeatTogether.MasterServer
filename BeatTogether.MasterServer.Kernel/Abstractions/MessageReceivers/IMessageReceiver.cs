using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMessageReceiver
    {
        Task OnReceived(ISession session, IMessage message);
    }
}
