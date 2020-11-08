using System.Net;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface ISessionService
    {
        bool AddSession(ISession session);
        bool RemoveSession(ISession session);

        ISession GetSession(EndPoint endPoint);
        bool TryGetSession(EndPoint endPoint, out ISession session);
    }
}
