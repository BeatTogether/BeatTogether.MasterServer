namespace BeatTogether.MasterServer.Kernel.Abstractions.Providers
{
    public interface IServerCodeProvider
    {
        string Generate(int length = 5);
    }
}
