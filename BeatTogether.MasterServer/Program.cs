using BeatTogether.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BeatTogether.MasterServer.NodeController.HttpControllers;
using BeatTogether.MasterServer.Api.HttpControllers;
using System.Reflection;


namespace BeatTogether.MasterServer
{
    public static class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureAPIServices().UseMasterServerNodeController().UseMasterServerApi();

        public static IHostBuilder ConfigureAPIServices(this IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureServices( services => services.AddControllers()
                .AddApplicationPart(Assembly.GetAssembly(typeof(MasterServerController)))
                .AddApplicationPart(Assembly.GetAssembly(typeof(GetMultiplayerInstanceController)))
                .AddControllersAsServices()
            );
    }
}
