namespace BeatTogether.MasterServer.Kernel.Abstractions.Providers
{
    public interface IRequestIdProvider
    {
        uint GetNextRequestId();
    }
}
