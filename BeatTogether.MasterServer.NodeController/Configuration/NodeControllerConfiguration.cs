
namespace BeatTogether.MasterServer.NodeController.Configuration
{
    public class NodeControllerConfiguration
    {
        public string EndPoint { get; set; } = "127.0.0.1:2328";
        public Version MasterServerVersion { get; } = new(2,0,0);
        public Version[] SupportedDediServerVersions { get; } = { new(2,0,0) }; //for example, if 1.1 is here, then 1.1.1, 1.1.5, 1.1.23, would all be accepted verisions and 1.2.3 would not. Only change when dedi and master would be incompat otherwise
        public long TicksBetweenUpdatingCachedApiResponses { get; set; } = TimeSpan.TicksPerSecond;
    }
}
