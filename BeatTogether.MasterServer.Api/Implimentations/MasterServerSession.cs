using System;
using BeatTogether.Core.Abstractions;
using BeatTogether.Core.Enums;

namespace BeatTogether.MasterServer.Api.Implementations
{
    public class MasterServerSession : IPlayer
    {
        public string PlatformUserId { get; set; }
        public string PlayerSessionId { get; set; }
        public string HashedUserId { get; set; }
        public Platform PlayerPlatform { get; set; }
        public Version PlayerClientVersion { get; set; }

        public byte[] SessionToken;
        public DateTimeOffset LastKeepAlive { get; set; }
        public MasterServerSessionState State { get; set; }

        public MasterServerSession(string playerSessionId)
        {
            PlayerSessionId = playerSessionId;
        }
    }

    public enum MasterServerSessionState
    {
        None = 0,
        New = 1,
        Established = 2,
        Authenticated = 3
    }
}
