using System;

namespace BeatTogether.MasterServer.Kernel.Delegates
{
    public delegate void ResponseCallback(ReadOnlySpan<byte> buffer);
}
