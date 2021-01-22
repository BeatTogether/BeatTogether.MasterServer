using System.Collections.Generic;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Messages.Handshake
{
    public class ServerCertificateRequest : IMessage, IReliableRequest, IReliableResponse
    {
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public List<byte[]> Certificates { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteVarUInt((uint)Certificates.Count);
            foreach (var certificate in Certificates)
                bufferWriter.WriteVarBytes(certificate);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Certificates = new List<byte[]>();
            var certificateCount = bufferReader.ReadVarUInt();
            for (var i = 0; i < certificateCount; i++)
                Certificates.Add(bufferReader.ReadVarBytes().ToArray());
        }
    }
}
