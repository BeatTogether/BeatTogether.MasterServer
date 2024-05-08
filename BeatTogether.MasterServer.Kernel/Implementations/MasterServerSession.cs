﻿using System;
using System.Net;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Messaging.Enums;
using Org.BouncyCastle.Crypto.Parameters;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MasterServerSession// : BaseSession
    {
        public MasterServerSessionState State { get; set; }
        public Platform Platform { get; set; }
        public string PlatformUserId { get; set; }
        public string ClientVersion { get; set; } = null;
        public string UserIdHash { get; set; }
        public string UserName { get; set; }
        public string PlayerSessionId { get; set; } = null;
        public byte[] Cookie { get; set; }
        public byte[] ClientRandom { get; set; }
        public byte[] ServerRandom { get; set; }
        public byte[] ClientPublicKey { get; set; }
        public ECPublicKeyParameters ClientPublicKeyParameters { get; set; }
        public ECPrivateKeyParameters ServerPrivateKeyParameters { get; set; }
        public byte[] PreMasterSecret { get; set; }
        public DateTimeOffset LastKeepAlive { get; set; }
        public MasterServerSession(EndPoint endPoint)
            //: base(endPoint)
        {
        }
    }
}
