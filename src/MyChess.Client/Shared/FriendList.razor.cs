using Microsoft.AspNetCore.Authorization;
using MyChess.Interfaces;

namespace MyChess.Client.Shared;

[Authorize]
public class FriendListBase : MyChessComponentBase
{
    protected List<User> Friends { get; set; } = new();
    protected string StatusMessage { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await RefreshFriends();
    }

    protected async Task RefreshFriends()
    {
        AppState.IsLoading = true;

        try
        {
            StatusMessage = string.Empty;
            Friends = await Client.GetFriendsAsync();
        }
        catch (Exception)
        {
            StatusMessage = "Could not load friends 😥";
        }
        AppState.IsLoading = false;
    }

    protected async Task Refresh()
    {
        await RefreshFriends();
    }

    protected void ManageFriend(string? id)
    {
        if (id != null)
        {
            NavigationManager.NavigateTo($"/friends/{id}");
        }
    }

    protected void AddNewFriend()
    {
        NavigationManager.NavigateTo("/friends/add");
    }
}
