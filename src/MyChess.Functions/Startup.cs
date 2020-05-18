using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyChess.Data;
using MyChess.Functions;
using MyChess.Handlers;

[assembly: FunctionsStartup(typeof(Startup))]

namespace MyChess.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
#if DEBUG
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
#endif
            builder.Services.AddOptions<AzureADOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("AzureAD").Bind(settings);
                });

            builder.Services.AddOptions<MyChessDataContextOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    settings.StorageConnectionString = configuration["Storage"];
                });

            builder.Services.AddSingleton<ISecurityValidator, SecurityValidator>();
            builder.Services.AddSingleton<IMyChessDataContext, MyChessDataContext>();
            builder.Services.AddSingleton<IGamesHandler, GamesHandler>();
            builder.Services.AddSingleton<IFriendsHandler, FriendsHandler>();
        }
    }
}
