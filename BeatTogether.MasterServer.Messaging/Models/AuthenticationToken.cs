﻿using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Extensions;
using BeatTogether.MasterServer.Messaging.Enums;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public class AuthenticationToken : IMessage
    {
        public Platform Platform { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte[] SessionToken { get; set; }

        public void WriteTo(ref GrowingSpanBuffer buffer)
        {
            buffer.WriteUInt8((byte)Platform);
            buffer.WriteString(UserId);
            buffer.WriteString(UserName);
            buffer.WriteVarBytes(SessionToken);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Platform = (Platform)bufferReader.ReadByte();
            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            SessionToken = bufferReader.ReadVarBytes().ToArray();
        }
    }
}
