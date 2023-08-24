using Newtonsoft.Json;
using System.Net;

namespace BeatTogether.MasterServer.Messaging.Models.HttpApi
{
    public class GetServerResponse
    {
        [JsonProperty("Name")]
        public string ServerName { get; set; }

        [JsonProperty("Id")]
        public string ServerId { get; set; }

        [JsonProperty("EndPoint")]
        public string ServerEndPoint { get; set; }

        [JsonProperty("Secret")]
        public string Secret { get; set; }

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Public")]
        public bool IsPublic { get; set; }

        [JsonProperty("InGameplay")]
        public bool IsInGameplay { get; set; }

        [JsonProperty("beatmap_level_selection_mask")]
        public BeatmapLevelSelectionMask BeatmapLevelSelectionMask { get; set; }

        [JsonProperty("gameplay_server_configuration")]
        public GameplayServerConfiguration GameplayServerConfiguration { get; set; }

        [JsonProperty("Player_count")]
        public int CurrentPlayerCount { get; set; }

        [JsonProperty("CurrentLevelId")]
        public string BeatmapLevelId { get; set; }

        [JsonProperty("Players")]
        public string[] UserHashes { get; set; }

        public GetServerResponse(IPAddress endPoint, string Name, string Id, string secret, string code, bool isPublic, bool isInGameplay, BeatmapLevelSelectionMask levelSelectionMask, GameplayServerConfiguration configuration, string[] userHashes, string beatmapLevelId)
        {
            ServerEndPoint = endPoint.ToString();
            ServerName = Name;
            ServerId = Id;
            Secret = secret;
            Code = code;
            IsPublic = isPublic;
            IsInGameplay = isInGameplay;
            BeatmapLevelSelectionMask = levelSelectionMask;
            GameplayServerConfiguration = configuration;
            CurrentPlayerCount = userHashes.Length;
            BeatmapLevelId = beatmapLevelId;
            UserHashes = userHashes;
        }
    }
}