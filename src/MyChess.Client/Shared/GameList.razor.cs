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
        AppFocus.OnFocus += SilentUpdate;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        AppFocus.OnFocus -= SilentUpdate;
    }

    protected async Task SilentUpdate()
    {
        try
        {
            AppState.IsSmallLoading = true;

            Games = await Client.GetGamesAsync(Filters);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await AppInsights.TrackException(new BlazorApplicationInsights.Error()
            {
                Message = ex.Message,
                Name = "FailedSilentRefresh"
            });
        }
        AppState.IsSmallLoading = false;
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
