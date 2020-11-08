using System.Collections.Concurrent;
using System.Net;
using BeatTogether.MasterServer.Kernel.Abstractions;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class SessionService : ISessionService
    {
        private readonly ConcurrentDictionary<EndPoint, ISession> _sessionsByEndPointLookup;

        public SessionService()
        {
            _sessionsByEndPointLookup = new ConcurrentDictionary<EndPoint, ISession>();
        }

        public bool AddSession(ISession session)
            => _sessionsByEndPointLookup.TryAdd(session.EndPoint, session);

        public bool RemoveSession(ISession session)
            => _sessionsByEndPointLookup.TryRemove(session.EndPoint, out _);

        public ISession GetSession(EndPoint endPoint)
            => _sessionsByEndPointLookup[endPoint];

        public bool TryGetSession(EndPoint endPoint, out ISession session)
            => _sessionsByEndPointLookup.TryGetValue(endPoint, out session);
    }
}
