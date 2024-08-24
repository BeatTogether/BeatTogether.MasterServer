using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public sealed class ExtraServerConfiguration
    {
        [JsonProperty("permenant_manager")]
        public bool? PermenantManger { get; set; }

        [JsonProperty("instance_destroy_timeout")]
        public long? Timeout { get; set; }

        [JsonProperty("server_name")]
        public string? ServerName { get; set; }

        [JsonProperty("lock_in_beatmap_time")]
        public long? BeatmapStartTime { get; set; }

        [JsonProperty("countdown_time")]
        public long? PlayersReadyCountdownTime { get; set; }

        [JsonProperty("results_time")]
        public long? ResultsScreenTime { get; set; }

        [JsonProperty("per_player_modifiers")]
        public bool? AllowPerPlayerModifiers { get; set; }

        [JsonProperty("per_player_difficulties")]
        public bool? AllowPerPlayerDifficulties { get; set; }

        [JsonProperty("enable_chroma")]
        public bool? AllowChroma { get; set; }

        [JsonProperty("enable_mapping_extensions")]
        public bool? AllowME { get; set; }

        [JsonProperty("enable_noodle_extensions")]
        public bool? AllowNE { get; set; }
    }
}
