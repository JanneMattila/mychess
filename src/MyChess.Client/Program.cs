﻿using BlazorApplicationInsights;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyChess;
using MyChess.Client;
using MyChess.Client.Models;
using MyChess.Client.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#App");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient<BackendClient>(
        client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient());

builder.Services.AddBlazorApplicationInsights(async applicationInsights =>
{
    var instrumentationKey = builder.Configuration.GetValue<string>("instrumentationKey");
    //var telemetryItem = new TelemetryItem()
    //{
    //    Tags = new Dictionary<string, object>()
    //        {
    //            { "ai.cloud.role", "SPA" },
    //            { "ai.cloud.roleInstance", "Blazor Wasm" },
    //        }
    //};

    //await applicationInsights.AddTelemetryInitializer(telemetryItem);
    await applicationInsights.SetInstrumentationKey(instrumentationKey);
    await applicationInsights.LoadAppInsights();
});
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddTransient<ChessBoard>();
builder.Services.AddScoped<AppState>();
builder.Services.AddSingleton<WebPushOptions>(wpo => new WebPushOptions()
{
    WebPushPublicKey = builder.Configuration.GetValue<string>("webPushPublicKey")
});

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
