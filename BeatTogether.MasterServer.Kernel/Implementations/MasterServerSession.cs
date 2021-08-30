using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using BeatTogether.Core.Messaging.Implementations;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Messaging.Enums;
using Org.BouncyCastle.Crypto.Parameters;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MasterServerSession : BaseSession
    {
        public MasterServerSessionState State { get; set; }
        public Platform Platform { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Secret { get; set; }

        public byte[] Cookie { get; set; }
        public byte[] ClientRandom { get; set; }
        public byte[] ServerRandom { get; set; }
        public byte[] ClientPublicKey { get; set; }
        public ECPublicKeyParameters ClientPublicKeyParameters { get; set; }
        public ECPrivateKeyParameters ServerPrivateKeyParameters { get; set; }
        public byte[] PreMasterSecret { get; set; }
        public DateTimeOffset LastKeepAlive { get; set; }

        public MasterServerSession(EndPoint endPoint)
            : base(endPoint)
        {
        }

        public string GetGameUserId()
        {
            string platformStr = Platform switch
            {
                Platform.Test => "Test#",
                Platform.Oculus => "Oculus#",
                Platform.OculusQuest => "Oculus#",
                Platform.Steam => "Steam#",
                Platform.PS4 => "PSN#",
                _ => ""
            };

            return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(platformStr+UserId))).Substring(0, 22);
        }
    }
}
