using System.Net;
using System.Threading.Tasks;
using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Interface.ApiInterface;
using BeatTogether.MasterServer.Interface.ApiInterface.Enums;
using BeatTogether.MasterServer.Interface.ApiInterface.Models;
using BeatTogether.MasterServer.Interface.ApiInterface.Requests;
using BeatTogether.MasterServer.Interface.ApiInterface.Responses;
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.MasterServer.Interface.Events;
using BeatTogether.MasterServer.Kernal.Abstractions;

namespace BeatTogether.MasterServer.Kernal
{
    public class ApiInterface : IApiInterface
    {
        private readonly IAutobus _autobus;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IServerRepository _serverRepository;
        private readonly ISecretProvider _secretProvider;
        private readonly IServerCodeProvider _serverCodeProvider;
        private readonly INodeRepository _nodeRepository;

        public ApiInterface(IMatchmakingService matchmakingService, IAutobus autobus, IServerRepository serverRepository, ISecretProvider secretProvider, IServerCodeProvider serverCodeProvider, INodeRepository nodeRepository)
        {
            _matchmakingService = matchmakingService;
            _autobus = autobus;
            _serverRepository = serverRepository;
            _secretProvider = secretProvider;
            _serverCodeProvider = serverCodeProvider;
            _nodeRepository = nodeRepository;
        }

