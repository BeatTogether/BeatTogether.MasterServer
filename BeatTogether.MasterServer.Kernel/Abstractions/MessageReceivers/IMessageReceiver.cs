using System.Threading.Tasks;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMessageReceiver
    {
        Task OnReceived(ISession session, IMessage message);
    }
}
