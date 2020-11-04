using System.Collections.Concurrent;
using System.Net;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Models;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class SessionService : ISessionService
    {
        private readonly ConcurrentDictionary<EndPoint, Session> _sessionsByEndPointLookup;

        public SessionService()
        {
            _sessionsByEndPointLookup = new ConcurrentDictionary<EndPoint, Session>();
        }

        public bool AddSession(Session session)
            => _sessionsByEndPointLookup.TryAdd(session.EndPoint, session);

        public bool RemoveSession(Session session)
            => _sessionsByEndPointLookup.TryRemove(session.EndPoint, out _);

        public Session GetSession(EndPoint endPoint)
            => _sessionsByEndPointLookup[endPoint];

        public bool TryGetSession(EndPoint endPoint, out Session session)
            => _sessionsByEndPointLookup.TryGetValue(endPoint, out session);
    }
}
