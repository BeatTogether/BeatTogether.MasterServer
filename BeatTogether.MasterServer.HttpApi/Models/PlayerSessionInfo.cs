using BeatTogether.MasterServer.Messaging.Models;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.HttpApi.Models
{
    public class PlayerSessionInfo
    {
        [JsonProperty("player_session_id")]
        public string PlayerSessionId { get; set; }

        [JsonProperty("game_session_id")]
        public string GameSessionId { get; set; }

        [JsonProperty("dns_name")]
        public string DnsName { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("beatmap_level_selection_mask")]
        public BeatmapLevelSelectionMask BeatmapLevelSelectionMask { get; set; }

        [JsonProperty("gameplay_server_configuration")]
        public GameplayServerConfiguration GameplayServerConfiguration { get; set; }

        [JsonProperty("private_game_secret")]
        public string PrivateGameSecret { get; set; }
        
        [JsonProperty("private_game_code")]
        public string PrivateGameCode { get; set; }
    }
}