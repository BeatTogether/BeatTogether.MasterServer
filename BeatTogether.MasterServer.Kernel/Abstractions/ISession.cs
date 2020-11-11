using System.Net;
using System.Security.Cryptography;
using System.Threading.Channels;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using BeatTogether.MasterServer.Messaging.Enums;
using Org.BouncyCastle.Crypto.Parameters;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface ISession
    {
        Implementations.MasterServer MasterServer { get; }

        EndPoint EndPoint { get; }
        SessionState State { get; set; }

        Platform Platform { get; set; }
        string UserId { get; set; }
        string UserName { get; set; }
        string Secret { get; set; }

        byte[] Cookie { get; set; }
        byte[] ClientRandom { get; set; }
        byte[] ServerRandom { get; set; }
        ECPrivateKeyParameters ServerPrivateKeyParameters { get; set; }
        ECPublicKeyParameters ClientPublicKeyParameters { get; set; }
        byte[] PreMasterSecret { get; set; }
        byte[] ReceiveKey { get; set; }
        byte[] SendKey { get; set; }
        HMACSHA256 ReceiveMac { get; set; }
        HMACSHA256 SendMac { get; set; }
        uint LastSentSequenceId { get; set; }

        Channel<IMessage> MessageReceiveChannel { get; }
    }
}
