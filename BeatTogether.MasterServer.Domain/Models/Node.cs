using System;
using System.Net;

namespace BeatTogether.MasterServer.Domain.Models
{
    public class Node
    {
        public IPAddress Endpoint { get; }
        public bool Online { get; set; }
        public DateTime LastStart { get; set; }
        public DateTime LastOnline { get; set; }
        public Version NodeVersion { get; set; }

        public Node(IPAddress endPoint, Version Version)
        {
            Endpoint = endPoint;
            Online = true;
            LastStart = DateTime.UtcNow;
            LastOnline = DateTime.UtcNow;
            NodeVersion = Version;
        }
    }
}
