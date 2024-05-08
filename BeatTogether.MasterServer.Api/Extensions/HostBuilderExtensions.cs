using System.Net.Http;
using System.Security.Cryptography;
using BeatTogether.MasterServer.Api.Abstractions;
using BeatTogether.MasterServer.Api.Abstractions.Providers;
using BeatTogether.MasterServer.Api.Configuration;
using BeatTogether.MasterServer.Api.Implementations;
using BeatTogether.MasterServer.Api.Implimentations;
using BeatTogether.MasterServer.Kernel.Implementations.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseMasterServerApi(this IHostBuilder hostBuilder) =>
            hostBuilder
                .ConfigureAppConfiguration()
                .UseSerilog()
                .ConfigureWebHostDefaults(webHostBuilder =>
                    webHostBuilder
                        .ConfigureServices((hostBuilderContext, services) =>
                            services
                                .AddConfiguration<ApiServerConfiguration>("ApiServer")
                                .AddSingleton(RandomNumberGenerator.Create())
                                .AddSingleton<HttpClient>()
                                .AddSingleton<IServerCodeProvider, ServerCodeProvider>()
                                .AddSingleton<ISecretProvider, SecretProvider>()
                                .AddSingleton<IUserAuthenticator, UserAuthenticator>()
                                .AddSingleton<IMasterServerSessionService, MasterServerSessionService>()
                                .AddHostedService<MasterServerSessionTickService>()
                                .AddOptions()
                                .AddControllers()
                                .AddNewtonsoftJson()
                        )
                        .Configure(applicationBuilder =>
                            applicationBuilder
                                .UseRouting()
                                .UseEndpoints(endPointRouteBuilder => endPointRouteBuilder.MapControllers())
                        )
                );
    }
}
