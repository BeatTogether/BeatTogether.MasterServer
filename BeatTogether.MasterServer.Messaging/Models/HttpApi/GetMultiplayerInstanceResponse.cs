using BeatTogether.MasterServer.HttpApi.Models;
using BeatTogether.MasterServer.HttpApi.Models.Enums;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models.HttpApi
{
    public class GetMultiplayerInstanceResponse
    {
        [JsonProperty("error_code")]
        public MultiplayerPlacementErrorCode? ErrorCode { get; set; }

        [JsonProperty("player_session_info")]
        public PlayerSessionInfo? PlayerSessionInfo { get; set; }

        [JsonProperty("poll_interval_ms")]
        public int? PollIntervalMs { get; set; }

        [JsonProperty("ticket_id")]
        public string? TicketId { get; set; }

        [JsonProperty("ticket_status")]
        public string? TicketStatus { get; set; }

        [JsonProperty("placement_id")]
        public string? PlacementId { get; set; }

        [JsonProperty("placement_status")]
        public string? PlacementStatus { get; set; }

        #region Utils

        /// <summary>
        /// Creates a response with a simple error code.
        /// </summary>
        public static GetMultiplayerInstanceResponse ForErrorCode(MultiplayerPlacementErrorCode errorCode) => new()
        {
            ErrorCode = errorCode
        };
        
        /// <summary>
        /// Creates a pending/timeout response that tells the client to retry their request after a certain poll time. 
        /// </summary>
        public static GetMultiplayerInstanceResponse ForPendingPlacement(string privateGameSecret, string placementId,
            int pollIntervalMs, GameplayServerConfiguration gameplayServerConfiguration,
            BeatmapLevelSelectionMask beatmapLevelSelectionMask) => new()
        {
            ErrorCode = MultiplayerPlacementErrorCode.RequestTimeout,
            PlayerSessionInfo = new PlayerSessionInfo()
            {
                GameSessionId = "",
                Port = -1,
                DnsName = "",
                PlayerSessionId = "",
                PrivateGameCode = "",
                GameplayServerConfiguration = gameplayServerConfiguration,
                BeatmapLevelSelectionMask = beatmapLevelSelectionMask,
                PrivateGameSecret = privateGameSecret
            },
            PollIntervalMs = pollIntervalMs,
            TicketId = "",
            TicketStatus = "",
            PlacementId = placementId,
            PlacementStatus = "PENDING"
        };
        
        #endregion
    }
}