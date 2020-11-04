using System;
using BeatTogether.MasterServer.Kernel.Models;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public delegate void ResponseCallback(ReadOnlySpan<byte> buffer);

    public interface IMessageReceiver
    {
        void OnReceived(Session session, ReadOnlySpan<byte> buffer, ResponseCallback responseCallback);
    }
}
