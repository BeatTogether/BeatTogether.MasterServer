using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Entities;

namespace BeatTogether.MasterServer.Data.Abstractions.Repositories
{
    public interface IServerRepository
    {
        void AddServer(Server server);
        void RemoveServerWithUserId(string userId);
        void RemoveServerWithCode(string code);

        Task<Server> GetServerWithCodeAndAddPlayer(string code, string userId, string userName);
        Task<Server> GetAvailablePublicServerAndAddPlayer(string userId, string userName);
        void RemovePlayerWithUserId(string userId);
        void RemovePlayerWithCode(string code);
    }
}
