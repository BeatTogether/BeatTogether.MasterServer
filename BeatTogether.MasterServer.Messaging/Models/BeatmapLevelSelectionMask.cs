using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Enums;
using Krypton.Buffers;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public sealed class BeatmapLevelSelectionMask : IMessage
    {
        [JsonProperty("difficulties")]
        public BeatmapDifficultyMask BeatmapDifficultyMask { get; set; }
        
        [JsonProperty("modifiers")]
        public GameplayModifiersMask GameplayModifiersMask { get; set; }
        
        [JsonProperty("song_packs")]
        public SongPackMask SongPackMask { get; set; } = new();

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteUInt8((byte)BeatmapDifficultyMask);
            bufferWriter.WriteUInt32((uint)GameplayModifiersMask);
            SongPackMask.WriteTo(ref bufferWriter);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            BeatmapDifficultyMask = (BeatmapDifficultyMask)bufferReader.ReadByte();
            GameplayModifiersMask = (GameplayModifiersMask)bufferReader.ReadUInt32();
            SongPackMask.ReadFrom(ref bufferReader);
        }
    }
}
