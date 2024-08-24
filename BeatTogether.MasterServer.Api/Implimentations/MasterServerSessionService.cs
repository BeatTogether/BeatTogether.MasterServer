using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BeatTogether.MasterServer.Api.Abstractions;
using Serilog;

namespace BeatTogether.MasterServer.Api.Implementations
{
    public class MasterServerSessionService : IMasterServerSessionService
    {
        private readonly ILogger _logger = Log.ForContext<MasterServerSessionService>();
        private readonly ConcurrentDictionary<string, MasterServerSession> _sessions = new();

        #region Public Methods

        public MasterServerSession[] GetMasterServerSessions()
        {
            return _sessions.Values.ToArray();
        }

        public IEnumerable<MasterServerSession> GetInactiveSessions(int timeToLive) =>
            _sessions.Values.ToList()
                .Where(session => (DateTimeOffset.UtcNow - session.LastKeepAlive).TotalSeconds > timeToLive);

        public MasterServerSession GetOrAddSession(string playerSessionId) =>
            _sessions.GetOrAdd(
                playerSessionId,
                key =>
                {
                    _logger.Verbose($"Opening session (playerSessionId='{playerSessionId}').");
                    return new MasterServerSession(playerSessionId)
                    {
                        State = MasterServerSessionState.New,
                        LastKeepAlive = DateTimeOffset.UtcNow
                    };
                }
            );

        public bool TryGetSession(string playerSessionId, [MaybeNullWhen(false)] out MasterServerSession session) =>
            _sessions.TryGetValue(playerSessionId, out session);

        public bool CloseSession(MasterServerSession session)
        {
            if (!_sessions.TryRemove(session.PlayerSessionId, out var _))
                return false;
            if (session.State == MasterServerSessionState.Authenticated)
                _logger.Information(
                    "Closing session " +
                    $"(PlayerSessionId='{session.PlayerSessionId}', " +
                    $"Platform={session.PlayerPlatform}, " +
                    $"UserId='{session.HashedUserId}', " +
                    $"ClientVersion='{session.PlayerClientVersion}', " +
                    $"PlatformUserId='{session.PlatformUserId}')."
                );
            else
                _logger.Information($"Closing session (PlayerSessionId='{session.PlayerSessionId}').");
            session.State = MasterServerSessionState.None;
            return true;
        }

        #endregion
    }
}