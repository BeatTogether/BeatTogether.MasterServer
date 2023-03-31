using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace BeatTogether.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseHttpApi(this IHostBuilder hostBuilder) =>
            hostBuilder
                .ConfigureWebHostDefaults(webHostBuilder =>
                    webHostBuilder
                        .ConfigureServices((hostBuilderContext, services) =>
                            services
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
