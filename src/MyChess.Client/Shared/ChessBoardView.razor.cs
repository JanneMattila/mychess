using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using MyChess.Client.Models;
using MyChess.Interfaces;

namespace MyChess.Client.Shared;

public class ChessBoardViewBase : MyChessComponentBase
{
    [Parameter]
    public string ID { get; set; }

    protected ElementReference _canvas;

    private DotNetObjectReference<ChessBoardViewBase> _selfRef;

    protected bool IsLocal { get; set; } = false;

    public string FriendID { get; set; }

    protected bool IsNew { get; set; } = false;

    protected MyChessGame Game { get; set; } = new();

    [Inject]
    protected ChessBoard Board { get; set; }

    public string Status { get; set; }
    public string Error { get; set; }
    public string LastComment { get; set; }
    public string ThinkTime { get; set; }
    public List<ChessMove> PreviousAvailableMoves { get; set; } = new();
    public int CurrentMoveNumber { get; set; }

    protected bool ShowConfirmationDialog { get; set; }
    protected bool ShowPromotionDialog { get; set; }
    protected bool ShowCommentDialog { get; set; }
    protected bool ShowGameNameDialog { get; set; }
    protected bool ShowEllipse { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            _selfRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("MyChessPlay.initialize", _canvas, _selfRef);
        }
    }

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

        Board.Initialize();
        await DrawAsync();
    }

    protected async Task RefreshGame(string id)
    {
        AppState.IsLoading = true;
        Game = await Client.GetGameAsync(id);
        AppState.IsLoading = false;
    }

    protected string GetComment()
    {
        if (CurrentMoveNumber == 0 ||
            CurrentMoveNumber > Game.Moves.Count)
        {
            return "";
        }

        var move = Game.Moves[CurrentMoveNumber - 1];
        return move.Comment;
    }

    protected async Task DrawAsync()
    {
        var lastMove = Board.LastMove;
        var lastMoveCapture = Board.LastMoveCapture;

        var rows = new List<List<ChessBoardGraphics>>();
        for (var row = 0; row < ChessBoard.BOARD_SIZE; row++)
        {
            var cells = new List<ChessBoardGraphics>();
            for (var column = 0; column < ChessBoard.BOARD_SIZE; column++)
            {
                var piece = Board.GetPiece(column, row);

                var className = (row + column) % 2 == 0 ?
                    "lightCell" :
                    "darkCell";

                for (var i = 0; i < PreviousAvailableMoves.Count; i++)
                {
                    var move = PreviousAvailableMoves[i];
                    if (row == move.To.VerticalLocation &&
                        column == move.To.HorizontalLocation)
                    {
                        className += " highlightMoveAvailable";
                    }
                }

                if (lastMove != null)
                {
                    if (lastMove.From.HorizontalLocation == column &&
                        lastMove.From.VerticalLocation == row)
                    {
                        className += " highlightPreviousFrom";
                    }
                    else if (lastMoveCapture != null &&
                        lastMoveCapture.From.HorizontalLocation == column &&
                        lastMoveCapture.From.VerticalLocation == row)
                    {
                        className += " highlightCapture";
                    }
                    else if (lastMove.To.HorizontalLocation == column &&
                        lastMove.To.VerticalLocation == row)
                    {
                        className += " highlightPreviousTo";
                    }
                }

                var key = "" + row + "-" + column;
                var image = piece.Player == PiecePlayer.None ?
                    "empty" :
                    piece.Rank.ToString().ToLower() + "_" + piece.Player.ToString().ToLower();

                cells.Add(new ChessBoardGraphics()
                {
                    Key = key,
                    Background = className,
                    Image = image
                });
            }
            rows.Add(cells);
        }

        await JS.InvokeVoidAsync("MyChessPlay.draw", rows);
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

    protected string GetBoardStatus()
    {
        var status = Board.GetBoardState();
        var gameStatusMessage = "";

        if (CurrentMoveNumber != Game.Moves.Count)
        {
            gameStatusMessage = "Move ";
            gameStatusMessage += CurrentMoveNumber;
        }

        switch (status)
        {
            case ChessBoardState.StaleMate:
                gameStatusMessage = "Stalemate";
                break;

            case ChessBoardState.Check:
                if (gameStatusMessage.Length > 0)
                {
                    gameStatusMessage += " - Check";
                }
                else
                {
                    gameStatusMessage = "Check";
                }
                break;

            case ChessBoardState.CheckMate:
                gameStatusMessage = "Checkmate!";
                break;

            default:
                gameStatusMessage += "";
                break;
        }
        return gameStatusMessage;
    }

    protected async Task ToggleEllipseMenu()
    {
        ShowEllipse = !ShowEllipse;

        await DrawAsync();
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
