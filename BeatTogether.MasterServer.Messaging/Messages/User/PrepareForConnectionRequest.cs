using System.Net;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public sealed class PrepareForConnectionRequest
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public bool IsConnectionOwner { get; set; }
        public bool IsDedicatedServer { get; set; }
    }
}
