using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BeatTogether.MasterServer.Api.Implementations;

namespace BeatTogether.MasterServer.Api.Abstractions
{
    public interface IMasterServerSessionService
    {
        IEnumerable<MasterServerSession> GetInactiveSessions(int timeToLive);
        MasterServerSession[] GetMasterServerSessions();
        MasterServerSession GetOrAddSession(string playerSessionId);
        bool TryGetSession(string playerSessionId, [MaybeNullWhen(false)] out MasterServerSession session);
        bool CloseSession(MasterServerSession session);
    }
}
