using System;
using System.Linq;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Models;
using BeatTogether.MasterServer.Messaging.Models.HttpApi;
using BeatTogether.Core.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using BeatTogether.MasterServer.Api.Implementations;
using BeatTogether.MasterServer.Api.Abstractions;
using BeatTogether.MasterServer.Api.Util;
using System.Text;
using BeatTogether.Core.Models;
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.MasterServer.Api.Abstractions.Providers;
using BeatTogether.MasterServer.Api.Configuration;

namespace BeatTogether.MasterServer.Api.HttpControllers
{
    [ApiController]
    public class GetMultiplayerInstanceController : Controller
    {
        public const string SessionIdPrefix = "ps:bt$";
        public const string FixedServerUserId = "ziuMSceapEuNN7wRGQXrZg";


        private readonly IMasterServerSessionService _sessionService;
        private readonly IUserAuthenticator _userAuthenticator;
        private readonly IServerCodeProvider _serverCodeProvider;
        private readonly ISecretProvider _secretProvider;
        private readonly ILogger _logger;

        private readonly ILayer2 _layer2;

        private readonly ApiServerConfiguration _apiServerConfiguration;

		public GetMultiplayerInstanceController(
            IMasterServerSessionService sessionService,
            ILayer2 layer2,
            IServerCodeProvider serverCodeProvider,
            ISecretProvider secretProvider,
            IUserAuthenticator userAuthenticator,
            ApiServerConfiguration configuration)
        {
            _layer2 = layer2;
            _sessionService = sessionService;
            _serverCodeProvider = serverCodeProvider;
            _secretProvider = secretProvider;
            _userAuthenticator = userAuthenticator;

            _apiServerConfiguration = configuration;

			_logger = Log.ForContext<GetMultiplayerInstanceController>();
        }

