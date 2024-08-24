using System.Net;
using BeatTogether.MasterServer.Messaging.Models;

namespace BeatTogether.MasterServer.Messaging.Messages.User
{
    public enum ConnectToServerResult : byte
    {
        Success,
        InvalidSecret,
        InvalidCode,
        InvalidPassword,
        ServerAtCapacity,
        NoAvailableDedicatedServers,
        VersionMismatch,
        ConfigMismatch,
        UnknownError
    }

    public sealed class ConnectToServerResponse
    {
        public uint SequenceId { get; set; }
        public uint RequestId { get; set; }
        public uint ResponseId { get; set; }
        public ConnectToServerResult Result { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Secret { get; set; }
        public BeatmapLevelSelectionMask BeatmapLevelSelectionMask { get; set; } = new();
        public bool IsConnectionOwner { get; set; }
        public bool IsDedicatedServer { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }
        public byte[] Random { get; set; }
        public byte[] PublicKey { get; set; }
        public string Code { get; set; }
        public GameplayServerConfiguration Configuration { get; set; } = new();
        public string ManagerId { get; set; }
        public bool Success => Result == ConnectToServerResult.Success;
    }
}
