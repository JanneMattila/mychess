using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyChess.Backend.Data;
using MyChess.Backend.Handlers;

namespace MyChess.Functions;

public class Program
{
    public static async Task Main()
    {
#if DEBUG
        Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
#endif

        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(s =>
            {
                s.AddApplicationInsightsTelemetryWorkerService();
                s.AddLogging(builder =>
                {
                    builder.AddApplicationInsights();
                });

                s.AddOptions<AzureADOptions>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection("AzureAD").Bind(settings);
                    });

                s.AddOptions<NotificationOptions>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection("WebPush").Bind(settings);
                    });

                s.AddOptions<MyChessDataContextOptions>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        settings.StorageConnectionString = configuration["Storage"];
                    });

                s.AddTransient<ChessBoard>();
                s.AddSingleton<INotificationHandler, NotificationHandler>();
                s.AddSingleton<ISecurityValidator, SecurityValidator>();
                s.AddSingleton<IMyChessDataContext, MyChessDataContext>();
                s.AddSingleton<IMeHandler, MeHandler>();
                s.AddSingleton<ISettingsHandler, SettingsHandler>();
                s.AddSingleton<IGamesHandler, GamesHandler>();
                s.AddSingleton<IFriendsHandler, FriendsHandler>();
            })
            .Build();

        await host.RunAsync();
    }
}
