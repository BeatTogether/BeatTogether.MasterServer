namespace BeatTogether.MasterServer.Interface.ApiInterface.Requests
{
    public record GetServerFromCodeRequest(string Code);

    public record GetServerFromSecretRequest(string secret);

}
