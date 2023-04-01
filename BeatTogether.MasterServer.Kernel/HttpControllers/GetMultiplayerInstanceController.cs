using System;
using System.Net;
using BeatTogether.MasterServer.HttpApi.Models.Enums;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Implementations;
using BeatTogether.MasterServer.Messaging.Models.HttpApi;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.HttpControllers
{
    [ApiController]
    public class GetMultiplayerInstanceController : Controller
    {
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private readonly IMasterServerSessionService _sessionService;

        public GetMultiplayerInstanceController(IMasterServerSessionService sessionService, IUserService userService)
        {
            _logger = Log.ForContext<GetMultiplayerInstanceController>();
            _userService = userService;
            _sessionService = sessionService;
        }

        /// <summary>
        /// Beat Saber sends this to request a server instance or begin matchmaking.
        /// </summary>
        [HttpPost]
        [Route("beat_saber_get_multiplayer_instance")]
        public IActionResult GetMultiplayerInstance([FromBody] GetMultiplayerInstanceRequest request)
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
                _logger.Information(
                    "Test - HTTP session resume test by psessid={Psess}",
                    session.PlayerSessionId);
            }
            else
            {
                session = _sessionService.GetOrAddSession(remoteEndPoint);
            }

            // Authenticate platform user and assign session ID if not yet authed
            if (string.IsNullOrEmpty(session.PlayerSessionId))
            {
                if (request.SingleUseAuthToken != null && request.AuthUserId != null)
                {
                    // TODO Use SingleUseAuthToken (= AuthenticationToken.sessionToken) to do platform auth like UserService

                    session.Platform = request.Platform;
                    session.UserId = request.UserId;
                    session.PlayerSessionId = Guid.NewGuid().ToString("N");

                    _logger.Information(
                        "Auth success (platform={Platform}, userId={UserId}, playerSessionId={PlayerSessionId})",
                        session.Platform, session.UserId, session.PlayerSessionId);
                }
                else
                {
                    _logger.Warning("Auth failure: Session did not go through platform authentication");
                    response.ErrorCode = MultiplayerPlacementErrorCode.AuthenticationFailed;
                    return new JsonResult(response);
                }
            }
            
            response.AddSessionContext(session.PlayerSessionId);

            // var authResult = _userService.Authenticate()
            response.ErrorCode = MultiplayerPlacementErrorCode.RequestTimeout;
            response.PollIntervalMs = 5000;
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
    }
}