using Microsoft.AspNetCore.Authorization;
using MyChess.Interfaces;

namespace MyChess.Client.Shared;

[Authorize]
public class GameListBase : MyChessComponentBase
{
    protected List<MyChessGame> Games { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await RefreshGames();
    }

    protected async Task RefreshGames()
    {
        AppState.IsLoading = true;
        Games = await Client.GetGamesAsync();
        AppState.IsLoading = false;
    }

    protected void AddNewGame()
    {
        NavigationManager.NavigateTo("/friends");
    }
}
