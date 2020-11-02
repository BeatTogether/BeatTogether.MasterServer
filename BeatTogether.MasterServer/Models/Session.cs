using System.Net;
using BeatTogether.MasterServer.Enums;

namespace BeatTogether.MasterServer.Models
{
    public class Session
    {
        public EndPoint EndPoint { get; set; }
        public SessionState State { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
