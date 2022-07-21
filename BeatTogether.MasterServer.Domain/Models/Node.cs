using BeatTogether.MasterServer.Interface.ApiInterface.Models;
using System;
using System.Net;

namespace BeatTogether.MasterServer.Domain.Models
{
    public class Node
    {
        public IPAddress endpoint { get; }
        public bool Online { get; set; }
        public bool RemovedServerInstances { get; set; }
        public DateTime LastStart { get; set; }
        public DateTime LastOnline { get; set; }
        public string NodeVersion { get; set; }

        public Node(IPAddress endPoint, string Version)
        {
            endpoint = endPoint;
            Online = true;
            RemovedServerInstances = false;
            LastStart = DateTime.Now;
            LastOnline = DateTime.Now;
            NodeVersion = Version;
        }

        public ServerNode Convert()
        {
            return new ServerNode(endpoint.ToString(), Online, LastStart, LastOnline, NodeVersion);
        }
    }
}
