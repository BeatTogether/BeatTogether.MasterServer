using System;
using System.Buffers;
using BeatTogether.MasterServer.Kernel.Models;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMessageReceiver
    {
        void OnReceived(Session session, ReadOnlySpan<byte> data, ReadOnlySpanAction<byte, Session> responseCallback);
    }
}
