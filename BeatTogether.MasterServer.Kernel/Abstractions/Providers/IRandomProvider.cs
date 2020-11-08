namespace BeatTogether.MasterServer.Kernel.Abstractions.Providers
{
    public interface IRandomProvider
    {
        byte[] GetRandom();
    }
}
