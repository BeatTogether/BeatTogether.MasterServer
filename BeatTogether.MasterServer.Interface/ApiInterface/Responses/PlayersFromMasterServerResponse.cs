using BeatTogether.MasterServer.Interface.ApiInterface.Models;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Responses
{
    public record PlayersFromMasterServerResponse(MServerPlayer[] ServerPlayers)
    {
        public bool Success => ServerPlayers != null;
    }
}
