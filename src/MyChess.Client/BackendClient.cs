using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MyChess.Interfaces;

namespace MyChess.Client;

public class BackendClient
{
    private readonly HttpClient _client;

    public BackendClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<List<MyChessGame>> GetGamesAsync()
    {
        var list = new List<MyChessGame>();

        try
        {
            list = await _client.GetFromJsonAsync<List<MyChessGame>>("/api/games?state=");
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
        ArgumentNullException.ThrowIfNull(list);
        return list;
    }

    public async Task<List<User>> GetFriendsAsync()
    {
        var list = new List<User>();

        try
        {
            list = await _client.GetFromJsonAsync<List<User>>("/api/users/me/friends");
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
        ArgumentNullException.ThrowIfNull(list);
        return list;
    }
}
