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
        var games = new List<MyChessGame>();

        try
        {
            games = await _client.GetFromJsonAsync<List<MyChessGame>>("/api/games?state=");
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
        ArgumentNullException.ThrowIfNull(games);
        return games;
    }
}
