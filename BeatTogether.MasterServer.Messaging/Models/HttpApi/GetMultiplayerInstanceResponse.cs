using BeatTogether.MasterServer.HttpApi.Models.Enums;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models.HttpApi
{
    public class GetMultiplayerInstanceResponse
    {
        [JsonProperty("error_code")]
        public MultiplayerPlacementErrorCode ErrorCode { get; set; } = MultiplayerPlacementErrorCode.Unknown;

        [JsonProperty("player_session_info")] 
        public PlayerSessionInfo PlayerSessionInfo { get; set; } = new();

        [JsonProperty("poll_interval_ms")] 
        public int PollIntervalMs { get; set; } = -1;

        [JsonProperty("ticket_id")]
        public string TicketId { get; set; } = "";

        [JsonProperty("ticket_status")]
        public string TicketStatus { get; set; } = "";

        [JsonProperty("placement_id")]
        public string PlacementId { get; set; } = "";

        [JsonProperty("placement_status")]
        public string PlacementStatus { get; set; } = "";

        #region Utils

        public void AddRequestContext(GetMultiplayerInstanceRequest request)
        {
            PlayerSessionInfo.PrivateGameCode = request.PrivateGameCode;
            PlayerSessionInfo.PrivateGameSecret = request.PrivateGameSecret;
            TicketId = request.TicketId;
            PlacementId = request.PlacementId;
        }

        public void AddSessionContext(string playerSessionId)
        {
            PlayerSessionInfo.PlayerSessionId = playerSessionId;
            TicketId = playerSessionId;
        }

        #endregion
    }
}