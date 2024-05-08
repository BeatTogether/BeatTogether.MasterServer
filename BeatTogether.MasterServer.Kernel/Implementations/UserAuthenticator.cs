﻿using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Kernel.Util;
using BeatTogether.MasterServer.Messaging.Enums;
using BeatTogether.MasterServer.Messaging.Models;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class UserAuthenticator : IUserAuthenticator
    {
        public const string BeatSaverVerifyUserUrl = "https://api.beatsaver.com/users/verify";

        private readonly MasterServerConfiguration _masterServerConfiguration;
        private readonly HttpClient _httpClient;

        private readonly ILogger _logger;

        public UserAuthenticator(
            MasterServerConfiguration masterServerConfiguration,
            HttpClient httpClient)
        {
            _masterServerConfiguration = masterServerConfiguration;
            _httpClient = httpClient;

            _logger = Log.ForContext<UserAuthenticator>();
        }

        public async Task<bool> TryAuthenticateUserWithPlatform(MasterServerSession session, AuthenticationToken token)
        {
            var authPasses = false;
            var authLogReason = "unknown";

            if (_masterServerConfiguration.AuthenticateClients &&
                GetPlatformRequiresAuth(token.Platform))
            {
                if(session.Platform == Platform.Steam && (session.PlatformUserId == "76561199098860979" || session.UserName == "IGGAMES" || session.UserName == "IGGGAMES"))
                {
                    _logger.Information("an IGG player just tried joining lmao");
                    return false;
                }

                var requestContent = new
                {
                    proof = BitConverter.ToString(token.SessionToken).Replace("-", ""),
                    oculusId = token.Platform == Platform.Oculus ? token.UserId : "",
                    steamId = token.Platform == Platform.Steam ? token.UserId : ""
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
                                "userId={UserId}, userName={UserName})",
                    authLogReason, token.Platform, token.UserId);
                return false;
            }

            _logger.Information("Authentication success (platform={Platform}, userId={UserId}, " +
                                "userName={UserName})",
                token.Platform, token.UserId);
            
            session.Platform = token.Platform;
            session.PlatformUserId = token.UserId;
            session.UserIdHash = UserIdHash.Generate(session.Platform, session.PlatformUserId);
            
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