        /// <summary>
        /// Beat Saber sends this to request a server instance or begin matchmaking.
        /// </summary>
        [HttpPost]
        [Route("beat_saber_get_multiplayer_instance")]
        public async Task<IActionResult> GetMultiplayerInstance([FromBody] GetMultiplayerInstanceRequest request)
        {
            var response = new GetMultiplayerInstanceResponse();
            response.AddRequestContext(request);

            if (HttpContext.Connection.RemoteIpAddress is null)
            {
                _logger.Warning("Auth failure: Missing IP address from HTTP request context");
                response.ErrorCode = MultiplayerPlacementErrorCode.AuthenticationFailed;
                return new JsonResult(response);
            }

            // Try to resume session from our assigned player session ID (transmitted as ticket ID)
            // If that doesn't work, get/create session by endpoint
            MasterServerSession session = null;

            if (string.IsNullOrEmpty(request.TicketId) || !_sessionService.TryGetSession(request.TicketId, out session))
            {
                // Failed to get player session, creating new player session
                session = _sessionService.GetOrAddSession(SessionIdPrefix + Guid.NewGuid().ToString("N"));
                session.PlayerClientVersion = TryParseGameVersion(request.Version);
                session.PlayerPlatform = request.Platform;
                session.PlatformUserId = request.AuthUserId;
                session.HashedUserId = UserIdHash.Generate(session.PlayerPlatform, session.PlatformUserId);

                //Authenticate the session
                if (!string.IsNullOrEmpty(request.SingleUseAuthToken) && !string.IsNullOrEmpty(request.UserId) &&
                    !string.IsNullOrEmpty(request.AuthUserId))
                {
                    session.SessionToken = GetSessionTokenFromRequest(request);
                    var authResult = await _userAuthenticator.TryAuthenticateUserWithPlatform(session);

                    if (!authResult)
                    {
                        // Hard auth failure
                        response.ErrorCode = MultiplayerPlacementErrorCode.AuthenticationFailed;
                        return new JsonResult(response);
                    }
                }
                else
                {
                    _logger.Information("Auth error: Session did not send platform auth token");
                    response.ErrorCode = MultiplayerPlacementErrorCode.AuthenticationFailed;
                    return new JsonResult(response);
                }
            }
            
            //if(session == null) { _logger.Information("Player Session is null"); }
            response.AddSessionContext(session.PlayerSessionId);

            //Player authed and has a session, now get them a server.
            bool isQuickplay = string.IsNullOrEmpty(request.PrivateGameCode) && string.IsNullOrEmpty(request.PrivateGameSecret); //Quickplay is true if there is no code and no secret
            IServerInstance server = null;
            if (!isQuickplay)
            {
                if(!string.IsNullOrEmpty(request.PrivateGameCode))
                    server = await _layer2.GetServerByCode(GetFixedCode(request.PrivateGameCode));
                if (server == null && !string.IsNullOrEmpty(request.PrivateGameSecret))
                    server = await _layer2.GetServer(request.PrivateGameSecret);
            }
            else
            {
                server = await _layer2.GetAvailablePublicServer(
                    (Core.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                    (Core.Enums.GameplayServerMode)request.GameplayServerConfiguration.GameplayServerMode,
                    (Core.Enums.SongSelectionMode)request.GameplayServerConfiguration.SongSelectionMode,
                    (Core.Enums.GameplayServerControlSettings)request.GameplayServerConfiguration.GameplayServerControlSettings,
                    (Core.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask,
                    (Core.Enums.GameplayModifiersMask)request.BeatmapLevelSelectionMask.GameplayModifiersMask,
                    request.BeatmapLevelSelectionMask.SongPackMasks);
            }

            //If the server is still null, then make new server

            //Check if trying join a server via a code still
            if (server == null && !isQuickplay)
            {
                if (!string.IsNullOrEmpty(request.PrivateGameCode) || string.IsNullOrEmpty(request.PrivateGameSecret)) //if code was incorrect then server does not exist OR if secret is empty then a server cannot be made/joined
                {
                    response.ErrorCode = MultiplayerPlacementErrorCode.ServerDoesNotExist;
                    response.PollIntervalMs = -1;
                    return new JsonResult(response);
                }
            }

            if (server == null)
            {
                string secret = request.PrivateGameSecret;
                string managerId = FixedServerUserId;
                VersionRange supportedRange = VersionRange.FindVersionRange(_apiServerConfiguration.VersionRanges.ToList(), session.PlayerClientVersion!);
                if (supportedRange == null)
                {
					// Version not supported at all according to config
					_logger.Error($"Could not find matching version range for client version: {session.PlayerClientVersion}");
					response.ErrorCode = MultiplayerPlacementErrorCode.LobbyHostVersionMismatch;
                    return new JsonResult(response);
                }

                _logger.Debug(
	                $"Found version range MinVersion: '{supportedRange.MinVersion}' MaxVersion: '{supportedRange.MaxVersion}' for client version '{session.PlayerClientVersion}'");
                if (!isQuickplay)
                    managerId = session.HashedUserId; //sets the manager to the player who is requesting
                else
                    secret = _secretProvider.GetSecret();

                string ServerName = string.Empty;
                if (isQuickplay)
                    ServerName = "BeatTogether Quickplay: " + ((Core.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask);
                else if (request.ExtraServerConfiguration != null && request.ExtraServerConfiguration.ServerName != null)
                {
                    ServerName = request.ExtraServerConfiguration.ServerName;
                }
                server = new Server()
                {
                    ServerName = ServerName,
                    InstanceId = FixedServerUserId,
                    Secret = secret,
                    Code = _serverCodeProvider.Generate(),
                    ManagerId = managerId,
                    GameplayServerConfiguration = new(
                        request.GameplayServerConfiguration.MaxPlayerCount,
                        (Core.Enums.DiscoveryPolicy)request.GameplayServerConfiguration.DiscoveryPolicy,
                        (Core.Enums.InvitePolicy)request.GameplayServerConfiguration.InvitePolicy,
                        (Core.Enums.GameplayServerMode)request.GameplayServerConfiguration.GameplayServerMode,
                        (Core.Enums.SongSelectionMode)request.GameplayServerConfiguration.SongSelectionMode,
                        (Core.Enums.GameplayServerControlSettings)request.GameplayServerConfiguration.GameplayServerControlSettings),
                    BeatmapDifficultyMask = (Core.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask,
                    GameplayModifiersMask = (Core.Enums.GameplayModifiersMask)request.BeatmapLevelSelectionMask.GameplayModifiersMask,
                    SongPackMasks = request.BeatmapLevelSelectionMask.SongPackMasks,

                    AllowChroma = request.ExtraServerConfiguration != null ? request.ExtraServerConfiguration.AllowChroma ?? true : true,
                    AllowME = request.ExtraServerConfiguration != null ? request.ExtraServerConfiguration.AllowME ?? true : true,
                    AllowNE = request.ExtraServerConfiguration != null ? request.ExtraServerConfiguration.AllowNE ?? true : true,
                    AllowPerPlayerDifficulties = request.ExtraServerConfiguration != null ? request.ExtraServerConfiguration.AllowPerPlayerDifficulties ?? false : false,
                    AllowPerPlayerModifiers = request.ExtraServerConfiguration != null ? request.ExtraServerConfiguration.AllowPerPlayerModifiers ?? false : false,
                    BeatmapStartTime = request.ExtraServerConfiguration != null ? request.ExtraServerConfiguration.BeatmapStartTime ?? 5000L : 5000L,
                    PlayersReadyCountdownTime = request.ExtraServerConfiguration != null ? request.ExtraServerConfiguration.PlayersReadyCountdownTime ?? 0L : 0L,
                    ResultScreenTime = request.ExtraServerConfiguration != null ? request.ExtraServerConfiguration.ResultsScreenTime ?? 20000L : 20000L,
                    ServerStartJoinTimeout = 10000L,
                    //ServerStartJoinTimeout = request.ExtraServerConfiguration.Timeout ?? 10, //Dont allow everyone to do this as -1 means infinite server online time, server wont turn off when a player leaves
                    PermanentManager = request.ExtraServerConfiguration != null ? request.ExtraServerConfiguration.PermenantManger ?? true : true,
                    SupportedVersionRange = supportedRange
				};
                //Missing values from the server instance such as endpoint, will be added from within the CreateInstance function below.
                if (!await _layer2.CreateInstance(server))
                {
                    response.ErrorCode = MultiplayerPlacementErrorCode.MatchmakingTimeout;
                    response.PollIntervalMs = -1;
                    return new JsonResult(response);
                }
            }
            //Server now has a value.
            //Join the player.

            if (server.PlayerHashes.Count + 1 > server.GameplayServerConfiguration.MaxPlayerCount)
            {
                response.ErrorCode = MultiplayerPlacementErrorCode.ServerAtCapacity;
                response.PollIntervalMs = -1;
                return new JsonResult(response);
            }

            // Checks if the joining players version is witin the supported range of the lobby
            if (!VersionRange.VersionRangeSatisfies(server.SupportedVersionRange,
	                session.PlayerClientVersion.ToString()))
            {
                _logger.Warning($"Player '{session.HashedUserId}' on version '{session.PlayerClientVersion}' cannot join lobby with range {server.SupportedVersionRange.MinVersion} - {server.SupportedVersionRange.MaxVersion}");
                response.ErrorCode = MultiplayerPlacementErrorCode.LobbyHostVersionMismatch;
                return new JsonResult(response);
            }

            _logger.Information("Player session data from player: " + session.HashedUserId + " Is being sent to node: " + server.InstanceEndPoint + ", Server secret: " + server.Secret + ", Player count before join: " + server.PlayerHashes.Count);

            if (!await _layer2.SetPlayerSessionData(server.Secret, session))
            {
                _logger.Warning("Player: " + session.HashedUserId + " Could not be sent to the node: " + server.InstanceEndPoint);
                response.ErrorCode = MultiplayerPlacementErrorCode.Unknown;
                response.PollIntervalMs = -1;
                return new JsonResult(response);
            }

            //Server created and is ready to accept connection

            // Success result
            _logger.Information($"Graph API join success (userId={session.HashedUserId}, gameVersion={request.Version}, " +
                                $"platform={session.PlayerPlatform}, playerSessionId={session.PlayerSessionId}, targetNode={server.InstanceEndPoint}");

            response.ErrorCode = MultiplayerPlacementErrorCode.Success;
            response.PlayerSessionInfo.GameSessionId = FixedServerUserId;
            response.PlayerSessionInfo.DnsName = server.InstanceEndPoint.Address.ToString();
            response.PlayerSessionInfo.Port = server.InstanceEndPoint.Port;

            response.PlayerSessionInfo.BeatmapLevelSelectionMask.BeatmapDifficultyMask = (BeatmapDifficultyMask)server.BeatmapDifficultyMask;
            response.PlayerSessionInfo.BeatmapLevelSelectionMask.GameplayModifiersMask = (GameplayModifiersMask)server.GameplayModifiersMask;
            response.PlayerSessionInfo.BeatmapLevelSelectionMask.SongPackMasks = server.SongPackMasks;
            response.PlayerSessionInfo.GameplayServerConfiguration = new()
            {
                MaxPlayerCount = server.GameplayServerConfiguration.MaxPlayerCount,
                DiscoveryPolicy = (DiscoveryPolicy)server.GameplayServerConfiguration.DiscoveryPolicy,
                InvitePolicy = (InvitePolicy)server.GameplayServerConfiguration.DiscoveryPolicy,
                GameplayServerMode = (GameplayServerMode)server.GameplayServerConfiguration.GameplayServerMode,
                SongSelectionMode = (SongSelectionMode)server.GameplayServerConfiguration.SongSelectionMode,
                GameplayServerControlSettings = (GameplayServerControlSettings)server.GameplayServerConfiguration.GameplayServerControlSettings
            };
            response.PlayerSessionInfo.PrivateGameSecret = server.Secret;
            response.PlayerSessionInfo.PrivateGameCode = server.Code;
            response.TicketStatus = "COMPLETED";

            return new JsonResult(response);
        }

        /// <summary>
        /// Beat Saber sends this request when matchmaking gets cancelled by the user.
        /// </summary>
        [HttpPost]
        [Route("beat_saber_multiplayer_cancel_matchmaking_ticket")]
        public IActionResult CancelMatchmakingTicket()
        {
            var response = new GetMultiplayerInstanceResponse
            {
                ErrorCode = MultiplayerPlacementErrorCode.ConnectionCanceled
            };
            return new JsonResult(response);
        }

        #region Util

        public static string GetFixedCode(string code)
        {
			// Allowed chars ABCEFGHJKLMNPQRSTUVWXYZ01234579
			return code.Replace('8', 'B').Replace('D', '0').Replace('O', '0').Replace('I', '1').Replace('6', 'G');
        }

        private static byte[] GetSessionTokenFromRequest(GetMultiplayerInstanceRequest request)
        {
            byte[] sessionToken;

            if (request.Platform == Core.Enums.Platform.Steam)
                sessionToken = AuthenticationToken.SessionTokenFromHex(request.SingleUseAuthToken);
            else
                sessionToken = AuthenticationToken.SessionTokenFromUtf8(request.SingleUseAuthToken);

            return sessionToken;
        }

#nullable enable
        private static Version? TryParseGameVersion(string versionText)
        {
            var idxUnderscore = versionText.IndexOf('_');

            if (idxUnderscore >= 0)
                versionText = versionText[..idxUnderscore];

            return Version.TryParse(versionText, out var version) ? version : null;
        }
        
        #endregion
    }
#nullable restore
}