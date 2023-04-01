using BeatTogether.MasterServer.HttpApi.Models.Enums;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Models;
using BeatTogether.MasterServer.Messaging.Models.HttpApi;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.HttpControllers
{
    [ApiController]
    public class GetMultiplayerInstanceController
    {
        private readonly ILogger _logger;
        private readonly IMasterServerSessionService _sessionService;

        public GetMultiplayerInstanceController(IMasterServerSessionService sessionService)
        {
            _logger = Log.ForContext<GetMultiplayerInstanceController>();
            _sessionService = sessionService;
        }
        
        /// <summary>
        /// Beat Saber sends this to request a server instance or begin matchmaking.
        /// </summary>
        [HttpPost]
        [Route("beat_saber_get_multiplayer_instance")]
        public IActionResult GetMultiplayerInstance([FromBody] GetMultiplayerInstanceRequest request)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            Serilog.Log.Logger.Information($"Get instance (Version={request.Version}, " +
                                           $"ServiceEnvironment={request.ServiceEnvironment}, " +
                                           $"SingleUseAuthToken={request.SingleUseAuthToken}, " +
                                           $"BeatmapLevelSelectionMask={request.BeatmapLevelSelectionMask}, " +
                                           $"GameplayServerConfiguration={request.GameplayServerConfiguration}, " +
                                           $"UserId={request.UserId}, " +
                                           $"PrivateGameSecret={request.PrivateGameSecret}, " +
                                           $"PrivateGameCode={request.PrivateGameCode}, " +
                                           $"Platform={request.Platform}, " +
                                           $"AuthUserId={request.AuthUserId}, " +
                                           $"GameliftRegionLatencies={request.GameliftRegionLatencies}, " +
                                           $"TicketId={request.TicketId}, " +
                                           $"PlacementId={request.PlacementId})");


            var response = new GetMultiplayerInstanceResponse()
            {
                ErrorCode = MultiplayerPlacementErrorCode.MatchmakingTimeout,
                PlayerSessionInfo = new PlayerSessionInfo()
                {
                    PrivateGameCode = "abc",
                    PrivateGameSecret = "def",
                    GameplayServerConfiguration = new GameplayServerConfiguration()
                    {
                        DiscoveryPolicy = DiscoveryPolicy.Public,
                        InvitePolicy = InvitePolicy.AnyoneCanInvite,
                        GameplayServerMode = GameplayServerMode.Managed,
                        MaxPlayerCount = 6,
                        SongSelectionMode =SongSelectionMode.ManagerPicks,
                        GameplayServerControlSettings = GameplayServerControlSettings.All
                    },
                    BeatmapLevelSelectionMask = request.BeatmapLevelSelectionMask,
                    Port = 1234,
                    DnsName = "test",
                    GameSessionId = "test",
                    PlayerSessionId = "test"
                },
                PollIntervalMs = 5000,
                PlacementStatus = "SEARCHING"
            };
            return new JsonResult(response);
        }
        
        /// <summary>
        /// Beat Saber sends this request when matchmaking gets cancelled by the user.
        /// </summary>
        [HttpPost]
        [Route("beat_saber_multiplayer_cancel_matchmaking_ticket")]
        public IActionResult CancelMatchmakingTicket()
        {
            return new JsonResult(
                GetMultiplayerInstanceResponse.ForErrorCode(MultiplayerPlacementErrorCode.ConnectionCanceled)
            );
        }
    }
}