using Microsoft.AspNetCore.Authorization;
using MyChess.Interfaces;

namespace MyChess.Client.Shared;

[Authorize]
public class FriendListBase: MyChessComponentBase
{
    protected List<User> Friends { get; set; }

    protected bool IsLoading = false;

    protected override async Task OnInitializedAsync()
    {
        await RefreshFriends();
    }

    protected async Task RefreshFriends()
    {
        IsLoading = true;
        Friends = await Client.GetFriendsAsync();
        IsLoading = false;
    }

    protected void ManageFriend()
    {
    }

    protected void AddNewFriend()
    {
    }
}
