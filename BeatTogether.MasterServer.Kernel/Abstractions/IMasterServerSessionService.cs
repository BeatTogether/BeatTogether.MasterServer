using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Implementations;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface IMasterServerSessionService
    {
        IEnumerable<MasterServerSession> GetInactiveSessions(int timeToLive);
        MasterServerSession[] GetMasterServerSessions();
        MasterServerSession GetOrAddSession(EndPoint endPoint);
        void AddSession(EndPoint endPoint, string Secret);
        MasterServerSession GetSession(EndPoint endPoint);
        bool TryGetSession(EndPoint endPoint, [MaybeNullWhen(false)] out MasterServerSession session);
        bool CloseSession(MasterServerSession session);
        bool CloseSession(EndPoint sessionEndPoint);
        void RemoveSecretFromSession(EndPoint sessionEndpoint);
    }
}
