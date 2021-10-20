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
}
