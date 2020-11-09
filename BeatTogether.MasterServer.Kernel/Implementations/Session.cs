using System;
using System.Net;
using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Messaging.Enums;
using Org.BouncyCastle.Crypto.Parameters;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class Session : ISession
    {
        public EndPoint EndPoint { get; }
        public SessionState State { get; set; }
        public Platform Platform { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte[] Cookie { get; set; }
        public byte[] ClientRandom { get; set; }
        public byte[] ServerRandom { get; set; }
        public ECPrivateKeyParameters ServerPrivateKeyParameters { get; set; }
        public ECPublicKeyParameters ClientPublicKeyParameters { get; set; }
        public byte[] PreMasterSecret { get; set; }
        public byte[] ReceiveKey { get; set; }
        public byte[] SendKey { get; set; }
        public HMACSHA256 SendMac { get; set; }
        public HMACSHA256 ReceiveMac { get; set; }
        public uint LastSentSequenceId { get; set; }

        private readonly MasterServer _masterServer;

        public Session(MasterServer masterServer, EndPoint endPoint)
        {
            _masterServer = masterServer;

            EndPoint = endPoint;
            State = SessionState.New;
        }

        public void Send(ReadOnlySpan<byte> buffer)
            => _masterServer.SendAsync(this, buffer);

        public void SendEncrypted(ReadOnlySpan<byte> buffer)
            => _masterServer.SendEncryptedAsync(this, buffer);
    }
}
