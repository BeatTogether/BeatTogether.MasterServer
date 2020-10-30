using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Data.Entities;

namespace BeatTogether.MasterServer.Data.Implementations.Repositories
{
    public class ServerRepository : IServerRepository
    {
        public void AddServer(Server server)
        {
            throw new System.NotImplementedException();
        }

        public Task<Server> GetAvailablePublicServerAndAddPlayer(string userId, string userName)
        {
            throw new System.NotImplementedException();
        }

        public Task<Server> GetServerWithCodeAndAddPlayer(string code, string userId, string userName)
        {
            throw new System.NotImplementedException();
        }

        public void RemovePlayerWithCode(string code)
        {
            throw new System.NotImplementedException();
        }

        public void RemovePlayerWithUserId(string userId)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveServerWithCode(string code)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveServerWithUserId(string userId)
        {
            throw new System.NotImplementedException();
        }
    }
}
