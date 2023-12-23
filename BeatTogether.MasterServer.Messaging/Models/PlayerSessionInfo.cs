using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public class PlayerSessionInfo
    {
        [JsonProperty("player_session_id")]
        public string PlayerSessionId { get; set; } = "";

        [JsonProperty("game_session_id")]
        public string GameSessionId { get; set; } = "";

        [JsonProperty("dns_name")]
        public string DnsName { get; set; } = "";

        [JsonProperty("port")]
        public int Port { get; set; } = -1;

        [JsonProperty("beatmap_level_selection_mask")]
        public BeatmapLevelSelectionMaskSimple BeatmapLevelSelectionMask { get; set; } = new();

        [JsonProperty("gameplay_server_configuration")]
        public GameplayServerConfiguration GameplayServerConfiguration { get; set; } = new();

        [JsonProperty("private_game_secret")]
        public string PrivateGameSecret { get; set; } = "";
        
        [JsonProperty("private_game_code")]
        public string PrivateGameCode { get; set; } = "";
    }
}