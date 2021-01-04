using System;
using System.Collections.Generic;
using System.Net;
using BeatTogether.MasterServer.Kernel.Implementations;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMasterServerSessionService
    {
        IEnumerable<MasterServerSession> GetInactiveSessions(int timeToLive);
        MasterServerSession GetOrAddSession(EndPoint endPoint);
        MasterServerSession GetSession(EndPoint endPoint);
        bool TryGetSession(EndPoint endPoint, out MasterServerSession session);
        bool CloseSession(MasterServerSession session);
    }
}
