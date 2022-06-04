namespace BeatTogether.MasterServer.Interface.ApiInterface.Responses
{
    public record CreatedServerResponse(bool Success, string Secret = "", string Code = "");
}
