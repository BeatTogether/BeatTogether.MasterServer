using BeatTogether.MasterServer.Interface.ApiInterface.Models;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Responses
{

    public record ServerFromCodeResponse(SimpleServer Server)
    {
        public bool Success = Server != null;
    }

    public record ServerFromSecretResponse(SimpleServer Server)
    {
        public bool Success = Server != null;
    }
}
