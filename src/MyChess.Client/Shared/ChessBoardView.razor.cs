using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using MyChess.Interfaces;

namespace MyChess.Client.Shared;

public class ChessBoardViewBase : MyChessComponentBase
{
    [Parameter]
    public string ID { get; set; }

    protected bool IsLocal { get; set; } = false;

    public string FriendID { get; set; }

    protected bool IsNew { get; set; } = false;

    protected MyChessGame Game { get; set; } = new();

    public string Status { get; set; }
    public string Error { get; set; }
    public string LastComment { get; set; }
    public string ThinkTime { get; set; }
    public int CurrentMoveNumber { get; set; }

    protected bool ShowConfirmationDialog { get; set; }
    protected bool ShowPromotionDialog { get; set; }
    protected bool ShowCommentDialog { get; set; }
    protected bool ShowGameNameDialog { get; set; }
    protected bool ShowEllipse { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (ID == "local")
        {
            IsLocal = true;
        }
        else if (ID == "new")
        {
            IsNew = true;
            var query = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query;
            if (QueryHelpers.ParseQuery(query).TryGetValue("friendID", out var parameterFriendID))
            {
                FriendID = parameterFriendID;
            }
            ArgumentNullException.ThrowIfNull(FriendID);
        }
        else
        {
            await RefreshGame(ID);
        }
    }

    protected async Task RefreshGame(string id)
    {
        AppState.IsLoading = true;
        Game = await Client.GetGameAsync(id);
        AppState.IsLoading = false;
    }

    protected string GetThinkTime()
    {
        if (CurrentMoveNumber == 0 ||
            CurrentMoveNumber > Game.Moves.Count)
        {
            return "";
        }

        var move = Game.Moves[CurrentMoveNumber - 1];
        var thinkTime = (move.End - move.Start).TotalSeconds;
        var minutes = 0;
        var seconds = (int)Math.Floor(thinkTime);
        if (thinkTime > 60)
        {
            minutes = (int)Math.Floor(thinkTime / 60);
            seconds = (int)Math.Floor(thinkTime % 60);
        }

        if (minutes > 0)
        {
            return $"Move {CurrentMoveNumber} think time was {minutes} minutes and {seconds} seconds.";
        }
        return $"Move {CurrentMoveNumber} think time was {seconds} seconds.";
    }

    protected void ToggleEllipseMenu()
    {
        ShowEllipse = !ShowEllipse;
    }

    protected void ConfirmMove()
    {
    }

    protected void Cancel()
    {
    }

    protected void ConfirmPromotion()
    {
    }

    protected void ConfirmComment()
    {
    }

    protected void FirstMove()
    {
    }

    protected void PreviousMove()
    {
    }

    protected void NextMove()
    {
    }

    protected void LastMove()
    {
    }

    protected void ResignGame()
    {
    }
}
