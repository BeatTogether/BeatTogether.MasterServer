using System.Net;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Domain.Enums;
using BeatTogether.MasterServer.Domain.Models;

namespace BeatTogether.MasterServer.Data.Abstractions.Repositories
{
    public interface IServerRepository
    {
        Task<Server> GetServer(string secret);
        Task<Server> GetServerByCode(string code);
        Task<Server> GetAvailablePublicServer(
            InvitePolicy invitePolicy,
            GameplayServerMode serverMode,
            SongSelectionMode songMode,
            GameplayServerControlSettings serverControlSettings,
            BeatmapDifficultyMask difficultyMask,
            GameplayModifiersMask modifiersMask,
            ulong songPackTop,
            ulong songPackBottom);

        Task<string[]> GetPublicServerSecretsList();
        Task<Server[]> GetPublicServerList();
        Task<string[]> GetServerSecretsList();
        Task<Server[]> GetServerList();
        Task<int> GetPublicServerCount();
        Task<int> GetServerCount();

        Task<bool> AddServer(Server server);
        Task<bool> RemoveServer(string secret);
        Task<bool> RemoveServersWithEndpoint(IPAddress EndPoint);

        Task<bool> IncrementCurrentPlayerCount(string secret);
        Task<bool> DecrementCurrentPlayerCount(string secret);
        void UpdateCurrentPlayerCount(string secret, int currentPlayerCount);
    }
}
