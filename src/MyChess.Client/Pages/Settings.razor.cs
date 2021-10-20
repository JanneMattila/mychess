using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MyChess.Client.Shared;
using MyChess.Interfaces;

namespace MyChess.Client.Pages;

[Authorize]
public class SettingsBase : MyChessComponentBase
{
    protected bool _playAlwaysUp;
    protected bool _isNotificationsEnabled;

    protected bool IsLoading = false;

    protected UserSettings Settings { get; set; }

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
    protected void SignOut()
    {
        NavigationManager.NavigateTo("/");
    }
}
