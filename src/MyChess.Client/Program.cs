using BlazorApplicationInsights;
using BlazorApplicationInsights.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyChess;
using MyChess.Client;
using MyChess.Client.Extensions;
using MyChess.Client.Models;
using MyChess.Client.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#App");
builder.RootComponents.Add<HeadOutlet>("head::after");

// https://github.com/Azure/static-web-apps/issues/34
// https://github.com/dotnet/AspNetCore.Docs/issues/21334
// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/additional-scenarios
builder.Services.AddTransient<CustomAddressAuthorizationMessageHandler>();
builder.Services.AddHttpClient<BackendClient>(
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    //.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
    .AddHttpMessageHandler<CustomAddressAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient());

builder.Services.AddBlazorApplicationInsights(config => {
        var instrumentationKey = builder.Configuration.GetValue<string>("instrumentationKey");
        if (!string.IsNullOrEmpty(instrumentationKey))
        {
            config.InstrumentationKey = instrumentationKey;
        }
    },
    async applicationInsights =>
    {
        var telemetryItem = new TelemetryItem()
        {
            Tags = new()
                {
                    { "ai.cloud.role", "SPA" },
                    { "ai.cloud.roleInstance", "Blazor" },
                }
        };
        await applicationInsights.AddTelemetryInitializer(telemetryItem);
});
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddTransient<ChessBoard>();
builder.Services.AddScoped<AppState>();
builder.Services.AddSingleton<WebPushOptions>(wpo => new WebPushOptions()
{
    WebPushPublicKey = builder.Configuration.GetValue<string>("webPushPublicKey") ?? throw new ArgumentNullException("webPushPublicKey"),
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);

    options.ProviderOptions.Cache.CacheLocation = "localStorage";
    options.ProviderOptions.LoginMode = "redirect";

    var applicationIdURI = builder.Configuration.GetValue<string>("applicationIdURI");
    options.ProviderOptions.DefaultAccessTokenScopes.Add(applicationIdURI + "/User.ReadWrite");
    options.ProviderOptions.DefaultAccessTokenScopes.Add(applicationIdURI + "/Games.ReadWrite");
});

await builder.Build().RunAsync();
