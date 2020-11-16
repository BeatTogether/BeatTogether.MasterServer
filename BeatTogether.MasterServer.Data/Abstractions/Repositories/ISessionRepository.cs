using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace BeatTogether.MasterServer.Data.Abstractions.Repositories
{
    public interface ISessionRepository
    {
        void AddSession(EndPoint endPoint);
        void RemoveSession(EndPoint endPoint);
        void UpdateLastKeepAlive(EndPoint endPoint, DateTimeOffset lastKeepAlive);
        Task<IEnumerable<EndPoint>> GetInactiveSessions(long timeToLive);
    }
}
