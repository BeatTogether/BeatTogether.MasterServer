using System.Threading.Tasks;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMultipartMessageService
    {
        Task<byte[]> WaitForEntireMessage(uint multipartMessageId);
        bool AddMultipartMessageWaiter(uint multipartMessageId);
        void OnReceived(MultipartMessage multipartMessage);
    }
}
