using System.Net;
using System.Threading.Tasks;
using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.DedicatedServer.Interface.Responses;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Interface.ApiInterface.Abstractions;
using BeatTogether.MasterServer.Interface.ApiInterface.Models;
using BeatTogether.MasterServer.Interface.ApiInterface.Requests;
using BeatTogether.MasterServer.Interface.ApiInterface.Responses;
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.DedicatedServer.Interface.Enums;
using BeatTogether.MasterServer.Interface.ApiInterface.Enums;

//using BeatTogether.MasterServer.Messaging.Messages.User;

namespace BeatTogether.MasterServer.Kernal
{
    public class ApiInterface : IApiInterface
    {
        private readonly IAutobus _autobus;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IServerRepository _serverRepository;
        private readonly ISecretProvider _secretProvider;
        private readonly IServerCodeProvider _serverCodeProvider;

        public ApiInterface(
            IMatchmakingService matchmakingService, IAutobus autobus, IServerRepository serverRepository, ISecretProvider secretProvider, IServerCodeProvider serverCodeProvider)
        {
            _matchmakingService = matchmakingService;
            _autobus = autobus;
            _serverRepository = serverRepository;
            _secretProvider = secretProvider;
            _serverCodeProvider = serverCodeProvider;
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
                    request.GameplayServerConfiguration,
                    request.PermenantManager,
                    request.Timeout,
                    request.ServerName
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
                IsPublic = request.IsPublic,
                DiscoveryPolicy = (Domain.Enums.DiscoveryPolicy)request.GameplayServerConfiguration.DiscoveryPolicy,
                InvitePolicy = (Domain.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                BeatmapDifficultyMask = (Domain.Enums.BeatmapDifficultyMask)request.BeatmapDifficultyMask,
                GameplayModifiersMask = (Domain.Enums.GameplayModifiersMask)request.GameplayModifiersMask,
                GameplayServerConfiguration = (new GameplayServerConfiguration(
                    request.GameplayServerConfiguration.MaxPlayerCount,
                    (Domain.Enums.DiscoveryPolicy)request.GameplayServerConfiguration.DiscoveryPolicy,
                    (Domain.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                    (Domain.Enums.GameplayServerMode)request.GameplayServerConfiguration.GameplayServerMode,
                    (Domain.Enums.SongSelectionMode)request.GameplayServerConfiguration.SongSelectionMode,
                    (Domain.Enums.GameplayServerControlSettings)request.GameplayServerConfiguration.GameplayServerControlSettings
                    )),
                SongPackBloomFilterTop = request.SongPackMask.Top,
                SongPackBloomFilterBottom = request.SongPackMask.Bottom,
                /* 
                * For Built-In songs
                * 1441169510525575168 Top Level Selection mask
                * 9799832799948046336 Bottom Level Selection mask
                * For All Songs
                * 4366328120852363808 Top Level Selection mask
                * 14411518887094911232 Bottom Level Selection mask
                * For CUSTOM SONGS
                * 18446744073709551615 Top
                * 18446744073709551615 Bottom
                */
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

        public async Task<RemoveSecretServerResponse> RemoveServer(RemoveSecretServerRequest request)
        {
            var response = await _matchmakingService.StopMatchmakingServer(new StopMatchmakingServerRequest(request.Secret));
            return new RemoveSecretServerResponse(response.Success);
        }

        public async Task<RemoveCodeServerResponse> RemoveServer(RemoveCodeServerCodeRequest request)
        {
            Server server = await _serverRepository.GetServerByCode(request.Secret);
            var response = await _matchmakingService.StopMatchmakingServer(new StopMatchmakingServerRequest(server.Secret));
            return new RemoveCodeServerResponse(response.Success);
        }

        public async Task<PublicServerSecretListFromDedicatedResponse> GetPublicServerSecrets(GetPrivateServerSecretsListFromDedicatedRequest request)
        {
            PublicMatchmakingServerListResponse DediServers = await _matchmakingService.GetPublicMatchmakingServerList(new GetPublicMatchmakingServerListRequest());
            return new PublicServerSecretListFromDedicatedResponse(DediServers.PublicInstances);
        }

        public async Task<PublicServerSecretListResponse> GetPublicServerSecrets(GetPublicServerSecretsListRequest request)
        {
            return new PublicServerSecretListResponse(await _serverRepository.GetPublicServerSecretsList());
        }

        public async Task<ServerSecretListResponse> GetServerSecretsList(GetServerSecretsListRequest request)
        {
            return new ServerSecretListResponse(await _serverRepository.GetServerSecretsList());
        }

        public async Task<PublicServerListResponse> GetPublicServers(GetPublicSimpleServersRequest request)
        {
            Server[] servers = await _serverRepository.GetPublicServerList();
            SimpleServer[] simpleServers = new SimpleServer[servers.Length];
            for (int i = 0; i < servers.Length; i++)
            {
                simpleServers[i] = Simplify(servers[i]);
            }
            return new PublicServerListResponse(simpleServers);
        }

        public async Task<ServerListResponse> GetServers(GetSimpleServersRequest request)
        {
            Server[] servers = await _serverRepository.GetServerList();
            SimpleServer[] simpleServers = new SimpleServer[servers.Length];
            for (int i = 0; i < servers.Length; i++)
            {
                simpleServers[i] = Simplify(servers[i]);
            }
            return new ServerListResponse(simpleServers);
        }

        public SimpleServer Simplify(Server server)
        {
            SimpleServer simpleServer = new(
                server.Secret,
                server.Code,
                server.Host.UserName,
                server.Host.UserId,
                server.CurrentPlayerCount,
                Convert(server.GameplayServerConfiguration),
                (BeatmapDifficultyMask)server.BeatmapDifficultyMask,
                (GameplayModifiersMask)server.GameplayModifiersMask,
                server.RemoteEndPoint
                );
            return simpleServer;
        }

        private DedicatedServer.Interface.Models.GameplayServerConfiguration Convert(GameplayServerConfiguration Configuration)
        {
            DedicatedServer.Interface.Models.GameplayServerConfiguration gameplayServerConfiguration = new(
                Configuration.MaxPlayerCount,
                (DiscoveryPolicy)Configuration.DiscoveryPolicy,
                (InvitePolicy)Configuration.InvitePolicy,
                (GameplayServerMode)Configuration.GameplayServerMode,
                (SongSelectionMode)Configuration.SongSelectionMode,
                (GameplayServerControlSettings)Configuration.GameplayServerControlSettings
                );
            return gameplayServerConfiguration;
        }
    
        public async Task<Interface.ApiInterface.Responses.PublicServerCountResponse> GetPublicServerCount(GetPublicServerCountRequest request)
        {
            return new Interface.ApiInterface.Responses.PublicServerCountResponse(await _serverRepository.GetPublicServerCount());
        }

        public async Task<Interface.ApiInterface.Responses.ServerCountResponse> GetServerCount(GetServerCountRequest request)
        {
            return new Interface.ApiInterface.Responses.ServerCountResponse(await _serverRepository.GetServerCount());
        }
    }
                
}
