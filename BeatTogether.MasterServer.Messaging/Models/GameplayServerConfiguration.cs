using BeatTogether.MasterServer.Messaging.Enums;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public sealed class GameplayServerConfiguration
    {
        [JsonProperty("max_player_count")]
        public int MaxPlayerCount { get; set; }
        
        [JsonProperty("discovery_policy")]
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        
        [JsonProperty("invite_policy")]
        public InvitePolicy InvitePolicy { get; set; }
        
        [JsonProperty("gameplay_server_mode")]
        public GameplayServerMode GameplayServerMode { get; set; }
        
        [JsonProperty("song_selection_mode")]
        public SongSelectionMode SongSelectionMode { get; set; }
        
        [JsonProperty("gameplay_server_control_settings")]
        public GameplayServerControlSettings GameplayServerControlSettings { get; set; }
    }
}
