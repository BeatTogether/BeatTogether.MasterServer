using System.Threading.Tasks;
using BeatTogether.MasterServer.Domain.Models;

namespace BeatTogether.MasterServer.Data.Abstractions.Repositories
{
    public interface IServerRepository
    {
        Task<Server> GetServer(string secret);
        Task<Server> GetServerByCode(string code);
        Task<Server> GetAvailablePublicServer();

        Task<bool> AddServer(Server server);
        Task<bool> RemoveServer(string secret);

        Task<bool> IncrementCurrentPlayerCount(string secret);
        void UpdateCurrentPlayerCount(string secret, int currentPlayerCount);
    }
}
