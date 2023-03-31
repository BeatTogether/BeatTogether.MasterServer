using BeatTogether.MasterServer.HttpApi.Models;
using BeatTogether.MasterServer.HttpApi.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BeatTogether.MasterServer.HttpApi.Controllers
{
    [ApiController]
    public class GetMultiplayerInstanceController
    {
        /// <summary>
        /// Beat Saber sends this request to request a server instance or begin matchmaking.
        /// </summary>
        [HttpPost]
        [Route("beat_saber_get_multiplayer_instance")]
        public IActionResult GetMultiplayerInstance()
        {
            return new JsonResult(
                new GetMultiplayerInstanceResponse()
            );
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