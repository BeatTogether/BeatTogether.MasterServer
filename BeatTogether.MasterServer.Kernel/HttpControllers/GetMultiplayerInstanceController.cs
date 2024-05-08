using System;
using System.Net;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Implementations;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Messages.User;
using BeatTogether.MasterServer.Messaging.Models;
using BeatTogether.MasterServer.Messaging.Models.HttpApi;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.HttpControllers
{
    [ApiController]
    public class GetMultiplayerInstanceController : Controller
    {
        public const string SessionIdPrefix = "ps:bt$";
        
        private readonly IUserService _userService;
        private readonly IMasterServerSessionService _sessionService;
        private readonly IUserAuthenticator _userAuthenticator;
        
        private readonly ILogger _logger;

        public GetMultiplayerInstanceController(
            IMasterServerSessionService sessionService, 
            IUserService userService,
            IUserAuthenticator userAuthenticator)
        {
            _userService = userService;
            _sessionService = sessionService;
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

            var remoteEndPoint = new IPEndPoint(
                HttpContext.Connection.RemoteIpAddress,
                HttpContext.Connection.RemotePort
            );

            // Try to resume session from our assigned player session ID (transmitted as ticket ID)
            // If that doesn't work, get/create session by endpoint
            MasterServerSession session = null;

            if (!string.IsNullOrEmpty(request.TicketId) &&
                _sessionService.TryGetSessionByPlayerSessionId(request.TicketId, out session))
            {
                // Session successfully resumed by playerSessionId
            }
            else
            {
                session = _sessionService.GetOrAddSession(remoteEndPoint);
                session.ClientVersion = request.Version;
                session.Platform = request.Platform;
            }

            // Authenticate platform user and assign session ID if not yet authed
            if (string.IsNullOrEmpty(session.PlayerSessionId))
            {
                if (!string.IsNullOrEmpty(request.SingleUseAuthToken) && !string.IsNullOrEmpty(request.UserId) &&
                    !string.IsNullOrEmpty(request.AuthUserId))
                {
                    var authToken = GetAuthenticationTokenFromRequest(request);
                    var authResult = await _userAuthenticator.TryAuthenticateUserWithPlatform(session, authToken);

                    if (!authResult)
                    {
                        // Hard auth failure
                        response.ErrorCode = MultiplayerPlacementErrorCode.AuthenticationFailed;
                        return new JsonResult(response);
                    }

                    // Auth success; generate session token
                    session.PlayerSessionId = SessionIdPrefix + Guid.NewGuid().ToString("N");
                }
                else
                {
                    _logger.Information("Auth error: Session did not send platform auth token");
                    response.ErrorCode = MultiplayerPlacementErrorCode.AuthenticationFailed;
                    return new JsonResult(response);
                }
            }

            response.AddSessionContext(session.PlayerSessionId);

            // Process matchmaking / create server / join server request
            ConnectToServerResponse matchResult = null;

            var versionParsed = TryParseGameVersion(request.Version);
            Version newSongPackMask = new(1, 35, 0);
            try
            {
                _logger.Debug($"Processing matchmaking request for client version {request.Version}");
                if (versionParsed >= newSongPackMask)
                {
                    _logger.Debug($"Sending matchmaking response for client version {request.Version}");
                    matchResult = await _userService.ConnectToMatchmakingServer(session,
                    new ConnectToMatchmakingServerRequest()
                    {
                        UserId = session.UserIdHash,
                        UserName = session.UserName,
                        Random = null,
                        PublicKey = null,
                        BeatmapLevelSelectionMask = new BeatmapLevelSelectionMask(request.BeatmapLevelSelectionMask),
                        Secret = request.PrivateGameSecret ?? "",
                        Code = request.PrivateGameCode ?? "",
                        GameplayServerConfiguration = request.GameplayServerConfiguration,
                        ExtraServerConfiguration = request.ExtraServerConfiguration ?? null
                    });
                }
                else
                {
                    _logger.Debug($"Not supported client version {request.Version}");
                    response.ErrorCode = MultiplayerPlacementErrorCode.MatchmakingTimeout;
                    return new JsonResult(response);

                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Matchmaking error");
                response.ErrorCode = MultiplayerPlacementErrorCode.MatchmakingTimeout; // results in CFR-15
                return new JsonResult(response);
            }

            if (!matchResult.Success || matchResult.Result != ConnectToServerResult.Success)
            {
                response.ErrorCode = matchResult.Result switch
                {
                    (ConnectToServerResult.InvalidPassword or ConnectToServerResult.InvalidCode
                        or ConnectToServerResult.InvalidSecret) => MultiplayerPlacementErrorCode.ServerDoesNotExist,
                    ConnectToServerResult.ServerAtCapacity => MultiplayerPlacementErrorCode.ServerAtCapacity,
                    ConnectToServerResult.NoAvailableDedicatedServers => MultiplayerPlacementErrorCode
                        .MatchmakingTimeout,
                    _ => MultiplayerPlacementErrorCode.Unknown
                };
                response.PollIntervalMs = -1;
                _logger.Information($"Matchmaking was unsuccessfull (userId={session.UserIdHash}, gameVersion={request.Version}, client error result={response.ErrorCode}, Server error={matchResult.Result}");
                return new JsonResult(response);
            }

            // For v1.31+ use ENet endpoint; for all other versions use default/LiteNet endpoint
            //var versionENet = new Version(1, 31, 0);
            //var useENet = versionParsed >= versionENet;
            //var targetEndPoint = useENet ? matchResult.RemoteEndPointENet : matchResult.RemoteEndPoint;
            
            // Success result
            _logger.Information($"Graph API join success (userId={session.UserIdHash}, gameVersion={request.Version}, " +
                                $"platform={session.Platform}, playerSessionId={session.PlayerSessionId}, targetNode={matchResult.RemoteEndPoint}");

            response.ErrorCode = MultiplayerPlacementErrorCode.Success;
            response.PlayerSessionInfo.GameSessionId = matchResult.ManagerId;
            response.PlayerSessionInfo.DnsName = matchResult.RemoteEndPoint.Address.ToString();
            response.PlayerSessionInfo.Port = matchResult.RemoteEndPoint.Port;

            //_logger.Verbose($"Sending matchmaking response with NewSongPackMask for client {request.Version}");
            response.PlayerSessionInfo.BeatmapLevelSelectionMask = BeatmapLevelSelectionMaskSimple.WithNewSongPackMask(matchResult.BeatmapLevelSelectionMask);
/*            if (versionParsed >= newSongPackMask)
            {
                _logger.Verbose($"Sending matchmaking response with NewSongPackMask for client {request.Version}");
                response.PlayerSessionInfo.BeatmapLevelSelectionMask = BeatmapLevelSelectionMaskSimple.WithNewSongPackMask(matchResult.BeatmapLevelSelectionMask);
            }
            else
            {
                _logger.Verbose($"Sending matchmaking response with LegacySongPackMask for client {request.Version}");
                response.PlayerSessionInfo.BeatmapLevelSelectionMask = BeatmapLevelSelectionMaskSimple.WithLegacySongPackMask(matchResult.BeatmapLevelSelectionMask);
            }*/

            response.PlayerSessionInfo.GameplayServerConfiguration = matchResult.Configuration;
            response.PlayerSessionInfo.PrivateGameSecret = matchResult.Secret;
            response.PlayerSessionInfo.PrivateGameCode = matchResult.Code;
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
            var response = new GetMultiplayerInstanceResponse();
            response.ErrorCode = MultiplayerPlacementErrorCode.ConnectionCanceled;
            return new JsonResult(response);
        }

        #region Util
        
        private static AuthenticationToken GetAuthenticationTokenFromRequest(GetMultiplayerInstanceRequest request)
        {
            byte[] sessionToken;

            if (request.Platform == Platform.Steam)
                sessionToken = AuthenticationToken.SessionTokenFromHex(request.SingleUseAuthToken);
            else
                sessionToken = AuthenticationToken.SessionTokenFromUtf8(request.SingleUseAuthToken);

            return new AuthenticationToken()
            {
                Platform = request.Platform,
                UserId = request.AuthUserId,
               // UserName = string.Empty,// not provided to master through GameLift auth process
                SessionToken = sessionToken
            };
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