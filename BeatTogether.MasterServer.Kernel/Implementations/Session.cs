using System.Net;
using System.Security.Cryptography;
using System.Threading;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Messaging.Enums;
using Org.BouncyCastle.Crypto.Parameters;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class Session : ISession
    {
        public MasterServer MasterServer { get; }

        public EndPoint EndPoint { get; }
        public SessionState State { get; set; }

        public Platform Platform { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Secret { get; set; }

        public uint Epoch { get; set; }
        public byte[] Cookie { get; set; }
        public byte[] ClientRandom { get; set; }
        public byte[] ServerRandom { get; set; }
        public ECPrivateKeyParameters ServerPrivateKeyParameters { get; set; }
        public ECPublicKeyParameters ClientPublicKeyParameters { get; set; }
        public byte[] PreMasterSecret { get; set; }
        public byte[] ReceiveKey { get; set; }
        public byte[] SendKey { get; set; }
        public HMACSHA256 ReceiveMac { get; set; }
        public HMACSHA256 SendMac { get; set; }
        public uint LastSentSequenceId { get; set; }

        private uint _lastRequestId = 0;

        public Session(MasterServer masterServer, EndPoint endPoint)
        {
            MasterServer = masterServer;
            EndPoint = endPoint;
        }

        public uint GetNextRequestId()
            => (unchecked(Interlocked.Increment(ref _lastRequestId)) % 16777216) | Epoch;
    }
}
