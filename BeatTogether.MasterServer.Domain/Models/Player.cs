using BeatTogether.Core.Abstractions;
using BeatTogether.Core.Enums;
using System;

namespace BeatTogether.MasterServer.Domain.Models
{
    public sealed class Player : IPlayer
    {
        public string HashedUserId { get; set; }
        public string PlatformUserId { get; set; }
        public string PlayerSessionId { get; set; }
        public Platform PlayerPlatform { get; set; }
        public Version PlayerClientVersion { get; set; }
    }
}
