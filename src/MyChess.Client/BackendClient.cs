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

    public async Task<MyChessGame> GetGameAsync(string id, string state = "")
    {
        var game = new MyChessGame();

        try
        {
            game = await _client.GetFromJsonAsync<MyChessGame>($"/api/games/{id}?state={state}");
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
        ArgumentNullException.ThrowIfNull(game);
        return game;
    }

    public async Task<List<MyChessGame>> GetGamesAsync(string state = "")
    {
        var list = new List<MyChessGame>();

        try
        {
            list = await _client.GetFromJsonAsync<List<MyChessGame>>($"/api/games?state={state}");
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

    public async Task<UserSettings> GetSettingsAsync()
    {
        var settings = new UserSettings();
        try
        {
            settings = await _client.GetFromJsonAsync<UserSettings>("/api/users/me/settings");
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
        ArgumentNullException.ThrowIfNull(settings);
        return settings;
    }

    public async Task UpsertSettingsAsync(UserSettings userSettings)
    {
        try
        {
            await _client.PostAsJsonAsync("/api/users/me/settings", userSettings);
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    public async Task<HandlerError?> UpsertFriendAsync(User friend)
    {
        try
        {
            var response = await _client.PostAsJsonAsync($"/api/users/me/friends/{friend.ID}", friend);
            if (!response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<HandlerError?>();
            }
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return null;
    }

    public async Task SubmitMoveAsync(string id, MyChessGameMove move)
    {
        try
        {
            var response = await _client.PostAsJsonAsync($"/api/games/{id}/moves", move);
            response.EnsureSuccessStatusCode();
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }


    public async Task ResignGameAsync(string id)
    {
        try
        {
            var response = await _client.DeleteAsync($"/api/games/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }
}
