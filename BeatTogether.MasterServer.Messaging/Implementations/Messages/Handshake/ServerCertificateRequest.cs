using System.Collections.Generic;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class ServerCertificateRequest : BaseReliableResponse
    {
        public List<byte[]> Certificates { get; set; }

        public override void WriteTo(GrowingSpanBuffer buffer)
        {
            base.WriteTo(buffer);

            buffer.WriteVarUInt((uint)Certificates.Count);
            foreach (var certificate in Certificates)
                buffer.WriteVarBytes(certificate);
        }

        public override void ReadFrom(SpanBufferReader bufferReader)
        {
            base.ReadFrom(bufferReader);

            Certificates = new List<byte[]>();
            var certificateCount = bufferReader.ReadVarUInt();
            for (var i = 0; i < certificateCount; i++)
                Certificates.Add(bufferReader.ReadVarBytes().ToArray());
        }
    }
}
