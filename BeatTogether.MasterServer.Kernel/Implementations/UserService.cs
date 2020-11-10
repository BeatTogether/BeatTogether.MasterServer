using System;
using System.Net;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Data.Entities;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Models;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.User;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class UserService : IUserService
    {
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly ISessionRepository _sessionRepository;
        private readonly IServerRepository _serverRepository;
        private readonly ISessionService _sessionService;
        private readonly IServerCodeProvider _serverCodeProvider;
        private readonly ILogger _logger;

        public UserService(
            IMessageDispatcher messageDispatcher,
            ISessionRepository sessionRepository,
            IServerRepository serverRepository,
            ISessionService sessionService,
            IServerCodeProvider serverCodeProvider)
        {
            _messageDispatcher = messageDispatcher;
            _sessionRepository = sessionRepository;
            _serverRepository = serverRepository;
            _sessionService = sessionService;
            _serverCodeProvider = serverCodeProvider;
            _logger = Log.ForContext<UserService>();
        }

        public Task<AuthenticateUserResponse> AuthenticateUser(ISession session, AuthenticateUserRequest request)
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
                $"(Platform={request.AuthenticationToken.Platform}, " +
                $"UserId='{request.AuthenticationToken.UserId}', " +
                $"UserName='{request.AuthenticationToken.UserName}')."
            );
            session.Platform = request.AuthenticationToken.Platform;
            session.UserId = request.AuthenticationToken.UserId;
            session.UserName = request.AuthenticationToken.UserName;
            return Task.FromResult(new AuthenticateUserResponse()
            {
                Result = AuthenticateUserResponse.ResultCode.Success
            });
        }

        public async Task<BroadcastServerStatusResponse> BroadcastServerStatus(ISession session, BroadcastServerStatusRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(BroadcastServerStatusRequest)} " +
                $"(ServerName='{request.ServerName}', " +
                $"UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"CurrentPlayerCount={request.CurrentPlayerCount}, " +
                $"MaximumPlayerCount={request.MaximumPlayerCount}, " +
                $"DiscoveryPolicy={request.DiscoveryPolicy}, " +
                $"InvitePolicy={request.InvitePolicy}, " +
                $"BeatmapDifficultyMask={request.Configuration.BeatmapDifficultyMask}, " +
                $"GameplayModifiersMask={request.Configuration.GameplayModifiersMask}, " +
                $"Random={BitConverter.ToString(request.Random)}, " +
                $"PublicKey={BitConverter.ToString(request.PublicKey)})."
            );
            var server = await _serverRepository.GetServerByHostUserId(session.UserId);
            if (server != null)
                return new BroadcastServerStatusResponse()
                {
                    Result = BroadcastServerStatusResponse.ResultCode.UnknownError
                };

            server = await _serverRepository.GetServer(request.Secret);
            if (server != null)
                return new BroadcastServerStatusResponse()
                {
                    Result = BroadcastServerStatusResponse.ResultCode.UnknownError
                };

            // TODO: We should probably retry in the event that a duplicate
            // code is ever generated (pretty unlikely to happen though)
            server = new Server()
            {
                Host = new Player()
                {
                    UserId = session.UserId,
                    UserName = session.UserName
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
                _logger.Information(
                    "Failed to create server " +
                    $"(ServerName='{request.ServerName}', " +
                    $"UserId='{request.UserId}', " +
                    $"UserName='{request.UserName}', " +
                    $"CurrentPlayerCount={request.CurrentPlayerCount}, " +
                    $"MaximumPlayerCount={request.MaximumPlayerCount}, " +
                    $"DiscoveryPolicy={request.DiscoveryPolicy}, " +
                    $"InvitePolicy={request.InvitePolicy}, " +
                    $"BeatmapDifficultyMask={request.Configuration.BeatmapDifficultyMask}, " +
                    $"GameplayModifiersMask={request.Configuration.GameplayModifiersMask}, " +
                    $"Random={BitConverter.ToString(request.Random)}, " +
                    $"PublicKey={BitConverter.ToString(request.PublicKey)})."
                );
                return new BroadcastServerStatusResponse()
                {
                    Result = BroadcastServerStatusResponse.ResultCode.UnknownError
                };
            }

            _logger.Information(
                "Successfully created server " +
                $"(ServerName='{request.ServerName}', " +
                $"UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"CurrentPlayerCount={request.CurrentPlayerCount}, " +
                $"MaximumPlayerCount={request.MaximumPlayerCount}, " +
                $"DiscoveryPolicy={request.DiscoveryPolicy}, " +
                $"InvitePolicy={request.InvitePolicy}, " +
                $"BeatmapDifficultyMask={request.Configuration.BeatmapDifficultyMask}, " +
                $"GameplayModifiersMask={request.Configuration.GameplayModifiersMask}, " +
                $"Random={BitConverter.ToString(request.Random)}, " +
                $"PublicKey={BitConverter.ToString(request.PublicKey)})."
            );
            return new BroadcastServerStatusResponse()
            {
                Result = BroadcastServerStatusResponse.ResultCode.Success,
                Code = server.Code,
                RemoteEndPoint = server.RemoteEndPoint
            };
        }

        public async Task<BroadcastServerHeartbeatResponse> BroadcastServerHeartbeat(ISession session, BroadcastServerHeartbeatRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(BroadcastServerHeartbeatRequest)} " +
                $"(UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"Secret='{request.Secret}', " +
                $"CurrentPlayerCount={request.CurrentPlayerCount})."
            );
            var server = await _serverRepository.GetServer(request.Secret);
            if (server == null)
                return new BroadcastServerHeartbeatResponse()
                {
                    Result = BroadcastServerHeartbeatResponse.ResultCode.ServerDoesNotExist
                };

            if (server.Host.UserId != session.UserId)
                return new BroadcastServerHeartbeatResponse()
                {
                    Result = BroadcastServerHeartbeatResponse.ResultCode.UnknownError
                };

            _serverRepository.UpdateCurrentPlayerCount(request.Secret, (int)request.CurrentPlayerCount);
            return new BroadcastServerHeartbeatResponse()
            {
                Result = BroadcastServerHeartbeatResponse.ResultCode.Success
            };
        }

        public async Task BroadcastServerRemove(ISession session, BroadcastServerRemoveRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(BroadcastServerRemoveRequest)} " +
                $"(UserId='{request.UserId}', " +
                $"UserName='{request.UserName}', " +
                $"Secret='{request.Secret}')."
            );
            var server = await _serverRepository.GetServer(request.Secret);
            if (server == null)
                return;

            if (server.Host.UserId != session.UserId)
                return;

            await _serverRepository.RemoveServer(server.Secret);
        }

        public Task<ConnectToServerResponse> ConnectToMatchmaking(ISession session, ConnectToMatchmakingRequest request)
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
            return Task.FromResult(new ConnectToServerResponse()
            {
                Result = ConnectToServerResponse.ResultCode.UnknownError
            });
        }

        public async Task<ConnectToServerResponse> ConnectToServer(ISession session, ConnectToServerRequest request)
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
            if (server == null)
                return new ConnectToServerResponse()
                {
                    Result = ConnectToServerResponse.ResultCode.InvalidCode
                };

            if (server.CurrentPlayerCount >= server.MaximumPlayerCount)
                return new ConnectToServerResponse()
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
                return new ConnectToServerResponse()
                {
                    Result = ConnectToServerResponse.ResultCode.UnknownError
                };
            }

            // Let the host know that someone is about to connect (hole-punch)
            _messageDispatcher.Send(hostSession, new PrepareForConnectionRequest()
            {
                UserId = request.UserId,
                UserName = request.UserName,
                RemoteEndPoint = (IPEndPoint)session.EndPoint,
                Random = request.Random,
                PublicKey = request.PublicKey,
                IsConnectionOwner = false,
                IsDedicatedServer = false
            });

            if (!await _serverRepository.IncrementCurrentPlayerCount(server.Secret))
            {
                _logger.Warning(
                    "Failed to increment player count " +
                    $"(Secret='{server.Secret}')."
                );
                return new ConnectToServerResponse()
                {
                    Result = ConnectToServerResponse.ResultCode.UnknownError
                };
            }

            return new ConnectToServerResponse()
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
                RemoteEndPoint = server.RemoteEndPoint,
                Random = server.Random,
                PublicKey = server.PublicKey
            };
        }

        public Task SessionKeepalive(ISession session, SessionKeepaliveMessage message)
        {
            _logger.Verbose(
                $"Handling {nameof(SessionKeepalive)} " +
                $"(EndPoint='{session.EndPoint}', " +
                $"Platform={session.Platform}, " +
                $"UserId='{session.UserId}', " +
                $"UserName='{session.UserName}')."
            );
            _sessionRepository.UpdateLastKeepAlive(session.UserId, DateTimeOffset.UtcNow);
            return Task.CompletedTask;
        }
    }
}
