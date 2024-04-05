using Newtonsoft.Json;
using System;
using System.Net;

namespace BeatTogether.MasterServer.Messaging.Models.HttpApi
{
    public class GetNodeResponse
    {
        [JsonProperty("Endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("Online")]
        public bool Online { get; set; }

        [JsonProperty("StartTime")]
        public DateTime LastStart { get; set; }

        [JsonProperty("LastOnline")]
        public DateTime LastOnline { get; set; }

        [JsonProperty("Version")]
        public string NodeVersion { get; set; }

        [JsonProperty("CurrentPlayers")]
        public int Players { get; set; } = 0;

        [JsonProperty("CurrentServers")]
        public int Servers { get; set; } = 0;

        public GetNodeResponse(IPAddress endPoint, bool online, DateTime lastStart, DateTime lastOnline, string version)
        {
            Endpoint = endPoint.ToString();
            Online = online;
            LastStart = lastStart;
            LastOnline = lastOnline;
            NodeVersion = version;
        }
    }
}