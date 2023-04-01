using System.Collections.Generic;
using BeatTogether.MasterServer.HttpApi.Models.Enums;
using BeatTogether.MasterServer.Messaging.Enums;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models.HttpApi
{
    public class GetMultiplayerInstanceRequest
    {
        [JsonProperty("version")]
        public string? Version { get; set; }

        [JsonProperty("service_environment")]
        public ServiceEnvironment? ServiceEnvironment { get; set; }

        [JsonProperty("single_use_auth_token")]
        public string? SingleUseAuthToken { get; set; }

        [JsonProperty("beatmap_level_selection_mask")]
        public BeatmapLevelSelectionMask? BeatmapLevelSelectionMask { get; set; }

        [JsonProperty("gameplay_server_configuration")]
        public GameplayServerConfiguration? GameplayServerConfiguration { get; set; }

        [JsonProperty("user_id")]
        public string? UserId { get; set; }

        [JsonProperty("private_game_secret")]
        public string? PrivateGameSecret { get; set; }

        [JsonProperty("private_game_code")]
        public string? PrivateGameCode { get; set; }

        [JsonProperty("platform")]
        public Platform? Platform { get; set; }

        [JsonProperty("auth_user_id")]
        public string? AuthUserId { get; set; }

        [JsonProperty("gamelift_region_latencies")]
        public Dictionary<string, long>? GameliftRegionLatencies { get; set; }

        [JsonProperty("ticket_id")]
        public string? TicketId { get; set; }

        [JsonProperty("placement_id")]
        public string? PlacementId { get; set; }
    }
}