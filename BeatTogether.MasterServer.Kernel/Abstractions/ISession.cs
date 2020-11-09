using System;
using System.Net;
using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Messaging.Enums;
using Org.BouncyCastle.Crypto.Parameters;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface ISession
    {
        EndPoint EndPoint { get; }
        SessionState State { get; set; }
        Platform Platform { get; set; }
        string UserId { get; set; }
        string UserName { get; set; }
        byte[] Cookie { get; set; }
        byte[] ClientRandom { get; set; }
        byte[] ServerRandom { get; set; }
        ECPrivateKeyParameters ServerPrivateKeyParameters { get; set; }
        ECPublicKeyParameters ClientPublicKeyParameters { get; set; }
        byte[] PreMasterSecret { get; set; }
        byte[] ReceiveKey { get; set; }
        byte[] SendKey { get; set; }
        HMACSHA256 SendMac { get; set; }
        HMACSHA256 ReceiveMac { get; set; }
        uint LastSentSequenceId { get; set; }

        void Send(ReadOnlySpan<byte> buffer);
        void SendEncrypted(ReadOnlySpan<byte> buffer);
    }
}