        public async Task<CreatedServerResponse> CreateServer(CreateServerRequest request)
        {
            string Code = _serverCodeProvider.Generate();
            string Secret = _secretProvider.GetSecret();
            if (request.Code.Length == 5)
                Code = request.Code;
            if(request.Secret != "")
                Secret = request.Secret;
            var createMatchmakingServerResponse = await _matchmakingService.CreateMatchmakingServer(
                new CreateMatchmakingServerRequest(
                    Secret,
                    request.ManagerId,
                    new DedicatedServer.Interface.Models.GameplayServerConfiguration(
                        request.GameplayServerConfiguration.MaxPlayerCount,
                        (DedicatedServer.Interface.Enums.DiscoveryPolicy)request.GameplayServerConfiguration.DiscoveryPolicy,
                        (DedicatedServer.Interface.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                        (DedicatedServer.Interface.Enums.GameplayServerMode)request.GameplayServerConfiguration.GameplayServerMode,
                        (DedicatedServer.Interface.Enums.SongSelectionMode)request.GameplayServerConfiguration.SongSelectionMode,
                        (DedicatedServer.Interface.Enums.GameplayServerControlSettings)request.GameplayServerConfiguration.GameplayServerControlSettings
                        ),
                    request.PermanentManager,
                    request.Timeout,
                    request.ServerName,
                    request.resultScreenTime,
                    request.BeatmapStartTime,
                    request.PlayersReadyCountdownTime,
                    request.AllowPerPlayerModifiers,
                    request.AllowPerPlayerDifficulties,
                    request.AllowPerPlayerBeatmaps,
                    request.AllowChroma,
                    request.AllowME,
                    request.AllowNE
                    )
                );
            if (!createMatchmakingServerResponse.Success)
            {
                return new CreatedServerResponse(false);
            }
            //Adds the server to the master server repository
            var remoteEndPoint = IPEndPoint.Parse(createMatchmakingServerResponse.RemoteEndPoint);
            Server server = new()
            {
                Host = new Player
                {
                    UserId = "ziuMSceapEuNN7wRGQXrZg",
                    UserName = request.ServerName
                },
                RemoteEndPoint = remoteEndPoint,
                Secret = Secret,
                Code = Code,
                IsPublic = request.GameplayServerConfiguration.DiscoveryPolicy == DiscoveryPolicy.Public,
                DiscoveryPolicy = (Domain.Enums.DiscoveryPolicy)request.GameplayServerConfiguration.DiscoveryPolicy,
                InvitePolicy = (Domain.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                BeatmapDifficultyMask = (Domain.Enums.BeatmapDifficultyMask)request.BeatmapDifficultyMask,
                GameplayModifiersMask = (Domain.Enums.GameplayModifiersMask)request.GameplayModifiersMask,
                GameplayServerConfiguration = new Domain.Models.GameplayServerConfiguration(
                    request.GameplayServerConfiguration.MaxPlayerCount,
                    (Domain.Enums.DiscoveryPolicy)request.GameplayServerConfiguration.DiscoveryPolicy,
                    (Domain.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                    (Domain.Enums.GameplayServerMode)request.GameplayServerConfiguration.GameplayServerMode,
                    (Domain.Enums.SongSelectionMode)request.GameplayServerConfiguration.SongSelectionMode,
                    (Domain.Enums.GameplayServerControlSettings)request.GameplayServerConfiguration.GameplayServerControlSettings
                    ),
                SongPackBloomFilterTop = request.SongPackMask.Top,
                SongPackBloomFilterBottom = request.SongPackMask.Bottom,
                CurrentPlayerCount = 0,
                Random = createMatchmakingServerResponse.Random,
                PublicKey = createMatchmakingServerResponse.PublicKey
            };
            bool created = await _serverRepository.AddServer(server);
            if (!created)
            {
                return new CreatedServerResponse(false);
            }
            return new CreatedServerResponse(true, server.Secret, server.Code);
        }

        public Task<DisconnectPlayerResponse> KickPlayer(DisconnectPlayerRequest request)
        {
            _autobus.Publish(new DisconnectPlayerFromMatchmakingServerEvent(request.Secret, request.UserId));
            return Task.FromResult(new DisconnectPlayerResponse());
        }

        public async Task<RemoveServerResponse> StopServer(RemoveServerRequest request)
        {
            Server ToRemove;
            if (request.IsCode)
                ToRemove = await _serverRepository.GetServerByCode(request.SecretOrCode);
            else
                ToRemove = await _serverRepository.GetServer(request.SecretOrCode);
            if (ToRemove == null)
                return new RemoveServerResponse(false);
            _autobus.Publish(new CloseServerInstanceEvent(ToRemove.Secret));
            return new RemoveServerResponse(true);
        }

        public async Task<ServerListResponse> GetServers(GetServersRequest request)
        {
            Server[] servers = await _serverRepository.GetServerList();
            SimpleServer[] simpleServers = new SimpleServer[servers.Length];
            for (int i = 0; i < servers.Length; i++)
            {
                simpleServers[i] = Simplify(servers[i]);
            }
            return new ServerListResponse(simpleServers);
        }

        public Task<GetServerNodesResponse> GetNodes(GetServerNodesRequest request)
        {
            ServerNode[] serverNodes = new ServerNode[_nodeRepository.GetNodes().Count];
            int i = 0;
            foreach (var node in _nodeRepository.GetNodes().Values)
            {
                serverNodes[i] = node.Convert();
                i++;
            }
            return Task.FromResult(new GetServerNodesResponse(serverNodes));
        }

        public async Task<ServerJoinsCountResponse> GetPlayerJoins(GetPlayerJoins request)
        {
            return new ServerJoinsCountResponse(await _serverRepository.TotalPlayerJoins());
        }

        public SimpleServer Simplify(Server server)
        {
            if(server == null)
                return null;
            SimpleServer simpleServer = new(
                server.Secret,
                server.Code,
                server.Host.UserName,
                server.Host.UserId,
                server.CurrentPlayerCount,
                Convert(server.GameplayServerConfiguration),
                (BeatmapDifficultyMask)server.BeatmapDifficultyMask,
                (GameplayModifiersMask)server.GameplayModifiersMask,
                server.RemoteEndPoint.ToString()
                );
            return simpleServer;
        }

        private Interface.ApiInterface.Models.GameplayServerConfiguration Convert(Domain.Models.GameplayServerConfiguration Configuration)
        {
            Interface.ApiInterface.Models.GameplayServerConfiguration gameplayServerConfiguration = new(
                Configuration.MaxPlayerCount,
                (DiscoveryPolicy)Configuration.DiscoveryPolicy,
                (InvitePolicy)Configuration.InvitePolicy,
                (GameplayServerMode)Configuration.GameplayServerMode,
                (SongSelectionMode)Configuration.SongSelectionMode,
                (GameplayServerControlSettings)Configuration.GameplayServerControlSettings
                );
            return gameplayServerConfiguration;
        }


    }

}
