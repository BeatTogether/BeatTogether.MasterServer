using System.Threading;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;

namespace BeatTogether.MasterServer.Kernel.Implementations.Providers
{
    public class RequestIdProvider : IRequestIdProvider
    {
        private uint _count;

        public uint GetNextRequestId()
            => Interlocked.Increment(ref _count);
    }
}
