using System.Net;

namespace BeatTogether.MasterServer.Kernel.Abstractions.Sessions
{
    public interface ISessionService
    {
        ISession OpenSession(Implementations.MasterServer masterServer, EndPoint endPoint);
        bool CloseSession(ISession session);

        ISession GetSession(EndPoint endPoint);
        bool TryGetSession(EndPoint endPoint, out ISession session);
    }
}
