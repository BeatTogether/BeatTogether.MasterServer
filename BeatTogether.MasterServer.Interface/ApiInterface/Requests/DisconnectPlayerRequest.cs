namespace BeatTogether.MasterServer.Interface.ApiInterface.Requests
{
    public record DisconnectPlayerRequest(string Secret, string UserId);

}
