using System;
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
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.MasterServer.Api.Abstractions.Providers;

namespace BeatTogether.MasterServer.Kernel.HttpControllers
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

        public GetMultiplayerInstanceController(
            IMasterServerSessionService sessionService,
            ILayer2 layer2,
            IServerCodeProvider serverCodeProvider,
            ISecretProvider secretProvider,
            IUserAuthenticator userAuthenticator)
        {
            _layer2 = layer2;
            _sessionService = sessionService;
            _serverCodeProvider = serverCodeProvider;
            _secretProvider = secretProvider;
            _userAuthenticator = userAuthenticator;
            
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

            // TODO Validate game client version supported range?

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
                session.PlayerPlatform = (Core.Enums.Platform)request.Platform;
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
                    server = (Server)await _layer2.GetServerByCode(request.PrivateGameCode.Replace('8', 'B').Replace('D', '0'));
                if (server == null && !string.IsNullOrEmpty(request.PrivateGameSecret))
                    server = (Server)await _layer2.GetServer(request.PrivateGameSecret);
            }
            else
            {
                server = (Server)await _layer2.GetAvailablePublicServer(
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
                if (!isQuickplay)
                    managerId = FixedServerUserId;// session.HashedUserId;//sets the manager to the player who is requesting
                else
                    secret = _secretProvider.GetSecret();

                string ServerName = string.Empty;
                if (isQuickplay)
                    ServerName = "BeatTogether Quickplay: " + ((Core.Enums.BeatmapDifficultyMask)request.BeatmapLevelSelectionMask.BeatmapDifficultyMask).ToString();
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
        
        private static byte[] GetSessionTokenFromRequest(GetMultiplayerInstanceRequest request)
        {
            byte[] sessionToken;

            if (request.Platform == Messaging.Enums.Platform.Steam)
                sessionToken = AuthenticationToken.SessionTokenFromHex(request.SingleUseAuthToken);
            else
                sessionToken = AuthenticationToken.SessionTokenFromUtf8(request.SingleUseAuthToken);

            return sessionToken;
        }

        public static byte[] SessionTokenFromHex(string str)
        {
            var array = new byte[str.Length / 2];
            var i = 0;
            var num = 0;
            var num2 = 1;

            while (i < array.Length)
            {
                array[i] = (byte)(GetHexVal(str[num]) << 4 | GetHexVal(str[num2]));
                i++;
                num += 2;
                num2 += 2;
            }

            return array;
        }

        public static byte GetHexVal(char c)
        {
            if (c >= '0' && c <= '9')
                return (byte)(c - '0');

            if (c >= 'a' && c <= 'f')
                return (byte)('\n' + c - 'a');

            if (c >= 'A' && c <= 'F')
                return (byte)('\n' + c - 'A');

            throw new Exception($"Invalid Hex Char {c}");
        }

        public static byte[] SessionTokenFromUtf8(string str)
        {
            return Encoding.UTF8.GetBytes(str);
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