using Microsoft.AspNetCore.Authorization;
using MyChess.Interfaces;

namespace MyChess.Client.Shared;

[Authorize]
public class GameListBase : MyChessComponentBase
{
    protected List<MyChessGame> Games { get; set; }

    protected bool IsLoading = false;

    protected override async Task OnInitializedAsync()
    {
        await RefreshGames();
    }

    protected async Task RefreshGames()
    {
        IsLoading = true;
        Games = await Client.GetGamesAsync();
        IsLoading = false;
    }

    protected void AddNewGame()
    {
    }
}
