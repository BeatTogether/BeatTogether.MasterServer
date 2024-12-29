using System;
using System.Net.Http;
using System.Security.Cryptography;
using BeatTogether.MasterServer.Api.Abstractions;
using BeatTogether.MasterServer.Api.Abstractions.Providers;
using BeatTogether.MasterServer.Api.Configuration;
using BeatTogether.MasterServer.Api.Implementations;
using BeatTogether.MasterServer.Api.Implimentations;
using BeatTogether.MasterServer.Kernel.Implementations.Providers;
using BinaryRecords.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
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
                                .AddConfiguration<ApiServerConfiguration>("ServerConfiguration")
                                .AddSingleton(RandomNumberGenerator.Create())
                                .AddSingleton<IServerCodeProvider, ServerCodeProvider>()
                                .AddSingleton<ISecretProvider, SecretProvider>()
                                .AddSingleton<IUserAuthenticator, UserAuthenticator>()
                                .AddSingleton<IMasterServerSessionService, MasterServerSessionService>()
                                .AddHostedService<MasterServerSessionTickService>()
                                .AddSingleton<HttpClient>()
                                .AddOptions()
                                .AddControllers()
                                .AddNewtonsoftJson()
                        )
                        .Configure(applicationBuilder =>
                            applicationBuilder
	                            .Use((context, next) =>
	                            {
		                            context.Response.Headers.Add("X-Robots-Tag", "noindex, nofollow"); // Tell everyone that we don't want to be indexed
		                            return next(context);
	                            })
								.UseRouting()
                                .UseEndpoints(endPointRouteBuilder => endPointRouteBuilder.MapControllers())
						)
				);
    }
}
