using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MyChess.Interfaces;

namespace MyChess.Client.Extensions;

// Due to:
// https://github.com/Azure/static-web-apps/issues/34
// This code is adopting this part of the code just to change header name!
// https://github.com/dotnet/aspnetcore/blob/main/src/Components/WebAssembly/WebAssembly.Authentication/src/Services/AuthorizationMessageHandler.cs
public class CustomAddressAuthorizationMessageHandler : DelegatingHandler, IDisposable
{
    private readonly IAccessTokenProvider _provider;
    private readonly NavigationManager _navigationManager;
    private readonly AuthenticationStateChangedHandler? _authenticationStateChangedHandler;
    private AccessToken? _lastToken;
    private string _cachedHeader = string.Empty;
    private Uri _authorizedUri;

    public CustomAddressAuthorizationMessageHandler(IAccessTokenProvider provider, NavigationManager navigationManager)
    {
        _provider = provider;
        _navigationManager = navigationManager;

        if (_provider is AuthenticationStateProvider authStateProvider)
        {
            _authenticationStateChangedHandler = _ => { _lastToken = null; };
            authStateProvider.AuthenticationStateChanged += _authenticationStateChangedHandler;
        }
        _authorizedUri = new Uri(navigationManager.BaseUri, UriKind.Absolute);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;
        if (request.RequestUri != null && _authorizedUri.IsBaseOf(request.RequestUri))
        {
            if (_lastToken == null || now >= _lastToken.Expires.AddMinutes(-5))
            {
                var tokenResult = await _provider.RequestAccessToken();
                if (tokenResult.TryGetToken(out var token))
                {
                    _lastToken = token;
                    _cachedHeader = $"Bearer {_lastToken.Value}";
                }
                else
                {
                    throw new AccessTokenNotAvailableException(_navigationManager, tokenResult, null);
                }
            }

            request.Headers.Add(CustomHeaderNames.Authorization, _cachedHeader);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    public new void Dispose()
    {
        if (_provider is AuthenticationStateProvider authStateProvider)
        {
            authStateProvider.AuthenticationStateChanged -= _authenticationStateChangedHandler;
        }
        Dispose(disposing: true);
    }
}
