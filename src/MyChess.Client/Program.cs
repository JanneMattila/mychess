using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyChess.Client;
using MyChess.Client.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#App");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient<BackendClient>(
        client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient());

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<AppState>();

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);

    options.ProviderOptions.LoginMode = "redirect";

    var applicationIdURI = builder.Configuration.GetValue<string>("applicationIdURI");
    options.ProviderOptions.DefaultAccessTokenScopes.Add(applicationIdURI + "/User.ReadWrite");
    options.ProviderOptions.DefaultAccessTokenScopes.Add(applicationIdURI + "/Games.ReadWrite");
});

await builder.Build().RunAsync();
