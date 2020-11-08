using System;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMessageReceiver
    {
        void OnReceived(ISession session, ReadOnlySpan<byte> buffer);
    }
}
