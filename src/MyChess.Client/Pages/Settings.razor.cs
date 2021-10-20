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
    protected bool _playAlwaysUp;
    protected bool _isNotificationsEnabled;

    protected bool IsLoading { get; set; } = false;

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
        IsLoading = true;
        Settings = await Client.GetSettingsAsync();
        IsLoading = false;
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

    protected void HandlePlayAlwaysUpChange(ChangeEventArgs changeEventArgs)
    {
    }

    protected void HandleNotificationChange(ChangeEventArgs changeEventArgs)
    {
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
