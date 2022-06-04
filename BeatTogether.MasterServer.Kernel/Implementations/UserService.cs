using System;
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

        public UserService(
            IAutobus autobus,
            IMapper mapper,
            MasterServerMessageDispatcher messageDispatcher,
            IMatchmakingService matchmakingService,
            IServerRepository serverRepository,
            IMasterServerSessionService sessionService,
            IServerCodeProvider serverCodeProvider,
            ISecretProvider secretProvider)
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

            Server server;
            if (!string.IsNullOrEmpty(request.Code))
            {
                server = await _serverRepository.GetServerByCode(request.Code);
                if (server is null)
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResult.InvalidCode
                    };
            }
            else if (!string.IsNullOrEmpty(request.Secret))
            {
                // Create a new matchmaking server

                // Creates a new dedicated instance
                var createMatchmakingServerResponse = await _matchmakingService.CreateMatchmakingServer(
                    new CreateMatchmakingServerRequest(
                        request.Secret,
                        session.GameId,
                        _mapper.Map<DedicatedServer.Interface.Models.GameplayServerConfiguration>(request.GameplayServerConfiguration)
                    )
                );
                if (!createMatchmakingServerResponse.Success)
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResult.NoAvailableDedicatedServers
                    };
                //Adds the server to the master server repository
                var remoteEndPoint = IPEndPoint.Parse(createMatchmakingServerResponse.RemoteEndPoint);
                server = new Server
                {
                    Host = new Player
                    {
                        UserId = "ziuMSceapEuNN7wRGQXrZg", //server id is the host, server id is alwase the host
                        UserName = session.UserName + "'s server"
                    },
                    RemoteEndPoint = remoteEndPoint,
                    Secret = request.Secret,
                    Code = _serverCodeProvider.Generate(),
                    IsPublic = false,
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
                    CurrentPlayerCount = 1,
                    Random = createMatchmakingServerResponse.Random,
                    PublicKey = createMatchmakingServerResponse.PublicKey
                };
                if (!await _serverRepository.AddServer(server))
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResult.InvalidSecret
                    };
            }
            else
            {
                //Joins quickplay

                //Finds a quickplay server, null if none found
                server = await _serverRepository.GetAvailablePublicServer(
                    (Domain.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                    (Domain.Enums.GameplayServerMode)request.GameplayServerConfiguration.GameplayServerMode,
                    (Domain.Enums.SongSelectionMode)request.GameplayServerConfiguration.SongSelectionMode,
                    (Domain.Enums.GameplayServerControlSettings)request.GameplayServerConfiguration.GameplayServerControlSettings,
                    (Domain.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask,
                    (Domain.Enums.GameplayModifiersMask)request.BeatmapLevelSelectionMask.GameplayModifiersMask,
                    request.BeatmapLevelSelectionMask.SongPackMask.Top,
                    request.BeatmapLevelSelectionMask.SongPackMask.Bottom
                );
                if (server is null)
                {
                    //Creates a quickplay server
                    var serverSecret = _secretProvider.GetSecret();

                    var createMatchmakingServerResponse = await _matchmakingService.CreateMatchmakingServer(
                        new CreateMatchmakingServerRequest(
                            serverSecret,
                            "ziuMSceapEuNN7wRGQXrZg", //manager id is the servers id
                            _mapper.Map<DedicatedServer.Interface.Models.GameplayServerConfiguration>(request.GameplayServerConfiguration)
                        )
                    );
                    if (!createMatchmakingServerResponse.Success)
                        return new ConnectToServerResponse
                        {
                            Result = ConnectToServerResult.NoAvailableDedicatedServers
                        };
                    var remoteEndPoint = IPEndPoint.Parse(createMatchmakingServerResponse.RemoteEndPoint);
                    var serverConfiguration = new Domain.Models.GameplayServerConfiguration
                    (
                        request.GameplayServerConfiguration.MaxPlayerCount,
                        Domain.Enums.DiscoveryPolicy.Public,
                        (Domain.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                        (Domain.Enums.GameplayServerMode)request.GameplayServerConfiguration.GameplayServerMode,
                        (Domain.Enums.SongSelectionMode)request.GameplayServerConfiguration.SongSelectionMode,
                        (Domain.Enums.GameplayServerControlSettings)request.GameplayServerConfiguration.GameplayServerControlSettings
                    );
                    string code = _serverCodeProvider.Generate();
                    server = new Server
                    {
                        
                        Host = new Player
                        {
                            UserId = "ziuMSceapEuNN7wRGQXrZg",
                            UserName = "QuickplayInstance: " + ((Domain.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask).ToString()//Server's name
                        },
                        RemoteEndPoint = remoteEndPoint,
                        Secret = serverSecret,
                        Code = code,
                        IsPublic = true,
                        DiscoveryPolicy = serverConfiguration.DiscoveryPolicy,
                        InvitePolicy = serverConfiguration.InvitePolicy,
                        BeatmapDifficultyMask = (Domain.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask,
                        GameplayModifiersMask = (Domain.Enums.GameplayModifiersMask)request.BeatmapLevelSelectionMask.GameplayModifiersMask,
                        GameplayServerConfiguration = serverConfiguration,
                        SongPackBloomFilterTop = request.BeatmapLevelSelectionMask.SongPackMask.Top,
                        SongPackBloomFilterBottom = request.BeatmapLevelSelectionMask.SongPackMask.Bottom,
                        CurrentPlayerCount = 1,
                        Random = createMatchmakingServerResponse.Random,
                        PublicKey = createMatchmakingServerResponse.PublicKey
                    };
                    if (!await _serverRepository.AddServer(server))
                        return new ConnectToServerResponse
                        {
                            Result = ConnectToServerResult.InvalidSecret
                        };
                }
            }

            _autobus.Publish(new PlayerConnectedToMatchmakingServerEvent(
                session.EndPoint.ToString(), session.UserId, session.UserName,
                request.Random, request.PublicKey
            ));
            await Task.Delay(EncryptionAddDelay);
            _logger.Information("Connected to matchmaking server!");
            _logger.Information($"Random='{BitConverter.ToString(request.Random)}'");
            _logger.Information($"PublicKey='{BitConverter.ToString(request.PublicKey)}'");
            _logger.Information($"session.ClientRandom='{BitConverter.ToString(session.ClientRandom)}'");
            _logger.Information($"session.ClientPublicKey='{BitConverter.ToString(session.ClientPublicKey)}'");
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
