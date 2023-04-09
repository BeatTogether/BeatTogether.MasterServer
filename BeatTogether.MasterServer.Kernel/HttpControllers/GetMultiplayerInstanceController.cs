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

            try
            {
                matchResult = await _userService.ConnectToMatchmakingServer(session,
                    new ConnectToMatchmakingServerRequest()
                    {
                        UserId = session.UserIdHash,
                        UserName = session.UserName,
                        Random = null,
                        PublicKey = null,
                        BeatmapLevelSelectionMask = request.BeatmapLevelSelectionMask,
                        Secret = request.PrivateGameSecret ?? "",
                        Code = request.PrivateGameCode ?? "",
                        GameplayServerConfiguration = request.GameplayServerConfiguration
                    });
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
                return new JsonResult(response);
            }

            _logger.Information("Graph API join success (userId={UserId}, gameVersion={GameVersion}, " +
                                "platform={Platform}, playerSessionId={SessionId}, targetNode={TargetNode})",
                session.UserIdHash, request.Version, session.Platform, session.PlayerSessionId,
                matchResult.RemoteEndPoint.ToString());

            response.ErrorCode = MultiplayerPlacementErrorCode.Success;
            response.PlayerSessionInfo.GameSessionId = matchResult.ManagerId;
            response.PlayerSessionInfo.DnsName = matchResult.RemoteEndPoint.Address.ToString();
            response.PlayerSessionInfo.Port = matchResult.RemoteEndPoint.Port;
            response.PlayerSessionInfo.BeatmapLevelSelectionMask = matchResult.BeatmapLevelSelectionMask;
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
                UserName = "Mystery Beater", // not provided to master through GameLift auth process
                SessionToken = sessionToken
            };
        }
        
        #endregion
    }
}