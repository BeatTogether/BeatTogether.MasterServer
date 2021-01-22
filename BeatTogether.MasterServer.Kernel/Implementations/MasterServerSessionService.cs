using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Enums;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MasterServerSessionService : IMasterServerSessionService
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<EndPoint, MasterServerSession> _sessions;

        public MasterServerSessionService()
        {
            _logger = Log.ForContext<MasterServerSessionService>();
            _sessions = new ConcurrentDictionary<EndPoint, MasterServerSession>();
        }

        #region Public Methods

        public IEnumerable<MasterServerSession> GetInactiveSessions(int timeToLive) =>
            _sessions.Values.ToList()
                .Where(session => (DateTimeOffset.UtcNow - session.LastKeepAlive).TotalSeconds > timeToLive);

        public MasterServerSession GetOrAddSession(EndPoint endPoint) =>
            _sessions.GetOrAdd(
                endPoint,
                key =>
                {
                    _logger.Verbose($"Opening session (EndPoint='{endPoint}').");
                    return new MasterServerSession(key)
                    {
                        State = MasterServerSessionState.New,
                        LastKeepAlive = DateTimeOffset.UtcNow
                    };
                }
            );

        public MasterServerSession GetSession(EndPoint endPoint) =>
            _sessions[endPoint];

        public bool TryGetSession(EndPoint endPoint, [MaybeNullWhen(false)] out MasterServerSession session) =>
            _sessions.TryGetValue(endPoint, out session);

        public bool CloseSession(MasterServerSession session)
        {
            if (!_sessions.TryRemove(session.EndPoint, out _))
                return false;

            if (session.State == MasterServerSessionState.Authenticated)
                _logger.Information(
                    "Closing session " +
                    $"(EndPoint='{session.EndPoint}', " +
                    $"Platform={session.Platform}, " +
                    $"UserId='{session.UserId}', " +
                    $"UserName='{session.UserName}')."
                );
            else
                _logger.Information($"Closing session (EndPoint='{session.EndPoint}').");
            session.State = MasterServerSessionState.None;
            return true;
        }

        #endregion
    }
}
