using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MyChess.Client.Shared;
using MyChess.Interfaces;

namespace MyChess.Client.Pages;

[Authorize]
public class ModifyFriendBase : MyChessComponentBase
{
    [Parameter]
    public string ID { get; set; }

    public string Title { get; set; } = "Modify friend";

    protected HandlerError? Error { get; set; } = null;

    protected User Friend { get; set; } = new();

    protected List<User> Friends { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (ID == "add")
        {
            Title = "Add friend";
        }
        else
        {
            await RefreshFriends();
            var f = Friends.FirstOrDefault(f => f.ID == ID);
            if (f != null)
            {
                Friend = f;
            }
        }
    }

    protected async Task RefreshFriends()
    {
        AppState.IsLoading = true;
        Friends = await Client.GetFriendsAsync();
        AppState.IsLoading = false;
    }


    protected async Task Save()
    {
        Error = null;
        try
        {
            Error = await Client.UpsertFriendAsync(Friend);

            if (Error == null)
            {
                NavigationManager.NavigateTo("/friends");
            }
        }
        catch (Exception ex)
        {
            Error = new HandlerError()
            {
                Detail = ex.Message
            };
        }
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/friends");
    }
}
