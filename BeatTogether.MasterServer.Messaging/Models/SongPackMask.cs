using BeatTogether.Core.Messaging.Abstractions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public sealed class SongPackMask : IMessage
    {
        public ulong Top { get; set; }
        public ulong Bottom { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteUInt64(Top);
            bufferWriter.WriteUInt64(Bottom);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Top = bufferReader.ReadUInt64();
            Bottom = bufferReader.ReadUInt64();
        }
    }
}
