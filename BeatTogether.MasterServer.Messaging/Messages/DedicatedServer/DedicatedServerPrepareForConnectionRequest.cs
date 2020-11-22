using System.Net;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.DedicatedServer
{
    public class DedicatedServerPrepareForConnectionRequest : IEncryptedMessage, IReliableRequest
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteString(Id);
            buffer.WriteString(UserId);
            buffer.WriteString(UserName);
            buffer.WriteIPEndPoint(RemoteEndPoint);
            buffer.WriteBytes(Random);
            buffer.WriteVarBytes(PublicKey);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Id = bufferReader.ReadString();
            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            RemoteEndPoint = bufferReader.ReadIPEndPoint();
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
