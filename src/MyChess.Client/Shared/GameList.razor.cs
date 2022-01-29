using Microsoft.AspNetCore.Authorization;
using MyChess.Interfaces;

namespace MyChess.Client.Shared;

[Authorize]
public class GameListBase : MyChessComponentBase
{
    protected bool ShowFilters { get; set; } = false;
    protected string Filters { get; set; } = GameFilterType.WaitingForYou;
    protected string Title { get; set; } = "Games waiting for you";
    protected List<MyChessGame> Games { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await RefreshGames();
    }

    protected async Task RefreshGames()
    {
        AppState.IsLoading = true;
        ShowFilters = false;
        Games = await Client.GetGamesAsync(Filters);
        AppState.IsLoading = false;
    }

    protected void AddNewGame()
    {
        NavigationManager.NavigateTo("/friends");
    }

    protected void ToggleFilterVisibility()
    {
        ShowFilters = !ShowFilters;
    }

    protected async Task WaitingForYou()
    {
        Title = "Games waiting for you";
        Filters = GameFilterType.WaitingForYou;
        await RefreshGames();
    }

    protected async Task WaitingForOpponent()
    {
        Title = "Games waiting for opponent";
        Filters = GameFilterType.WaitingForOpponent;
        await RefreshGames();
    }

    protected async Task Archive()
    {
        Title = "Archive";
        Filters = GameFilterType.Archive;
        await RefreshGames();
    }
}
