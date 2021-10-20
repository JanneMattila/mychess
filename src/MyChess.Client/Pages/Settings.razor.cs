using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;
using MyChess.Client.Shared;
using MyChess.Interfaces;

namespace MyChess.Client.Pages;

[Authorize]
public class SettingsBase : MyChessComponentBase
{
    protected bool IsNotificationsEnabled { get; set; } = false;
    protected string NotificationText { get; set; } = string.Empty;

    protected UserSettings Settings { get; set; } = new();

    [AllowNull]
    [Inject]
    protected SignOutSessionStateManager SignOutManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await RefreshSettings();
    }

    protected async Task RefreshSettings()
    {
        AppState.IsLoading = true;
        Settings = await Client.GetSettingsAsync();

        IsNotificationsEnabled = Settings.Notifications.Any();
        AppState.IsLoading = false;
    }

    protected async Task CopyIdentifierToClipboard()
    {
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", Settings.ID);
    }

    protected async Task CopyAddFriendLinkToClipboard()
    {
        var uri = $"{NavigationManager.BaseUri}friends/add/{Settings.ID}";
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", uri);
    }

    protected async Task Save()
    {
        NotificationText = String.Empty;
        Settings.Notifications.Clear();

        try
        {
            var settings = new UserSettings();
            if (IsNotificationsEnabled)
            {
                var notificationSettings = await JS.InvokeAsync<UserNotifications>("MyChessSettings.enableNotifications", "");
                settings.Notifications.Add(new UserNotifications()
                {
                    Enabled = true,
                    Name = "browser1",
                    Endpoint = notificationSettings.Endpoint,
                    Auth = notificationSettings.Auth,
                    P256dh = notificationSettings.P256dh
                });
            }

            await Client.UpsertSettingsAsync(settings);

            NavigationManager.NavigateTo("/");
        }
        catch (JSException ex)
        {
            IsNotificationsEnabled = false;
            NotificationText = ex.Message;
        }
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/");
    }

    protected async Task SignOut()
    {
        await SignOutManager.SetSignOutState();
        NavigationManager.NavigateTo("authentication/logout");
    }
}
