using System;
using BeatTogether.MasterServer.Kernel.Delegates;
using BeatTogether.MasterServer.Kernel.Models;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMessageReceiver
    {
        void OnReceived(Session session, ReadOnlySpan<byte> buffer, ResponseCallback responseCallback);
    }
}
