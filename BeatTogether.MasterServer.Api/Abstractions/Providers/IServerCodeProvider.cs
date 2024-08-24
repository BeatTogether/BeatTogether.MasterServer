namespace BeatTogether.MasterServer.Api.Abstractions.Providers
{
    public interface IServerCodeProvider
    {
        string Generate(int length = 5);
    }
}
