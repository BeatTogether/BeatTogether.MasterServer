using System;
using System.Linq;
using System.Net.Http;
using System.Text;
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
        public const string PicoUSVerifyUserUrl = "https://platform-us.picovr.com/s2s/v1/user/validate";
        public const string PicoCNVerifyUserUrl = "https://platform-cn.picovr.com/s2s/v1/user/validate";


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
	            if (session.PlayerPlatform != Platform.Pico)
	            {
		            var requestContent = new
		            {
			            proof = session.PlayerPlatform == Platform.Steam ? BitConverter.ToString(session.SessionToken).Replace("-", "") : Encoding.UTF8.GetString(session.SessionToken),
			            oculusId = session.PlayerPlatform == Platform.Oculus || session.PlayerPlatform == Platform.OculusQuest ? session.PlatformUserId : null,
			            steamId = session.PlayerPlatform == Platform.Steam ? session.PlatformUserId : null
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
		            var requestContent = new
		            {
			            access_token = Encoding.UTF8.GetString(session.SessionToken),
			            user_id = session.PlatformUserId
		            };
		            try
		            {
			            using var verifyResponse = await _httpClient.PostAsync(PicoUSVerifyUserUrl,
				            new StringContent(JsonSerializer.Serialize(requestContent), null, "application/json"));

			            verifyResponse.EnsureSuccessStatusCode();

			            var stringContent = await verifyResponse.Content.ReadAsStringAsync();
			            if (stringContent.Contains("\"is_validate\":true"))
			            {
				            authPasses = true;
				            authLogReason = "Authentication success";
			            }
			            else
			            {
				            _logger.Debug($"Pico US auth failed trying CN, API returned: {stringContent}");
							// Trying CN auth
							using var verifyResponseCN = await _httpClient.PostAsync(PicoCNVerifyUserUrl,
								new StringContent(JsonSerializer.Serialize(requestContent), null, "application/json"));

							verifyResponseCN.EnsureSuccessStatusCode();

							stringContent = await verifyResponseCN.Content.ReadAsStringAsync();
							if (stringContent.Contains("\"is_validate\":true"))
							{
								authPasses = true;
								authLogReason = "Authentication success";
							}
							else
							{
								authPasses = false;
								authLogReason = "Authentication rejected";
								_logger.Debug($"Pico CN auth failed, API returned: {stringContent}");
							}

						}
					}
		            catch (Exception)
		            {
			            authPasses = true;
			            authLogReason = "Pico verify request failed, skipping authentication";
		            }
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

            _logger.Information("Authentication success (reason={Reason} platform={Platform}, userId={UserId})", 
	            authLogReason, session.PlayerPlatform, session.PlatformUserId);
            
            return true;
        }

        private bool GetPlatformRequiresAuth(Platform platform)
        {
			_logger.Debug("Authed Platforms: " + string.Join(", ", _apiServerConfiguration.AuthedClients));
			return _apiServerConfiguration.AuthedClients.Contains(platform);
		}
    }
}