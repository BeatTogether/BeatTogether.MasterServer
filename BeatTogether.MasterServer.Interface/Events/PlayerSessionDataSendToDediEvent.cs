using BeatTogether.Core.ServerMessaging.Models;

namespace BeatTogether.MasterServer.Interface.Events
{
    public record PlayerSessionDataSendToDediEvent(
        string NodeEndpoint,
        string serverInstanceSecret,
        Player Player);
}