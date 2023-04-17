﻿using System.Net;
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

        Task<string[]> GetPublicServerSecrets();
        Task<string[]> GetPublicServerCodes();
        Task<Server[]> GetPublicServerList();
        Task<Server[]> GetServerList();
        Task<int> GetPublicServerCount();
        Task<int> GetServerCount();
        Task<int> GetPlayerCount();
        Task<bool> AddServer(Server server);
        Task<bool> RemoveServer(string secret);
        Task<bool> RemoveServersWithEndpoint(IPAddress EndPoint);

        Task<int> GetServerCountOnEndpoint(IPAddress EndPoint);
        Task<int> GetPlayerCountOnEndpoint(IPAddress EndPoint);

        Task<bool> IncrementCurrentPlayerCount(string secret);
        Task<bool> DecrementCurrentPlayerCount(string secret);
        Task<bool> UpdateCurrentPlayerCount(string secret, int currentPlayerCount);
        Task<bool> UpdateServerGameplayState(string secret, bool InGameplay);
        Task<long> TotalPlayerJoins();
        Task<long> TotalServersMade();
    }
}
