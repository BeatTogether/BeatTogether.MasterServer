using System.Net;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.User
{
    public class PrepareForConnectionRequest : BaseReliableRequest
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public bool IsConnectionOwner { get; set; }
        public bool IsDedicatedServer { get; set; }

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            buffer.WriteString(UserId);
            buffer.WriteString(UserName);
            buffer.WriteIPEndPoint(RemoteEndPoint);
            buffer.WriteBytes(Random);
            buffer.WriteVarBytes(PublicKey);
            buffer.WriteUInt8((byte)((IsConnectionOwner ? 1 : 0) | (IsDedicatedServer ? 2 : 0)));
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            RemoteEndPoint = bufferReader.ReadIPEndPoint();
            Random = bufferReader.ReadBytes(32).ToArray();
            PublicKey = bufferReader.ReadVarBytes().ToArray();
            var flags = bufferReader.ReadUInt8();
            IsConnectionOwner = (flags & 1) > 0;
            IsDedicatedServer = (flags & 2) > 0;
        }
    }
}
