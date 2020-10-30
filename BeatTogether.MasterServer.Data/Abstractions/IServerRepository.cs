using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Entities;

namespace BeatTogether.MasterServer.Data.Abstractions
{
    public interface IServerRepository
    {
        void AddServer(ServerEntity server);
        void RemoveServerWithUserId(string userId);
        void RemoveServerWithCode(string code);

        Task<ServerEntity> GetServerWithCodeAndAddPlayer(string code, string userId, string userName);
        Task<ServerEntity> GetAvailablePublicServerAndAddPlayer(string userId, string userName);
        void RemovePlayerWithUserId(string userId);
        void RemovePlayerWithCode(string code);
    }
}
