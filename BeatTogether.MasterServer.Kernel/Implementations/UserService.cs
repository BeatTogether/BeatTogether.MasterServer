using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Autobus;
using AutoMapper;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.MasterServer.Interface.Events;
using BeatTogether.MasterServer.Kernal.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Messages.User;
using BeatTogether.MasterServer.Messaging.Models;
using Serilog;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class UserService : IUserService
    {
        public const string FixedServerUserId = "ziuMSceapEuNN7wRGQXrZg";
        public const string VerifyUserURL = "https://api.beatsaver.com/users/verify";
        public const int EncryptionRecieveTimeout = 2000;

        private readonly IAutobus _autobus;
        private readonly IMapper _mapper;
        private readonly MasterServerMessageDispatcher _messageDispatcher;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IServerRepository _serverRepository;
        private readonly IMasterServerSessionService _sessionService;
        private readonly IServerCodeProvider _serverCodeProvider;
        private readonly ISecretProvider _secretProvider;
        private readonly ILogger _logger;
        private readonly INodeRepository _nodeRepository;
        private readonly HttpClient _httpClient;
        private readonly MasterServerConfiguration _configuration;
        private readonly IUserAuthenticator _userAuthenticator;

        public UserService(
            IAutobus autobus,
            IMapper mapper,
            MasterServerMessageDispatcher messageDispatcher,
            IMatchmakingService matchmakingService,
            IServerRepository serverRepository,
            IMasterServerSessionService sessionService,
            IServerCodeProvider serverCodeProvider,
            ISecretProvider secretProvider,
            INodeRepository nodeRepository,
            HttpClient httpClient,
            MasterServerConfiguration configuration,
            IUserAuthenticator userAuthenticator)
        {
            _autobus = autobus;
            _mapper = mapper;
            _messageDispatcher = messageDispatcher;
            _matchmakingService = matchmakingService;
            _serverRepository = serverRepository;
            _sessionService = sessionService;
            _serverCodeProvider = serverCodeProvider;
            _secretProvider = secretProvider;
            _logger = Log.ForContext<UserService>();
            _nodeRepository = nodeRepository;
            _httpClient = httpClient;
            _configuration = configuration;
            _userAuthenticator = userAuthenticator;
        }

        public async Task<AuthenticateUserResponse> Authenticate(MasterServerSession session, AuthenticateUserRequest request)
        {
            var authSuccess =
                await _userAuthenticator.TryAuthenticateUserWithPlatform(session, request.AuthenticationToken);

            return new AuthenticateUserResponse
            {
                Result = authSuccess ? AuthenticateUserResult.Success : AuthenticateUserResult.Failed
            };
        }

        private async Task<Server> GetServerToConnectTo(ConnectToMatchmakingServerRequest request, bool IsQuickplay)
        {
            if (!IsQuickplay)
            {
                Server server = await _serverRepository.GetServerByCode(request.Code.Replace('8', 'B').Replace('D', '0')); //Similar characters are replaced
                if(server == null)
                    server = await _serverRepository.GetServer(request.Secret);
                return server;
            }
            return await _serverRepository.GetAvailablePublicServer(
                (Domain.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                (Domain.Enums.GameplayServerMode)request.GameplayServerConfiguration.GameplayServerMode,
                (Domain.Enums.SongSelectionMode)request.GameplayServerConfiguration.SongSelectionMode,
                (Domain.Enums.GameplayServerControlSettings)request.GameplayServerConfiguration.GameplayServerControlSettings,
                (Domain.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask,
                (Domain.Enums.GameplayModifiersMask)request.BeatmapLevelSelectionMask.GameplayModifiersMask,
                request.BeatmapLevelSelectionMask.SongPackMask.D0,
                request.BeatmapLevelSelectionMask.SongPackMask.D1,
                request.BeatmapLevelSelectionMask.SongPackMask.D2,
                request.BeatmapLevelSelectionMask.SongPackMask.D3);
        }

        private bool DoesServerExist(Server server)
        {
            return _nodeRepository.EndpointExists(server.LiteNetEndPoint);
        }

        private async Task<ConnectToServerResponse> ConnectPlayer(MasterServerSession session, Server server, byte[] Random, byte[] PublicKey)
        {
            if(server.CurrentPlayerCount + 1 > server.GameplayServerConfiguration.MaxPlayerCount)
            {
                return new ConnectToServerResponse()
                {
                    Result = ConnectToServerResult.ServerAtCapacity
                };
            }

            _logger.Information("Player: " + session.UserIdHash + " Is being sent to node: " + server.LiteNetEndPoint + ", Server name: " + server.ServerName + ", Player count before join: " + server.CurrentPlayerCount);

            if (!await _nodeRepository.SendAndAwaitPlayerEncryptionRecievedFromNode(server.LiteNetEndPoint,
                    session.EndPoint, session.UserIdHash, session.UserName, session.Platform, Random, PublicKey,
                     session.PlayerSessionId, session.ClientVersion, session.PlatformUserId, server.Secret, EncryptionRecieveTimeout))
            {
                _autobus.Publish(new DisconnectPlayerFromMatchmakingServerEvent(server.Secret, session.UserIdHash, session.EndPoint.ToString()));
                _logger.Warning("Player: " + session.UserIdHash + " Could not be sent to the node: " + server.LiteNetEndPoint);
                return new ConnectToServerResponse()
                {
                    Result = ConnectToServerResult.UnknownError
                };
            }

            return new ConnectToServerResponse
            {
                UserId = FixedServerUserId,
                UserName = server.ServerName,
                Secret = server.Secret,
                BeatmapLevelSelectionMask = new BeatmapLevelSelectionMask
                {
                    BeatmapDifficultyMask = (BeatmapDifficultyMask)server.BeatmapDifficultyMask,
                    GameplayModifiersMask = (GameplayModifiersMask)server.GameplayModifiersMask,
                    SongPackMask = new SongPackMask(server.SongPackBloomFilterD0, server.SongPackBloomFilterD1, server.SongPackBloomFilterD2, server.SongPackBloomFilterD3)
                },
                IsConnectionOwner = true,
                IsDedicatedServer = true,
                RemoteEndPoint = server.LiteNetEndPoint,
                RemoteEndPointENet = server.ENetEndPoint,
                Random = server.Random,
                PublicKey = server.PublicKey,
                Code = server.Code,
                Configuration = new Messaging.Models.GameplayServerConfiguration
                {
                    MaxPlayerCount = server.GameplayServerConfiguration.MaxPlayerCount,
                    DiscoveryPolicy = (DiscoveryPolicy)server.GameplayServerConfiguration.DiscoveryPolicy,
                    InvitePolicy = (InvitePolicy)server.GameplayServerConfiguration.InvitePolicy,
                    GameplayServerMode = (GameplayServerMode)server.GameplayServerConfiguration.GameplayServerMode,
                    SongSelectionMode = (SongSelectionMode)server.GameplayServerConfiguration.SongSelectionMode,
                    GameplayServerControlSettings = (GameplayServerControlSettings)server.GameplayServerConfiguration.GameplayServerControlSettings
                },
                ManagerId = server.ServerId
            };
        }

        public Server CreateServer(ConnectToMatchmakingServerRequest request ,string serverName, string managerName,
            string secret, IPEndPoint liteNetEndPoint, bool isQuickplay, byte[] random, byte[] publicKey,
            IPEndPoint eNetEndPoint)
        {
            return new Server
            {
                ServerId = FixedServerUserId,
                ServerName = serverName,
                LiteNetEndPoint = liteNetEndPoint,
                ENetEndPoint = eNetEndPoint,
                Secret = secret,
                Code = _serverCodeProvider.Generate(),
                IsPublic = isQuickplay,
                BeatmapDifficultyMask = (Domain.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask,
                GameplayModifiersMask = (Domain.Enums.GameplayModifiersMask)request.BeatmapLevelSelectionMask.GameplayModifiersMask,
                GameplayServerConfiguration = new Domain.Models.GameplayServerConfiguration
                    (
                        Math.Min(request.GameplayServerConfiguration.MaxPlayerCount, 100), //max player count //TODO more if patreon?
                        (Domain.Enums.DiscoveryPolicy)request.GameplayServerConfiguration.DiscoveryPolicy,
                        (Domain.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                        (Domain.Enums.GameplayServerMode)request.GameplayServerConfiguration.GameplayServerMode,
                        (Domain.Enums.SongSelectionMode)request.GameplayServerConfiguration.SongSelectionMode,
                        (Domain.Enums.GameplayServerControlSettings)request.GameplayServerConfiguration.GameplayServerControlSettings
                    ),
                SongPackBloomFilterD0 = request.BeatmapLevelSelectionMask.SongPackMask.D0,
                SongPackBloomFilterD1 = request.BeatmapLevelSelectionMask.SongPackMask.D1,
                SongPackBloomFilterD2 = request.BeatmapLevelSelectionMask.SongPackMask.D2,
                SongPackBloomFilterD3 = request.BeatmapLevelSelectionMask.SongPackMask.D3,
                PlayerHashes = new(),
                Random = random,
                PublicKey = publicKey,
                IsInGameplay = false,
                GameplayLevelId = string.Empty
            };
 
        }

        public async Task<ConnectToServerResponse> ConnectToMatchmakingServer(MasterServerSession session, ConnectToMatchmakingServerRequest request)
        {
            var randomLog = request.Random != null ? BitConverter.ToString(request.Random) : "Pending";
            var pubKeyLog = request.PublicKey != null ? BitConverter.ToString(request.PublicKey) : "Pending";
            _logger.Verbose(
                $"Handling {nameof(ConnectToMatchmakingServerRequest)} " +
                $"(UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"Random='{randomLog}', " +
                $"PublicKey='{pubKeyLog}', " +
                $"BeatmapDifficultyMask={request.BeatmapLevelSelectionMask.BeatmapDifficultyMask}, " +
                $"GameplayModifiersMask={request.BeatmapLevelSelectionMask.GameplayModifiersMask}, " +
                $"Secret='{request.Secret}', " +
                $"Code='{request.Code}')."
            );
            
            bool isQuickplay = string.IsNullOrEmpty(request.Code) && string.IsNullOrEmpty(request.Secret); //Quickplay is true if there is no code and no secret

            Server server = await GetServerToConnectTo(request, isQuickplay); //Gets the server that is requested to join
            if (server == null && !isQuickplay)
            {
                if (!string.IsNullOrEmpty(request.Code)) //if code was incorrect then server does not exist
                {
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResult.InvalidCode
                    };
                }
                if (string.IsNullOrEmpty(request.Secret))//If secret is empty then a server cannot be made/joined
                {
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResult.InvalidSecret
                    };
                } 

            }
            if(server != null)
            {
                if (!DoesServerExist(server))
                {
                    _logger.Information("NODE OFFLINE removing server with ID: " + server.Secret + " from the server list");
                    await _serverRepository.RemoveServer(server.Secret);
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResult.UnknownError //there is no specific error result for this
                    };
                }
            }
            string secret = request.Secret;
            string ManagerId = FixedServerUserId;
            if (!isQuickplay)
                ManagerId = session.UserIdHash;//sets the manager to the player who is requesting
            else
                secret = _secretProvider.GetSecret();

            if(server == null) //Creates the server, then the player can join
            {
                string ServerName = string.Empty;
                if (isQuickplay)
                    ServerName = "BeatTogether Quickplay: " + ((Domain.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask).ToString();
                else if(!string.IsNullOrEmpty(session.UserName))
                    ServerName = session.UserName + "'s server";
                var createMatchmakingServerResponse = await _matchmakingService.CreateMatchmakingServer(
                    new CreateMatchmakingServerRequest(
                        secret,
                        ManagerId,
                        _mapper.Map<DedicatedServer.Interface.Models.GameplayServerConfiguration>(request.GameplayServerConfiguration)
                     )
                    {
                    ServerName = ServerName,
                    });

                if (!createMatchmakingServerResponse.Success)
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResult.NoAvailableDedicatedServers
                    };

                var liteNetEndPoint = IPEndPoint.Parse(createMatchmakingServerResponse.RemoteEndPoint);
                var eNetEndPoint = IPEndPoint.Parse(createMatchmakingServerResponse.RemoteEndPointENet);
                
                server = CreateServer(request, ServerName, session.UserName, secret, liteNetEndPoint,
                    isQuickplay, createMatchmakingServerResponse.Random, createMatchmakingServerResponse.PublicKey,
                    eNetEndPoint);
                
                if (!await _serverRepository.AddServer(server))
                {
                    _autobus.Publish(new CloseServerInstanceEvent(secret));//Closes the server on the dedi side because master could not add it to the repository
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResult.UnknownError
                    };
                }

            }
            return await ConnectPlayer(session, server, request.Random, request.PublicKey);
        }

        public Task SessionKeepalive(MasterServerSession session, SessionKeepaliveMessage message)
        {
            _logger.Verbose(
                $"Handling {nameof(SessionKeepalive)} " +
                $"(EndPoint='{session.EndPoint}', " +
                $"Platform={session.Platform}, " +
                $"UserIdHash='{session.UserIdHash}', " +
                $"UserName='{session.UserName}')."
            );
            session.LastKeepAlive = DateTimeOffset.UtcNow;
            return Task.CompletedTask;
        }
    }
}
