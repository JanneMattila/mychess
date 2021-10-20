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

    protected async Task HandleNotificationChange(ChangeEventArgs changeEventArgs)
    {
        NotificationText = String.Empty;
        Settings.Notifications.Clear();

        if (IsNotificationsEnabled)
        {
            try
            {
                var notificationSettings = await JS.InvokeAsync<UserSettings>("MyChessSettings.enableNotifications", "");
            }
            catch (JSException ex)
            {
                IsNotificationsEnabled = false;
                NotificationText = ex.Message;
            }
        }
    }

    protected void Save()
    {
        NavigationManager.NavigateTo("/");
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
