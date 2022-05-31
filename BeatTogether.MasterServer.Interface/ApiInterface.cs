using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.MasterServer.Interface.Abstractions;

namespace BeatTogether.MasterServer.Interface
{
    public class ApiInterface : IApiInterface
    {
        private readonly IAutobus _autobus;
        private readonly IMatchmakingService _matchmakingService;

        public ApiInterface(
            IMatchmakingService matchmakingService, IAutobus autobus)
        {
            _matchmakingService = matchmakingService;
            _autobus = autobus;
        }
    }
                
}
