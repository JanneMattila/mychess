using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyChess.Backend.Data;
using MyChess.Backend.Handlers;
using MyChess.Functions;

[assembly: FunctionsStartup(typeof(Startup))]

namespace MyChess.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
//#if DEBUG
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
//#endif
            builder.Services.AddOptions<AzureADOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("AzureAD").Bind(settings);
                });

            builder.Services.AddOptions<NotificationOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("WebPush").Bind(settings);
                });

            builder.Services.AddOptions<MyChessDataContextOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    settings.StorageConnectionString = configuration["Storage"];
                });

            builder.Services.AddTransient<ChessBoard>();
            builder.Services.AddSingleton<INotificationHandler, NotificationHandler>();
            builder.Services.AddSingleton<ISecurityValidator, SecurityValidator>();
            builder.Services.AddSingleton<IMyChessDataContext, MyChessDataContext>();
            builder.Services.AddSingleton<IMeHandler, MeHandler>();
            builder.Services.AddSingleton<ISettingsHandler, SettingsHandler>();
            builder.Services.AddSingleton<IGamesHandler, GamesHandler>();
            builder.Services.AddSingleton<IFriendsHandler, FriendsHandler>();
        }
    }
}
