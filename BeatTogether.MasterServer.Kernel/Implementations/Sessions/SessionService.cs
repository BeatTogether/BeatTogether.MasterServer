using System;
using System.Collections.Concurrent;
using System.Net;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Kernel.Enums;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations.Sessions
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<EndPoint, ISession> _sessions;

        public SessionService(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
            _logger = Log.ForContext<SessionService>();

            _sessions = new ConcurrentDictionary<EndPoint, ISession>();
        }

        #region Public Methods

        public ISession OpenSession(MasterServer masterServer, EndPoint endPoint)
        {
            bool isNewSession = false;
            var session = _sessions.GetOrAdd(endPoint, key =>
            {
                isNewSession = true;
                return new Session(masterServer, key);
            });
            if (!isNewSession)
                return session;

            _logger.Information($"Opening session (EndPoint='{session.EndPoint}').");
            _sessionRepository.AddSession(endPoint);
            session.State = SessionState.New;
            return session;
        }

        public bool CloseSession(ISession session)
        {
            if (!_sessions.TryRemove(session.EndPoint, out _))
                return false;

            if (session.State == SessionState.Authenticated)
                _logger.Information(
                    "Closing session " +
                    $"(EndPoint='{session.EndPoint}', " +
                    $"Platform={session.Platform}, " +
                    $"UserId='{session.UserId}', " +
                    $"UserName='{session.UserName}')."
                );
            else
                _logger.Information($"Closing session (EndPoint='{session.EndPoint}').");
            _sessionRepository.RemoveSession(session.EndPoint);
            session.State = SessionState.None;
            return true;
        }

        public ISession GetSession(EndPoint endPoint)
            => _sessions[endPoint];

        public bool TryGetSession(EndPoint endPoint, out ISession session)
            => _sessions.TryGetValue(endPoint, out session);

        #endregion
    }
}
