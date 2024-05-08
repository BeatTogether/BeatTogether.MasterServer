﻿using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Api.Configuration;
using BeatTogether.MasterServer.Api.Abstractions;
using BeatTogether.Core.Enums;
using Serilog;

namespace BeatTogether.MasterServer.Api.Implementations
{
    public class UserAuthenticator : IUserAuthenticator
    {
        public const string BeatSaverVerifyUserUrl = "https://api.beatsaver.com/users/verify";

        private readonly ApiServerConfiguration _apiServerConfiguration;
        private readonly HttpClient _httpClient;

        private readonly ILogger _logger;

        public UserAuthenticator(
            ApiServerConfiguration apiServerConfiguration,
            HttpClient httpClient)
        {
            _apiServerConfiguration = apiServerConfiguration;
            _httpClient = httpClient;

            _logger = Log.ForContext<UserAuthenticator>();
        }

        public async Task<bool> TryAuthenticateUserWithPlatform(MasterServerSession session)
        {
            var authPasses = false;
            var authLogReason = "unknown";

            if (_apiServerConfiguration.AuthenticateClients &&
                GetPlatformRequiresAuth(session.PlayerPlatform))
            {

                var requestContent = new
                {
                    proof = BitConverter.ToString(session.SessionToken).Replace("-", ""),
                    oculusId = session.PlayerPlatform == Platform.Oculus ? session.PlatformUserId : "",
                    steamId = session.PlayerPlatform == Platform.Steam ? session.PlatformUserId : ""
                };

                try
                {
                    using var verifyResponse = await _httpClient.PostAsync(BeatSaverVerifyUserUrl,
                        new StringContent(JsonSerializer.Serialize(requestContent), null, "application/json"));

                    verifyResponse.EnsureSuccessStatusCode();

                    var stringContent = await verifyResponse.Content.ReadAsStringAsync();
                    if (stringContent.Contains("\"success\": false"))
                    {
                        authPasses = false;
                        authLogReason = "Authentication rejected";
                    }
                    else
                    {
                        authPasses = true;
                        authLogReason = "Authentication success";
                    }
                }
                catch (Exception)
                {
                    authPasses = true;
                    authLogReason = "BeatSaver verify request failed, skipping authentication";
                }
            }
            else
            {
                authPasses = true;
                authLogReason = $"Authentication not required for this platform";
            }

            if (!authPasses)
            {
                _logger.Information("Authentication failure (reason={Reason}, platform={Platform}, " +
                                "userId={UserId})",
                    authLogReason, session.PlayerPlatform, session.PlatformUserId);
                return false;
            }

            _logger.Information("Authentication success (platform={Platform}, userId={UserId})",
                session.PlayerPlatform, session.PlatformUserId);
            
            return true;
        }

        private bool GetPlatformRequiresAuth(Platform platform)
        {
            // TODO figure out why oculus does not authenticate correctly at some point

            return platform switch
            {
                Platform.Steam => true,
                _ => false,
            };
        }
    }
}