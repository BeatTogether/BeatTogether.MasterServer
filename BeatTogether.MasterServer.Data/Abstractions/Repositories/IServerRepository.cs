using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Entities;

namespace BeatTogether.MasterServer.Data.Abstractions.Repositories
{
    public interface IServerRepository
    {
        Task<Server> GetServer(string secret);
        Task<Server> GetServerByCode(string code);

        Task<bool> AddServer(Server server);
        Task<bool> RemoveServer(string secret);

        Task<Server> GetAvailablePublicServerAndAddPlayer();

        Task<bool> IncrementCurrentPlayerCount(string secret);
        void UpdateCurrentPlayerCount(string secret, int currentPlayerCount);
    }
}
