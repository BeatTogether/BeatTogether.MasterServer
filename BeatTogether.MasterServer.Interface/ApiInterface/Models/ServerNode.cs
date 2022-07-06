using System;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Models
{
    public record ServerNode(string EndPoint, bool Online, DateTime LastRestart, DateTime LastOnline, string NodeVersion);
}
