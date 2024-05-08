namespace BeatTogether.MasterServer.Api.Abstractions.Providers
{
    public interface ISecretProvider
    {
        string GetSecret();
    }
}
