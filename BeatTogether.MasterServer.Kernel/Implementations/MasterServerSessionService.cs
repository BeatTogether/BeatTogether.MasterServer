using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Autobus;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Interface.Events;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Enums;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class MasterServerSessionService : IMasterServerSessionService
    {
        private readonly IAutobus _autobus;
        private readonly ILogger _logger = Log.ForContext<MasterServerSessionService>();
        private readonly IServerRepository _serverRepository;

        private readonly ConcurrentDictionary<EndPoint, MasterServerSession> _sessions = new();

        public MasterServerSessionService(IAutobus autobus, IServerRepository serverRepository)
        {
            _autobus = autobus;
            _serverRepository = serverRepository;
        }

        #region Public Methods

        public MasterServerSession[] GetMasterServerSessions()
        {
            return _sessions.Values.ToArray();
        }

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
            _serverRepository.DecrementCurrentPlayerCount(session.Secret);
            session.State = MasterServerSessionState.None;
            return true;
        }

        public Task<bool> CloseSession(EndPoint sessionEndpoint)
        {
            if (!_sessions.TryRemove(sessionEndpoint, out var serverSession))
                return Task.FromResult(false);

            if (serverSession.State == MasterServerSessionState.Authenticated)
                _logger.Information(
                    "Closing session " +
                    $"(EndPoint='{serverSession.EndPoint}', " +
                    $"Platform={serverSession.Platform}, " +
                    $"UserId='{serverSession.UserId}', " +
                    $"UserName='{serverSession.UserName}')."
                );
            else
                _logger.Information($"Closing session (EndPoint='{serverSession.EndPoint}').");
            _serverRepository.DecrementCurrentPlayerCount(serverSession.Secret);
            serverSession.State = MasterServerSessionState.None;
            return Task.FromResult(true);
        }

        #endregion
    }
}
