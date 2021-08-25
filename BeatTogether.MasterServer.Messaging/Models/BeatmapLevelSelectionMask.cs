using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Enums;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public sealed class BeatmapLevelSelectionMask : IMessage
    {
        public BeatmapDifficultyMask BeatmapDifficultyMask { get; set; }
        public GameplayModifiersMask GameplayModifiersMask { get; set; }
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
