using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using MyChess.Client.Extensions;
using MyChess.Client.Models;
using MyChess.Interfaces;

namespace MyChess.Client.Shared;

public class ChessBoardViewBase : MyChessComponentBase
{
    [Parameter]
    public string ID { get; set; }

    protected ElementReference _canvas;

    private DateTimeOffset _start = DateTimeOffset.UtcNow;
    private string _promotion = "";
    private int _pieceSize = 20;

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
    protected string GameName { get; set; }
    protected string Comment { get; set; }


    protected override async Task OnInitializedAsync()
    {
        Board.Initialize();

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

        if (IsLocal)
        {
            try
            {
                Console.WriteLine($"Loading local game from storage");
                var game = await JS.GetLocalStorage().Get<MyChessGame>("LocalGame");
                if (game != null)
                {
                    await MakeMoves(game);
                }
            }
            catch (Exception)
            {
                // Reset the loading due to failure
                Board.Initialize();
            }
        }
    }

    private async Task MakeMoves(MyChessGame game, int moves = int.MaxValue)
    {
        Board.Initialize();

        foreach (var gameMove in game.Moves.Take(moves))
        {
            Console.WriteLine($"Make move {gameMove.Move}");

            Board.MakeMove(gameMove.Move);
            if (gameMove.SpecialMove != null)
            {
                switch (gameMove.SpecialMove)
                {
                    case MyChessGameSpecialMove.PromotionToRook:
                        Board.ChangePromotion(PieceRank.Rook);
                        break;
                    case MyChessGameSpecialMove.PromotionToKnight:
                        Board.ChangePromotion(PieceRank.Knight);
                        break;
                    case MyChessGameSpecialMove.PromotionToBishop:
                        Board.ChangePromotion(PieceRank.Bishop);
                        break;
                }
            }
        }

        await DrawAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            _selfRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("MyChessPlay.initialize", _canvas, _selfRef);
            await DrawAsync();
        }
    }

    protected async Task RefreshGame(string id)
    {
        AppState.IsLoading = true;
        Game = await Client.GetGameAsync(id);
        CurrentMoveNumber = Game.Moves.Count + 1;
        await MakeMoves(Game);
        await DrawAsync();
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
        Status = GetBoardStatus();
        ThinkTime = GetThinkTime();
        LastComment = GetComment();

        var lastMove = Board.LastMove;
        var lastMoveCapture = Board.LastMoveCapture;

        var rows = new List<List<ChessBoardGraphics>>();
        for (var row = 0; row < ChessBoard.BOARD_SIZE; row++)
        {
            var cells = new List<ChessBoardGraphics>();
            for (var column = 0; column < ChessBoard.BOARD_SIZE; column++)
            {
                var piece = Board.GetPiece(column, row);
                var moveAvailable = PreviousAvailableMoves
                    .Any(p => p.To.VerticalLocation == row && p.To.HorizontalLocation == column);
                var lastMoveHighlight = "";
                if (lastMove != null)
                {
                    if (lastMove.From.HorizontalLocation == column &&
                        lastMove.From.VerticalLocation == row)
                    {
                        lastMoveHighlight = "HighlightPreviousFrom";
                    }
                    else if (lastMoveCapture != null &&
                        lastMoveCapture.From.HorizontalLocation == column &&
                        lastMoveCapture.From.VerticalLocation == row)
                    {
                        lastMoveHighlight = "HighlightCapture";
                    }
                    else if (lastMove.To.HorizontalLocation == column &&
                        lastMove.To.VerticalLocation == row)
                    {
                        lastMoveHighlight = "HighlightPreviousTo";
                    }
                }

                var image = piece.Player == PiecePlayer.None ?
                    "empty" :
                    piece.Rank.ToString().ToLower() + "_" + piece.Player.ToString().ToLower();

                cells.Add(new ChessBoardGraphics()
                {
                    MoveAvailable = moveAvailable,
                    LastMove = lastMoveHighlight,
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

        if (CurrentMoveNumber < Game.Moves.Count)
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

    [JSInvokable]
    public async Task CanvasOnClick(int x, int y)
    {
        Console.WriteLine($"CanvasOnClick: {x} - {y}");

        PreviousAvailableMoves = Board.GetAvailableMoves(x, y).ToList();
        await DrawAsync();
    }

    [JSInvokable]
    public async Task UpdateSize(int pieceSize)
    {
        Console.WriteLine($"UpdateSize: {pieceSize}");
        _pieceSize = pieceSize;
        await Task.CompletedTask;
    }

    public async Task CanvasOnClick(MouseEventArgs mouseEventArgs)
    {
        var column = (int)Math.Floor(mouseEventArgs.OffsetX / _pieceSize);
        var row = (int)Math.Floor(mouseEventArgs.OffsetY / _pieceSize);
        Console.WriteLine($"CanvasOnClick: {column} - {row}");

        if (PreviousAvailableMoves.Count > 0)
        {
            var selectedMove = PreviousAvailableMoves
                    .FirstOrDefault(p => p.To.VerticalLocation == row && p.To.HorizontalLocation == column);
            if (selectedMove != null)
            {
                Board.MakeMove(selectedMove);
                if (Board.LastMovePromotion != null)
                {
                    _promotion = "";
                    ShowPromotionDialog = true;
                }
                else
                {
                    ShowConfirmationDialog = true;
                }
            }
            PreviousAvailableMoves.Clear();
        }
        else
        {
            PreviousAvailableMoves = Board.GetAvailableMoves(column, row).ToList();
        }
        await DrawAsync();
    }

    protected async Task ConfirmMove()
    {
        ShowConfirmationDialog = false;

        if (IsLocal)
        {
            var lastMove = Board.LastMove;
            var lastPromotion = Board.LastMovePromotion;
            if (lastMove == null)
            {
                Console.WriteLine("No last move available");
                return;
            }

            var move = new MyChessGameMove
            {
                Move = lastMove.ToString(),
                Comment = "",
                Start = _start,
                End = DateTimeOffset.UtcNow
            };
            if (lastPromotion != null)
            {
                move.SpecialMove = lastPromotion.Rank switch
                {
                    PieceRank.Bishop => MyChessGameSpecialMove.PromotionToBishop,
                    PieceRank.Knight => MyChessGameSpecialMove.PromotionToKnight,
                    PieceRank.Rook => MyChessGameSpecialMove.PromotionToRook,
                    PieceRank.Queen => MyChessGameSpecialMove.PromotionToQueen,
                    _ => throw new Exception($"Invalid rank: {lastPromotion.Rank}")
                };
            }

            Game.Moves.Add(move);
            CurrentMoveNumber++;

            await SaveState();
        }
        else
        {
            ShowGameNameDialog = IsNew;
            ShowCommentDialog = true;
        }
    }

    protected async Task Cancel()
    {
        Error = string.Empty;
        ShowPromotionDialog = false;
        ShowCommentDialog = false;
        ShowConfirmationDialog = false;
        Board.Undo();
        await DrawAsync();
    }

    protected async Task ChangePromotion(ChangeEventArgs changeEventArgs)
    {
        var value = changeEventArgs.Value?.ToString();
        if (value != null)
        {
            _promotion = value;
        }

        if (_promotion == "Knight")
        {
            Board.ChangePromotion(PieceRank.Knight);
        }
        else if (_promotion == "Rook")
        {
            Board.ChangePromotion(PieceRank.Rook);
        }
        else if (_promotion == "Bishop")
        {
            Board.ChangePromotion(PieceRank.Bishop);
        }
        else
        {
            Board.ChangePromotion(PieceRank.Queen);
        }

        Console.WriteLine($"Changing promotion to {_promotion}");
        await DrawAsync();
    }

    protected async Task ConfirmPromotion()
    {
        ShowPromotionDialog = false;
        ShowConfirmationDialog = false;
        ShowCommentDialog = !IsLocal;

        if (IsLocal)
        {
            await SaveState();
        }
    }

    protected async Task ConfirmComment()
    {
        Error = string.Empty;
        if (IsNew && string.IsNullOrWhiteSpace(GameName))
        {
            Error = "Game name required";
            return;
        }

        var lastMove = Board.LastMove;
        var lastPromotion = Board.LastMovePromotion;
        if (lastMove == null)
        {
            Console.WriteLine("No last move available");
            return;
        }

        var move = new MyChessGameMove
        {
            Move = lastMove.ToString(),
            Comment = Comment,
            Start = _start,
            End = DateTimeOffset.UtcNow
        };
        if (lastPromotion != null)
        {
            move.SpecialMove = lastPromotion.Rank switch
            {
                PieceRank.Bishop => MyChessGameSpecialMove.PromotionToBishop,
                PieceRank.Knight => MyChessGameSpecialMove.PromotionToKnight,
                PieceRank.Rook => MyChessGameSpecialMove.PromotionToRook,
                PieceRank.Queen => MyChessGameSpecialMove.PromotionToQueen,
                _ => throw new Exception($"Invalid rank: {lastPromotion.Rank}")
            };
        }

        try
        {
            AppState.IsLoading = true;
            await Client.SubmitMoveAsync(Game.ID, move);
        }
        catch (Exception ex)
        {
            Console.Write(ex);

            if (IsNew)
            {
                Error = "Could not create game. Please try again later.";

            }
            else
            {
                Error = "Could not submit move. Please try again later.";
            }
            return;
        }
        finally
        {
            AppState.IsLoading = false;
        }
        Game.Moves.Add(move);
        CurrentMoveNumber++;
        ShowGameNameDialog = false;
        ShowCommentDialog = false;
        Comment = string.Empty;
        GameName = string.Empty;
    }

    protected async Task FirstMove()
    {
        await Cancel();
        CurrentMoveNumber = 1;
        await MakeMoves(Game, CurrentMoveNumber);
    }

    protected async Task PreviousMove()
    {
        await Cancel();
        CurrentMoveNumber = Math.Max(CurrentMoveNumber - 1, 1);
        await MakeMoves(Game, CurrentMoveNumber);
    }

    protected async Task NextMove()
    {
        await Cancel();
        CurrentMoveNumber = Math.Min(CurrentMoveNumber + 1, Game.Moves.Count + 1);
        await MakeMoves(Game, CurrentMoveNumber);
    }

    protected async Task LastMove()
    {
        await Cancel();
        CurrentMoveNumber = Game.Moves.Count + 1;
        await MakeMoves(Game, CurrentMoveNumber);
    }

    private async Task SaveState()
    {
        await JS.GetLocalStorage().Set("LocalGame", Game);
    }

    protected async Task ResignGame()
    {
        if (!await JS.Confirm("Do you want to resign current game?"))
        {
            return;
        }

        if (IsLocal)
        {
            await JS.GetLocalStorage().Delete("LocalGame");

            CurrentMoveNumber = 1;
            ShowEllipse = false;
            ShowPromotionDialog = false;
            ShowConfirmationDialog = false;
            ShowCommentDialog = false;
            Board.Initialize();
            PreviousAvailableMoves.Clear();
            await DrawAsync();
        }
        else
        {
            try
            {
                AppState.IsLoading = true;
                await Client.ResignGameAsync(Game.ID);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                Error = "Could not resign the game. Please try again later.";
                return;
            }
            finally
            {
                AppState.IsLoading = false;
            }
            NavigationManager.NavigateTo("/");
        }
    }
}
