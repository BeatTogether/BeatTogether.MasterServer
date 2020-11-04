using System.Collections.Generic;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake
{
    public class ServerCertificateRequest : BaseReliableResponse
    {
        public List<byte[]> Certificates { get; set; }

        public override void WriteTo(ref GrowingSpanBuffer buffer)
        {
            base.WriteTo(ref buffer);

            buffer.WriteVarUInt((uint)Certificates.Count);
            foreach (var certificate in Certificates)
                buffer.WriteVarBytes(certificate);
        }

        public override void ReadFrom(ref SpanBufferReader bufferReader)
        {
            base.ReadFrom(ref bufferReader);

            Certificates = new List<byte[]>();
            var certificateCount = bufferReader.ReadVarUInt();
            for (var i = 0; i < certificateCount; i++)
                Certificates.Add(bufferReader.ReadVarBytes().ToArray());
        }
    }
}
