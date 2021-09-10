using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.MasterServer.Messaging.Enums;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public sealed class GameplayServerConfiguration : IMessage
    {
        public int MaxPlayerCount { get; set; }
        public DiscoveryPolicy DiscoveryPolicy { get; set; }
        public InvitePolicy InvitePolicy { get; set; }
        public GameplayServerMode GameplayServerMode { get; set; }
        public SongSelectionMode SongSelectionMode { get; set; }
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
