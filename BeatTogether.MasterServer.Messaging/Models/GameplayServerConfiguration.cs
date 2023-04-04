using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.MasterServer.Messaging.Enums;
using Krypton.Buffers;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public sealed class GameplayServerConfiguration : IMessage
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

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteVarInt(MaxPlayerCount);
            bufferWriter.WriteVarInt((int)DiscoveryPolicy);
            bufferWriter.WriteVarInt((int)InvitePolicy);
            bufferWriter.WriteVarInt((int)GameplayServerMode);
            bufferWriter.WriteVarInt((int)SongSelectionMode);
            bufferWriter.WriteVarInt((int)GameplayServerControlSettings);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            MaxPlayerCount = bufferReader.ReadVarInt();
            DiscoveryPolicy = (DiscoveryPolicy)bufferReader.ReadVarInt();
            InvitePolicy = (InvitePolicy)bufferReader.ReadVarInt();
            GameplayServerMode = (GameplayServerMode)bufferReader.ReadVarInt();
            SongSelectionMode = (SongSelectionMode)bufferReader.ReadVarInt();
            GameplayServerControlSettings = (GameplayServerControlSettings)bufferReader.ReadVarInt();
        }
    }
}
