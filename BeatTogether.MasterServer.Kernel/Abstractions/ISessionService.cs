using System.Net;
using BeatTogether.MasterServer.Kernel.Models;

namespace BeatTogether.MasterServer.Kernel.Abstractions
{
    public interface ISessionService
    {
        bool AddSession(Session session);
        bool RemoveSession(Session session);

        Session GetSession(EndPoint endPoint);
        bool TryGetSession(EndPoint endPoint, out Session session);
    }
}
