using System;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Autobus;
using AutoMapper;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Interface.Requests;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.MasterServer.Interface.ApiInterface;
using BeatTogether.MasterServer.Interface.Events;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Messages.User;
using BeatTogether.MasterServer.Messaging.Models;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class UserService : IUserService
    {
        public const int EncryptionAddDelay = 1500;

        private readonly IAutobus _autobus;
        private readonly IMapper _mapper;
        private readonly MasterServerMessageDispatcher _messageDispatcher;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IServerRepository _serverRepository;
        private readonly IMasterServerSessionService _sessionService;
        private readonly IServerCodeProvider _serverCodeProvider;
        private readonly ISecretProvider _secretProvider;
        private readonly ILogger _logger;
        private readonly IApiInterface _apiInterface;
        private readonly INodeRepository _nodeRepository;

        public UserService(
            IAutobus autobus,
            IMapper mapper,
            MasterServerMessageDispatcher messageDispatcher,
            IMatchmakingService matchmakingService,
            IServerRepository serverRepository,
            IMasterServerSessionService sessionService,
            IServerCodeProvider serverCodeProvider,
            ISecretProvider secretProvider,
            IApiInterface apiInterface,
            INodeRepository nodeRepository)
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
            _apiInterface = apiInterface;
            _nodeRepository = nodeRepository;
        }

        public Task<AuthenticateUserResponse> Authenticate(MasterServerSession session, AuthenticateUserRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(AuthenticateUserRequest)} " +
                $"(Platform={request.AuthenticationToken.Platform}, " +
                $"UserId='{request.AuthenticationToken.UserId}', " +
                $"UserName='{request.AuthenticationToken.UserName}')."
            );
            // TODO: Verify that there aren't any other sessions with the same user ID
            // TODO: Validate session token?
            _logger.Information(
                "Session authenticated " +
                $"(EndPoint='{session.EndPoint}', " +
                $"Platform={request.AuthenticationToken.Platform}, " +
                $"UserId='{request.AuthenticationToken.UserId}', " +
                $"UserName='{request.AuthenticationToken.UserName}')."
            );
            session.Platform = request.AuthenticationToken.Platform;
            session.UserId = request.AuthenticationToken.UserId;
            session.UserName = request.AuthenticationToken.UserName;

            string platformStr = session.Platform switch
            {
                Platform.Test => "Test#",
                Platform.Oculus => "Oculus#",
                Platform.OculusQuest => "Oculus#",
                Platform.Steam => "Steam#",
                Platform.PS4 => "PSN#",
                _ => ""
            };

            session.GameId = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(platformStr + session.UserId))).Substring(0, 22);

            return Task.FromResult(new AuthenticateUserResponse
            {
                Result = AuthenticateUserResult.Success
            });
        }


        private async Task<Server> GetServerToConnectTo(ConnectToMatchmakingServerRequest request, bool IsQuickplay)
        {
            if (!IsQuickplay)
            {
                Server server = await _serverRepository.GetServerByCode(request.Code);
                if(server == null)
                    server = await _serverRepository.GetServer(request.Secret);
                return server;
            }
            return  await _serverRepository.GetAvailablePublicServer(
                (Domain.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                (Domain.Enums.GameplayServerMode)request.GameplayServerConfiguration.GameplayServerMode,
                (Domain.Enums.SongSelectionMode)request.GameplayServerConfiguration.SongSelectionMode,
                (Domain.Enums.GameplayServerControlSettings)request.GameplayServerConfiguration.GameplayServerControlSettings,
                (Domain.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask,
                (Domain.Enums.GameplayModifiersMask)request.BeatmapLevelSelectionMask.GameplayModifiersMask,
                request.BeatmapLevelSelectionMask.SongPackMask.Top,
                request.BeatmapLevelSelectionMask.SongPackMask.Bottom);
        }

        private bool DoesServerExist(Server server)
        {
            return _nodeRepository.EndpointExists(server.RemoteEndPoint);
        }

        private async Task<ConnectToServerResponse> ConnectPlayer(MasterServerSession session, Server server, byte[] Random, byte[] PublicKey)
        {
            if (_sessionService.TryGetSession(session.EndPoint, out var QueSession))
            {
                if(QueSession.InQue)
                    return new ConnectToServerResponse()
                    {
                        Result = ConnectToServerResult.UnknownError
                    };
            }
            _sessionService.SetSessionJoining(session.EndPoint, true);
            server = await _serverRepository.GetServer(server.Secret);
            if (server.CurrentPlayerCount+1 > server.GameplayServerConfiguration.MaxPlayerCount)
                return new ConnectToServerResponse()
                {
                    Result = ConnectToServerResult.ServerAtCapacity
                };
            if(_sessionService.TryGetSession(session.EndPoint, out var ExistingSession))
            {
                if (ExistingSession.Secret != "" && ExistingSession.Secret != null)
                {
                    _autobus.Publish(new DisconnectPlayerFromMatchmakingServerEvent(ExistingSession.Secret, ExistingSession.UserId));
                }
            }
            await _serverRepository.IncrementCurrentPlayerCount(server.Secret);
            _sessionService.AddSession(session.EndPoint, server.Secret);
            _autobus.Publish(new PlayerConnectedToMatchmakingServerEvent(
                session.EndPoint.ToString(), session.UserId, session.UserName,
                Random, PublicKey
            ));
            //TODO temp queing systemm replace with checking all clients at some point 
            DateTime initial = DateTime.Now;
            bool connect = false;
            while (DateTime.Now.Subtract(server.LastPlayerJoinTime).Milliseconds < 8000 && DateTime.Now.Subtract(server.LastPlayerJoinTime).Milliseconds > 0 && DateTime.Now.Subtract(initial).Seconds > 0 && DateTime.Now.Subtract(initial).Seconds < 16)
            {
                await Task.Delay(DateTime.Now.Subtract(server.LastPlayerJoinTime).Milliseconds);
                connect = true;
            }
            if (DateTime.Now.Subtract(server.LastPlayerJoinTime).Seconds > 8)
            {
                connect = true;
            }
            if (!connect)
            {
                return new ConnectToServerResponse
                {
                    Result = ConnectToServerResult.UnknownError
                };
            }
            _serverRepository.SetLastPlayerTime(server.Secret);

            _logger.Information("Connected to matchmaking server!");
            _logger.Information($"Random='{BitConverter.ToString(Random)}'");
            _logger.Information($"PublicKey='{BitConverter.ToString(PublicKey)}'");
            _logger.Information($"session.ClientRandom='{BitConverter.ToString(session.ClientRandom)}'");
            _logger.Information($"session.ClientPublicKey='{BitConverter.ToString(session.ClientPublicKey)}'");
            _sessionService.SetSessionJoining(session.EndPoint, false);
            return new ConnectToServerResponse
            {
                UserId = "ziuMSceapEuNN7wRGQXrZg",
                UserName = server.Host.UserName,
                Secret = server.Secret,
                BeatmapLevelSelectionMask = new BeatmapLevelSelectionMask
                {
                    BeatmapDifficultyMask = (BeatmapDifficultyMask)server.BeatmapDifficultyMask,
                    GameplayModifiersMask = (GameplayModifiersMask)server.GameplayModifiersMask,
                    SongPackMask = new SongPackMask
                    {
                        Top = server.SongPackBloomFilterTop,
                        Bottom = server.SongPackBloomFilterBottom
                    }
                },
                IsConnectionOwner = true,
                IsDedicatedServer = true,
                RemoteEndPoint = server.RemoteEndPoint,
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
                ManagerId = server.Host.UserId
            };
        }

        public Server CreateServer(ConnectToMatchmakingServerRequest request ,string ManagerName,string secret, IPEndPoint DediEndpoint, bool IsQuickplay, byte[] random, byte[] publicKey)
        {
            string ServerName = ManagerName + "'s server";
            if (IsQuickplay)
                ServerName = "BeatTogether Quickplay: " + ((Domain.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask).ToString();
            return new Server
            {
                Host = new Player
                {
                    UserId = "ziuMSceapEuNN7wRGQXrZg", //server id is the host, server id is alwase the host
                    UserName = ServerName
                },
                RemoteEndPoint = DediEndpoint,
                Secret = secret,
                Code = _serverCodeProvider.Generate(),
                IsPublic = IsQuickplay,
                DiscoveryPolicy = (Domain.Enums.DiscoveryPolicy)request.GameplayServerConfiguration.DiscoveryPolicy,
                InvitePolicy = (Domain.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                BeatmapDifficultyMask = (Domain.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask,
                GameplayModifiersMask = (Domain.Enums.GameplayModifiersMask)request.BeatmapLevelSelectionMask.GameplayModifiersMask,
                GameplayServerConfiguration = new Domain.Models.GameplayServerConfiguration
                    (
                        request.GameplayServerConfiguration.MaxPlayerCount,
                        (Domain.Enums.DiscoveryPolicy)request.GameplayServerConfiguration.DiscoveryPolicy,
                        (Domain.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                        (Domain.Enums.GameplayServerMode)request.GameplayServerConfiguration.GameplayServerMode,
                        (Domain.Enums.SongSelectionMode)request.GameplayServerConfiguration.SongSelectionMode,
                        (Domain.Enums.GameplayServerControlSettings)request.GameplayServerConfiguration.GameplayServerControlSettings
                    ),
                SongPackBloomFilterTop = request.BeatmapLevelSelectionMask.SongPackMask.Top,
                SongPackBloomFilterBottom = request.BeatmapLevelSelectionMask.SongPackMask.Bottom,
                CurrentPlayerCount = 0,
                Random = random,
                PublicKey = publicKey,
                LastPlayerJoinTime = DateTime.Now.AddSeconds(-10)
            };
 
        }

        public async Task<ConnectToServerResponse> ConnectToMatchmakingServer(MasterServerSession session, ConnectToMatchmakingServerRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ConnectToMatchmakingServerRequest)} " +
                $"(UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"Random='{BitConverter.ToString(request.Random)}', " +
                $"PublicKey='{BitConverter.ToString(request.PublicKey)}', " +
                $"BeatmapDifficultyMask={request.BeatmapLevelSelectionMask.BeatmapDifficultyMask}, " +
                $"GameplayModifiersMask={request.BeatmapLevelSelectionMask.GameplayModifiersMask}, " +
                $"Secret='{request.Secret}', " +
                $"Code='{request.Code}')."
            );
            bool IsQuickplay = true;

            if (!string.IsNullOrEmpty(request.Code) || !string.IsNullOrEmpty(request.Secret))
            {
                IsQuickplay = false;
            }

            Server server = await GetServerToConnectTo(request, IsQuickplay);
            if (server == null && !IsQuickplay)
            {
                if (!string.IsNullOrEmpty(request.Code)) //if code was incorrect
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResult.InvalidCode
                    };
                if (string.IsNullOrEmpty(request.Secret)) //If secret is empty(cannot make server then)
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResult.InvalidSecret
                    };
            }
            if(server != null)
            {
                if(!DoesServerExist(server))
                {
                    await _serverRepository.RemoveServer(server.Secret);
                    server = null;
                }
            }
            string secret = request.Secret;
            string ManagerId = "ziuMSceapEuNN7wRGQXrZg";
            if (!IsQuickplay)
                ManagerId = session.GameId;
            else
                secret = _secretProvider.GetSecret();

            if(server == null) //Creates the server, then the player can join
            {
                var createMatchmakingServerResponse = await _matchmakingService.CreateMatchmakingServer(
                    new CreateMatchmakingServerRequest(
                        secret,
                        ManagerId,
                        _mapper.Map<DedicatedServer.Interface.Models.GameplayServerConfiguration>(request.GameplayServerConfiguration)
                     ));

                if (!createMatchmakingServerResponse.Success)
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResult.NoAvailableDedicatedServers
                    };

                var remoteEndPoint = IPEndPoint.Parse(createMatchmakingServerResponse.RemoteEndPoint);

                server = CreateServer(request, session.UserName, secret, remoteEndPoint, IsQuickplay, createMatchmakingServerResponse.Random, createMatchmakingServerResponse.PublicKey);

                if (!await _serverRepository.AddServer(server))
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResult.InvalidSecret
                    };
            }

            return await ConnectPlayer(session, server, request.Random, request.PublicKey);
        }

        public Task SessionKeepalive(MasterServerSession session, SessionKeepaliveMessage message)
        {
            _logger.Verbose(
                $"Handling {nameof(SessionKeepalive)} " +
                $"(EndPoint='{session.EndPoint}', " +
                $"Platform={session.Platform}, " +
                $"UserId='{session.UserId}', " +
                $"UserName='{session.UserName}')."
            );
            session.LastKeepAlive = DateTimeOffset.UtcNow;
            return Task.CompletedTask;
        }
    }
}
