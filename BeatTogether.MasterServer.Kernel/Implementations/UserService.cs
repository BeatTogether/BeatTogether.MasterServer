using System;
using System.Net;
using System.Threading.Tasks;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.DedicatedServer.Messaging.Requests;
using BeatTogether.DedicatedServer.Messaging.Responses;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Data.Entities;
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
        private readonly MasterServerMessageDispatcher _messageDispatcher;
        private readonly IRelayServerService _relayServerService;
        private readonly IServerRepository _serverRepository;
        private readonly IMasterServerSessionService _sessionService;
        private readonly IServerCodeProvider _serverCodeProvider;
        private readonly ILogger _logger;

        public UserService(
            MasterServerMessageDispatcher messageDispatcher,
            IRelayServerService relayServerService,
            IServerRepository serverRepository,
            IMasterServerSessionService sessionService,
            IServerCodeProvider serverCodeProvider)
        {
            _messageDispatcher = messageDispatcher;
            _relayServerService = relayServerService;
            _serverRepository = serverRepository;
            _sessionService = sessionService;
            _serverCodeProvider = serverCodeProvider;
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
            return Task.FromResult(new AuthenticateUserResponse
            {
                Result = AuthenticateUserResponse.ResultCode.Success
            });
        }

        public async Task<BroadcastServerStatusResponse> BroadcastServerStatus(MasterServerSession session, BroadcastServerStatusRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(BroadcastServerStatusRequest)} " +
                $"(ServerName='{request.ServerName}', " +
                $"UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"Secret='{request.Secret}', " +
                $"CurrentPlayerCount={request.CurrentPlayerCount}, " +
                $"MaximumPlayerCount={request.MaximumPlayerCount}, " +
                $"DiscoveryPolicy={request.DiscoveryPolicy}, " +
                $"InvitePolicy={request.InvitePolicy}, " +
                $"BeatmapDifficultyMask={request.Configuration.BeatmapDifficultyMask}, " +
                $"GameplayModifiersMask={request.Configuration.GameplayModifiersMask}, " +
                $"Random={BitConverter.ToString(request.Random)}, " +
                $"PublicKey={BitConverter.ToString(request.PublicKey)})."
            );

            var server = await _serverRepository.GetServer(request.Secret);
            if (server != null)
                return new BroadcastServerStatusResponse
                {
                    Result = BroadcastServerStatusResponse.ResultCode.SecretNotUnique
                };

            // TODO: We should probably retry in the event that a duplicate
            // code is ever generated (pretty unlikely to happen though)
            session.Secret = request.Secret;
            server = new Server()
            {
                Host = new Player()
                {
                    UserId = request.UserId,
                    UserName = request.UserName
                },
                RemoteEndPoint = (IPEndPoint)session.EndPoint,
                Secret = request.Secret,
                Code = _serverCodeProvider.Generate(),
                IsPublic = request.DiscoveryPolicy == DiscoveryPolicy.Public,
                DiscoveryPolicy = (Data.Enums.DiscoveryPolicy)request.DiscoveryPolicy,
                InvitePolicy = (Data.Enums.InvitePolicy)request.InvitePolicy,
                BeatmapDifficultyMask = (Data.Enums.BeatmapDifficultyMask)request.Configuration.BeatmapDifficultyMask,
                GameplayModifiersMask = (Data.Enums.GameplayModifiersMask)request.Configuration.GameplayModifiersMask,
                SongPackBloomFilterTop = request.Configuration.SongPackBloomFilterTop,
                SongPackBloomFilterBottom = request.Configuration.SongPackBloomFilterBottom,
                CurrentPlayerCount = request.CurrentPlayerCount,
                MaximumPlayerCount = request.MaximumPlayerCount,
                Random = request.Random,
                PublicKey = request.PublicKey
            };
            if (!await _serverRepository.AddServer(server))
            {
                _logger.Warning(
                    "Failed to create server " +
                    $"(ServerName='{request.ServerName}', " +
                    $"UserId='{request.UserId}', " +
                    $"UserName='{request.UserName}', " +
                    $"Secret='{request.Secret}', " +
                    $"CurrentPlayerCount={request.CurrentPlayerCount}, " +
                    $"MaximumPlayerCount={request.MaximumPlayerCount}, " +
                    $"DiscoveryPolicy={request.DiscoveryPolicy}, " +
                    $"InvitePolicy={request.InvitePolicy}, " +
                    $"BeatmapDifficultyMask={request.Configuration.BeatmapDifficultyMask}, " +
                    $"GameplayModifiersMask={request.Configuration.GameplayModifiersMask}, " +
                    $"Random={BitConverter.ToString(request.Random)}, " +
                    $"PublicKey={BitConverter.ToString(request.PublicKey)})."
                );
                return new BroadcastServerStatusResponse
                {
                    Result = BroadcastServerStatusResponse.ResultCode.UnknownError
                };
            }

            _logger.Information(
                "Successfully created server " +
                $"(ServerName='{request.ServerName}', " +
                $"UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"Secret='{request.Secret}', " +
                $"CurrentPlayerCount={request.CurrentPlayerCount}, " +
                $"MaximumPlayerCount={request.MaximumPlayerCount}, " +
                $"DiscoveryPolicy={request.DiscoveryPolicy}, " +
                $"InvitePolicy={request.InvitePolicy}, " +
                $"BeatmapDifficultyMask={request.Configuration.BeatmapDifficultyMask}, " +
                $"GameplayModifiersMask={request.Configuration.GameplayModifiersMask}, " +
                $"Random='{BitConverter.ToString(request.Random)}', " +
                $"PublicKey='{BitConverter.ToString(request.PublicKey)}')."
            );
            return new BroadcastServerStatusResponse
            {
                Result = BroadcastServerStatusResponse.ResultCode.Success,
                Code = server.Code,
                RemoteEndPoint = server.RemoteEndPoint
            };
        }

        public async Task BroadcastServerHeartbeat(MasterServerSession session, BroadcastServerHeartbeatRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(BroadcastServerHeartbeatRequest)} " +
                $"(UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"Secret='{request.Secret}', " +
                $"CurrentPlayerCount={request.CurrentPlayerCount})."
            );
            if (session.Secret != request.Secret)
            {
                _logger.Warning(
                    $"User sent {nameof(BroadcastServerHeartbeatRequest)} " +
                    "with an invalid secret " +
                    $"(UserId='{request.UserId}', " +
                    $"UserName='{request.UserName}', " +
                    $"Secret='{request.Secret}', " +
                    $"Expected='{session.Secret}')."
                );
                _messageDispatcher.Send(session, new BroadcastServerHeartbeatResponse
                {
                    Result = BroadcastServerHeartbeatResponse.ResultCode.UnknownError
                });
                return;
            }

            var server = await _serverRepository.GetServer(request.Secret);
            if (server == null)
            {
                _messageDispatcher.Send(session, new BroadcastServerHeartbeatResponse
                {
                    Result = BroadcastServerHeartbeatResponse.ResultCode.ServerDoesNotExist
                });
                return;
            }

            session.LastKeepAlive = DateTimeOffset.UtcNow;
            _serverRepository.UpdateCurrentPlayerCount(request.Secret, (int)request.CurrentPlayerCount);
            _messageDispatcher.Send(session, new BroadcastServerHeartbeatResponse
            {
                Result = BroadcastServerHeartbeatResponse.ResultCode.Success
            });
        }

        public async Task BroadcastServerRemove(MasterServerSession session, BroadcastServerRemoveRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(BroadcastServerRemoveRequest)} " +
                $"(UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"Secret='{request.Secret}')."
            );
            if (session.Secret != request.Secret)
            {
                _logger.Warning(
                    $"User sent {nameof(BroadcastServerRemoveRequest)} " +
                    "with an invalid secret " +
                    $"(UserId='{request.UserId}', " +
                    $"UserName='{request.UserName}', " +
                    $"Secret='{request.Secret}', " +
                    $"Expected='{session.Secret}')."
                );
                return;
            }

            var server = await _serverRepository.GetServer(request.Secret);
            if (server == null)
                return;

            if (!await _serverRepository.RemoveServer(server.Secret))
                return;

            _logger.Information(
                "Successfully removed server " +
                $"(UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"Secret='{server.Secret}')."
            );
        }

        public Task<ConnectToServerResponse> ConnectToMatchmaking(MasterServerSession session, ConnectToMatchmakingRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ConnectToMatchmakingRequest)} " +
                $"(UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"Random='{BitConverter.ToString(request.Random)}', " +
                $"PublicKey='{BitConverter.ToString(request.PublicKey)}', " +
                $"BeatmapDifficultyMask={request.Configuration.BeatmapDifficultyMask}, " +
                $"GameplayModifiersMask={request.Configuration.GameplayModifiersMask}, " +
                $"Secret='{request.Secret}')."
            );
            return Task.FromResult(new ConnectToServerResponse
            {
                Result = ConnectToServerResponse.ResultCode.UnknownError
            });
        }

        public async Task<ConnectToServerResponse> ConnectToServer(MasterServerSession session, ConnectToServerRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ConnectToServerRequest)} " +
                $"(UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"Random='{BitConverter.ToString(request.Random)}', " +
                $"PublicKey='{BitConverter.ToString(request.PublicKey)}', " +
                $"Secret='{request.Secret}', " +
                $"Code='{request.Code}', " +
                $"Password='{request.Password}', " +
                $"UseRelay={request.UseRelay})."
            );

            var server = await _serverRepository.GetServerByCode(request.Code);
            if (server is null)
                return new ConnectToServerResponse
                {
                    Result = ConnectToServerResponse.ResultCode.InvalidCode
                };

            if (server.CurrentPlayerCount >= server.MaximumPlayerCount)
                return new ConnectToServerResponse
                {
                    Result = ConnectToServerResponse.ResultCode.ServerAtCapacity
                };

            if (!_sessionService.TryGetSession(server.RemoteEndPoint, out var hostSession))
            {
                _logger.Warning(
                    "Failed to retrieve server host session while handling " +
                    $"{nameof(ConnectToServerRequest)} " +
                    $"(EndPoint='{server.RemoteEndPoint}')."
                );
                return new ConnectToServerResponse
                {
                    Result = ConnectToServerResponse.ResultCode.UnknownError
                };
            }

            var remoteEndPoint = (IPEndPoint)session.EndPoint;
            if (request.UseRelay)
            {
                GetAvailableRelayServerResponse getAvailableRelayServerResponse;
                try
                {
                    getAvailableRelayServerResponse = await _relayServerService.GetAvailableRelayServer(
                        new GetAvailableRelayServerRequest(
                            session.EndPoint.ToString()!,
                            hostSession.EndPoint.ToString()!
                        )
                    );
                }
                catch (TimeoutException e)
                {
                    _logger.Error(e, "Failed to get available relay server.");
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResponse.ResultCode.NoAvailableDedicatedServers
                    };
                }
                if (!getAvailableRelayServerResponse.Success)
                {
                    _logger.Warning("No available relay servers.");
                    return new ConnectToServerResponse
                    {
                        Result = ConnectToServerResponse.ResultCode.NoAvailableDedicatedServers
                    };
                }
                remoteEndPoint = IPEndPoint.Parse(getAvailableRelayServerResponse.RemoteEndPoint!);
            }

            // Let the host know that someone is about to connect (hole-punch)
            await _messageDispatcher.SendWithRetry(hostSession, new PrepareForConnectionRequest
            {
                UserId = request.UserId,
                UserName = request.UserName,
                RemoteEndPoint = remoteEndPoint,
                Random = request.Random,
                PublicKey = request.PublicKey,
                IsConnectionOwner = false,
                IsDedicatedServer = false
            });

            session.Secret = request.Secret;

            return new ConnectToServerResponse
            {
                Result = ConnectToServerResponse.ResultCode.Success,
                UserId = server.Host.UserId,
                UserName = server.Host.UserName,
                Secret = server.Secret,
                DiscoveryPolicy = (DiscoveryPolicy)server.DiscoveryPolicy,
                InvitePolicy = (InvitePolicy)server.InvitePolicy,
                MaximumPlayerCount = server.MaximumPlayerCount,
                Configuration = new GameplayServerConfiguration()
                {
                    BeatmapDifficultyMask = (BeatmapDifficultyMask)server.BeatmapDifficultyMask,
                    GameplayModifiersMask = (GameplayModifiersMask)server.GameplayModifiersMask,
                    SongPackBloomFilterTop = server.SongPackBloomFilterTop,
                    SongPackBloomFilterBottom = server.SongPackBloomFilterBottom
                },
                IsConnectionOwner = true,
                IsDedicatedServer = false,
                RemoteEndPoint = remoteEndPoint,
                Random = server.Random,
                PublicKey = server.PublicKey
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
