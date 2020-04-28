using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyChess.Functions;

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
            builder.Services.AddSingleton<ISecurityValidator, SecurityValidator>();
        }
    }
}
