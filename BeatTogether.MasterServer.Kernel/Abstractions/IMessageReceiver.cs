using System;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Models;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMessageReceiver
    {
        Task OnReceived(Session session, ReadOnlySpan<byte> data);
    }
}
