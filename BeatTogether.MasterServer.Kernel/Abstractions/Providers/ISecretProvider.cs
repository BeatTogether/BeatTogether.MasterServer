namespace BeatTogether.MasterServer.Kernel.Abstractions.Providers
{
    public interface ISecretProvider
    {
        string GetSecret();
    }
}
