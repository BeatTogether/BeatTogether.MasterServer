using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Messaging.Enums;
using Org.BouncyCastle.Crypto.Parameters;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations.Sessions
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

        private uint _lastSentSequenceId = 0;
        private uint _lastSentRequestId = 0;
        private HashSet<uint> _handledRequests { get; set; } = new HashSet<uint>();
        private object _handledRequestsLock { get; set; } = new object();
        private uint _lastHandledRequestId = 0;

        public Session(MasterServer masterServer, EndPoint endPoint)
        {
            MasterServer = masterServer;
            EndPoint = endPoint;
        }

        public uint GetNextSequenceId()
            => unchecked(Interlocked.Increment(ref _lastSentSequenceId));

        public uint GetNextRequestId()
            => (unchecked(Interlocked.Increment(ref _lastSentRequestId)) % 16777216) | Epoch;

        public bool ShouldHandleRequest(uint requestId)
        {
            lock (_handledRequestsLock)
            {
                if (_handledRequests.Add(requestId))
                {
                    if (_handledRequests.Count > 64)
                        _handledRequests.Remove(_lastHandledRequestId);
                    _lastHandledRequestId = requestId;
                    return true;
                }
            }
            return false;
        }
    }
}
