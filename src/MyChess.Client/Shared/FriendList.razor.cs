using Microsoft.AspNetCore.Authorization;
using MyChess.Interfaces;

namespace MyChess.Client.Shared;

[Authorize]
public class FriendListBase : MyChessComponentBase
{
    protected List<User> Friends { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await RefreshFriends();
    }

    protected async Task RefreshFriends()
    {
        AppState.IsLoading = true;
        Friends = await Client.GetFriendsAsync();
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
