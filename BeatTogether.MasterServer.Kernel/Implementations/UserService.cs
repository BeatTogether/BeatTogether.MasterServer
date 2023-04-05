using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
            MasterServerConfiguration configuration)
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
        }

        public async Task<AuthenticateUserResponse> Authenticate(MasterServerSession session, AuthenticateUserRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(AuthenticateUserRequest)} " +
                $"(Platform={request.AuthenticationToken.Platform}, " +
                $"UserId='{request.AuthenticationToken.UserId}', " +
                $"UserName='{request.AuthenticationToken.UserName}')."
            );

            var platform = request.AuthenticationToken.Platform;

            if (_configuration.AuthenticateClients)
            {
                if (platform != Platform.OculusQuest && platform != Platform.Oculus && platform != Platform.Test) //TODO figure out why oculus does not authenticate correctly at some point
                {
                    try
                    {
                        var requestContent = new
                        {
                            proof = BitConverter.ToString(request.AuthenticationToken.SessionToken).Replace("-", ""),
                            oculusId = platform == Platform.Oculus ? request.AuthenticationToken.UserId : "",
                            steamId = platform == Platform.Steam ? request.AuthenticationToken.UserId : ""
                        };

                        using var verifyResponse = await _httpClient.PostAsync(VerifyUserURL, new StringContent(JsonSerializer.Serialize(requestContent), null, "application/json"));

                        if (verifyResponse.StatusCode == HttpStatusCode.OK)
                        {
                            var stringContent = await verifyResponse.Content.ReadAsStringAsync();
                            if (stringContent.Contains("\"success\": false"))
                            {
                                _logger.Information(
                                    "Session authentication failed " +
                                    $"(EndPoint='{session.EndPoint}', " +
                                    $"Platform='{request.AuthenticationToken.Platform}', " +
                                    $"UserId='{request.AuthenticationToken.UserId}', " +
                                    $"UserName='{request.AuthenticationToken.UserName}').");

                                return new AuthenticateUserResponse
                                {
                                    Result = AuthenticateUserResult.Failed
                                };
                            }
                            else
                            {
                                _logger.Information(
                                    "Session authenticated " +
                                    $"(EndPoint='{session.EndPoint}', " +
                                    $"Platform='{request.AuthenticationToken.Platform}', " +
                                    $"UserId='{request.AuthenticationToken.UserId}', " +
                                    $"UserName='{request.AuthenticationToken.UserName}')."
                                );
                            }
                        }
                        else
                        {
                            _logger.Information(
                                "Skipping session authentication (BeatSaverDown)" +
                                $"(EndPoint='{session.EndPoint}', " +
                                $"Platform='{request.AuthenticationToken.Platform}', " +
                                $"UserId='{request.AuthenticationToken.UserId}', " +
                                $"UserName='{request.AuthenticationToken.UserName}')."
                            );
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.Information(
                            $"Skipping session authentication ({ex.Message})" +
                            $"(EndPoint='{session.EndPoint}', " +
                            $"Platform='{request.AuthenticationToken.Platform}', " +
                            $"UserId='{request.AuthenticationToken.UserId}', " +
                            $"UserName='{request.AuthenticationToken.UserName}')."
                        );
                    }
                }
                else
                {
                    _logger.Information(
                        "Skipping session authentication (OculusQuest)" +
                        $"(EndPoint='{session.EndPoint}', " +
                        $"Platform='{request.AuthenticationToken.Platform}', " +
                        $"UserId='{request.AuthenticationToken.UserId}', " +
                        $"UserName='{request.AuthenticationToken.UserName}')."
                    );
                }
            }
            else
            {
                _logger.Information(
                    "Skipping session authentication (disabled)" +
                    $"(EndPoint='{session.EndPoint}', " +
                    $"Platform='{request.AuthenticationToken.Platform}', " +
                    $"UserId='{request.AuthenticationToken.UserId}', " +
                    $"UserName='{request.AuthenticationToken.UserName}')."
                );
            }
               
            session.Platform = request.AuthenticationToken.Platform;
            session.UserId = request.AuthenticationToken.UserId;
            session.UserName = request.AuthenticationToken.UserName;

            string platformStr = session.Platform switch
            {
                Platform.Test => "Test#",
                Platform.Oculus => "Oculus#",
                Platform.OculusQuest => "OculusQuest#",
                Platform.Steam => "Steam#",
                Platform.PS4 => "PSN#",
                _ => ""
            };

            session.GameId = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(platformStr + session.UserId)))[..22];

            return new AuthenticateUserResponse
            {
                Result = AuthenticateUserResult.Success
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
            Server serverFromRepo = await _serverRepository.GetServer(server.Secret);
            if (serverFromRepo.CurrentPlayerCount + 1 > serverFromRepo.GameplayServerConfiguration.MaxPlayerCount)
            {
                return new ConnectToServerResponse()
                {
                    Result = ConnectToServerResult.ServerAtCapacity
                };
            }
            if (!await _nodeRepository.SendAndAwaitPlayerEncryptionRecievedFromNode(server.RemoteEndPoint, session.EndPoint, session.UserId, session.UserName, session.Platform, Random, PublicKey, EncryptionRecieveTimeout))
            {
                _autobus.Publish(new DisconnectPlayerFromMatchmakingServerEvent(server.Secret, session.UserId, session.EndPoint.ToString()));
                return new ConnectToServerResponse()
                {
                    Result = ConnectToServerResult.UnknownError
                };
            }

            var sessioncheck = _sessionService.GetSession(session.EndPoint);
            int LastServerMilliSeconds = (int)DateTime.Now.Subtract(session.LastGameDisconnect).TotalMilliseconds;
            if (sessioncheck.LastGameIp == server.RemoteEndPoint.ToString() && LastServerMilliSeconds < 6000)
            {
                _logger.Verbose("Delaying player from joining");
                await Task.Delay(6000 - (LastServerMilliSeconds) );
            }

            _sessionService.AddSession(session.EndPoint, server.Secret);

            _logger.Information("Player: " + session.GameId + " Is being sent to node: " + server.RemoteEndPoint + ", Server name: " + serverFromRepo.Host.UserName + ", PlayerCountBeforeJoin: " + serverFromRepo.CurrentPlayerCount);
            /*
                        _logger.Information($"Random='{BitConverter.ToString(Random)}'");
                        _logger.Information($"PublicKey='{BitConverter.ToString(PublicKey)}'");
                        _logger.Information($"session.ClientRandom='{BitConverter.ToString(session.ClientRandom)}'");
                        _logger.Information($"session.ClientPublicKey='{BitConverter.ToString(session.ClientPublicKey)}'");
            */
            return new ConnectToServerResponse
            {
                UserId = "ziuMSceapEuNN7wRGQXrZg",
                UserName = server.Host.UserName,
                Secret = server.Secret,
                BeatmapLevelSelectionMask = new BeatmapLevelSelectionMask
                {
                    BeatmapDifficultyMask = (BeatmapDifficultyMask)server.BeatmapDifficultyMask,
                    GameplayModifiersMask = (GameplayModifiersMask)server.GameplayModifiersMask,
                    SongPackMask = new SongPackMask(server.SongPackBloomFilterTop, server.SongPackBloomFilterBottom)
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

        public Server CreateServer(ConnectToMatchmakingServerRequest request ,string ServerName, string ManagerName,string secret, IPEndPoint DediEndpoint, bool IsQuickplay, byte[] random, byte[] publicKey)
        {
            return new Server
            {
                Host = new Player
                {
                    UserId = "ziuMSceapEuNN7wRGQXrZg", //Server UserId is the host, always
                    UserName = ServerName
                },
                RemoteEndPoint = DediEndpoint,
                Secret = secret,
                Code = _serverCodeProvider.Generate(),
                IsPublic = IsQuickplay,
                BeatmapDifficultyMask = (Domain.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask,
                GameplayModifiersMask = (Domain.Enums.GameplayModifiersMask)request.BeatmapLevelSelectionMask.GameplayModifiersMask,
                GameplayServerConfiguration = new Domain.Models.GameplayServerConfiguration
                    (
                        Math.Min(request.GameplayServerConfiguration.MaxPlayerCount, 250), //New max player count
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
                IsInGameplay = false,
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
            string ManagerId = "ziuMSceapEuNN7wRGQXrZg";
            if (!isQuickplay)
                ManagerId = session.GameId;//sets the manager to the player who is requesting
            else
                secret = _secretProvider.GetSecret();

            if(server == null) //Creates the server, then the player can join
            {
                string ServerName = session.UserName + "'s server";
                if (isQuickplay)
                    ServerName = "BeatTogether Quickplay: " + ((Domain.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask).ToString();

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

                var remoteEndPoint = IPEndPoint.Parse(createMatchmakingServerResponse.RemoteEndPoint);
                server = CreateServer(request, ServerName, session.UserName, secret, remoteEndPoint, isQuickplay, createMatchmakingServerResponse.Random, createMatchmakingServerResponse.PublicKey);
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
                $"UserId='{session.GameId}', " +
                $"UserName='{session.UserName}')."
            );
            session.LastKeepAlive = DateTimeOffset.UtcNow;
            return Task.CompletedTask;
        }
    }
}
