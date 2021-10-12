using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyChess.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);

    options.ProviderOptions.LoginMode = "redirect";

    var applicationIdURI = builder.Configuration.GetValue<string>("applicationIdURI");
    options.ProviderOptions.DefaultAccessTokenScopes.Add(applicationIdURI + "/User.ReadWrite");
    options.ProviderOptions.DefaultAccessTokenScopes.Add(applicationIdURI + "/Games.ReadWrite");
});

await builder.Build().RunAsync();
