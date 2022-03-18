using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MyChess.Client.Extensions;
using MyChess.Client.Models;
using MyChess.Interfaces;

namespace MyChess.Client.Shared;

public class ChessBoardViewBase : MyChessComponentBase
{
    [Parameter]
    public string ID { get; set; } = string.Empty;

    protected ElementReference _canvas;

    private DateTimeOffset _start = DateTimeOffset.UtcNow;
    private string _promotion = "";
    private int _pieceSize = 20;
    private bool _updateAfterAnimation = false;

    private DotNetObjectReference<ChessBoardViewBase>? _selfRef;

    protected bool IsLocal { get; set; } = false;

    public string FriendID { get; set; } = string.Empty;

    protected bool IsNew { get; set; } = false;

    protected MyChessGame Game { get; set; } = new();

    [Inject]
    protected ChessBoard Board { get; set; } = new(NullLogger<ChessBoard>.Instance);

    public string Status { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public string LastComment { get; set; } = string.Empty;
    public string ThinkTime { get; set; } = string.Empty;
    public List<ChessMove> PreviousAvailableMoves { get; set; } = new();
    public int CurrentMoveNumber { get; set; }

    protected bool ShowConfirmationDialog { get; set; }
    protected bool ShowPromotionDialog { get; set; }
    protected bool ShowCommentDialog { get; set; }
    protected bool ShowGameNameDialog { get; set; }
    protected bool ShowEllipse { get; set; }
    protected string GameName { get; set; } = string.Empty;
    protected string Comment { get; set; } = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        Console.WriteLine($"Parameters updated: {ID}");

        ShowConfirmationDialog = false;
        ShowPromotionDialog = false;
        ShowCommentDialog = false;
        ShowGameNameDialog = false;
        ShowEllipse = false;
        GameName = string.Empty;
        Comment = string.Empty;
        Status = string.Empty;
        Error = string.Empty;
        LastComment = string.Empty;
        ThinkTime = string.Empty;
        CurrentMoveNumber = 0;
        PreviousAvailableMoves.Clear();
        Board.Initialize();
        Game = new();
        IsLocal = false;
        IsNew = false;

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
                    Game = game;
                    CurrentMoveNumber = Game.Moves.Count;
                }
            }
            catch (Exception)
            {
                // Reset the loading due to failure
                Board.Initialize();
            }
        }
        await base.OnParametersSetAsync();
        await DrawAsync(direction: 1);
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        AppFocus.OnFocus += SilentUpdate;
        await Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        AppFocus.OnFocus -= SilentUpdate;
    }

    protected async Task SilentUpdate()
    {
        if (IsLocal || IsNew)
        {
            return;
        }

        try
        {
            AppState.IsSmallLoading = true;
            Console.WriteLine("Checking for game updates in the background");

            var state = string.Empty;
            var query = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query;
            if (QueryHelpers.ParseQuery(query).TryGetValue("state", out var parameterState))
            {
                state = parameterState;
            }
            var gameUpdate = await Client.GetGameAsync(ID, state);

            if (gameUpdate != null)
            {
                if (gameUpdate.Moves.Count > Game.Moves.Count)
                {
                    Console.WriteLine("Game updated silently in the background");
                    Game = gameUpdate;
                    CurrentMoveNumber = Game.Moves.Count;
                    await MakeMoves(Game);
                    await DrawAsync(direction: 1);
                }
                else
                {
                    Console.WriteLine("No game updates");
                }
            }
        }
        catch (Exception ex)
        {
            await AppInsights.TrackException(new BlazorApplicationInsights.Error()
            {
                Message = ex.Message,
                Name = "FailedSilentRefreshGameview"
            });
        }
        AppState.IsSmallLoading = false;
        StateHasChanged();
    }

    private async Task MakeMoves(MyChessGame game, int moves = int.MaxValue, int direction = 0)
    {
        PreviousAvailableMoves.Clear();
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

        await DrawAsync(direction);
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
        var state = string.Empty;
        var query = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query;
        if (QueryHelpers.ParseQuery(query).TryGetValue("state", out var parameterState))
        {
            state = parameterState;
        }
        Game = await Client.GetGameAsync(id, state);
        CurrentMoveNumber = Game.Moves.Count;
        await MakeMoves(Game);
        AppState.IsLoading = false;
        await DrawAsync(direction: 1);
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

    protected async Task DrawAsync(int direction = 0)
    {
        Status = GetBoardStatus();
        ThinkTime = GetThinkTime();
        LastComment = GetComment();

        var graphics = new ChessBoardDraw();

        if (direction != 0)
        {
            foreach (var lastMove in Board.LastMoveList.OrderBy(m => m))
            {
                var from = new ChessBoardPosition()
                {
                    Column = lastMove.From.Column,
                    Row = lastMove.From.Row
                };
                var to = new ChessBoardPosition()
                {
                    Column = lastMove.To.Column,
                    Row = lastMove.To.Row
                };

                if (lastMove.SpecialMove == ChessSpecialMove.Capture)
                {
                    // Captured piece to locations is outside board.
                    to = from;
                }

                graphics.Animations.Add(new ChessBoardAnimation()
                {
                    From = direction > 0 ? from : to,
                    To = direction > 0 ? to : from,
                    Data = lastMove.Rank.ToString().ToLower() + "_" + lastMove.Player.ToString().ToLower()
                });
            }
        }
        else
        {
            var lastMove = Board.LastMove;
            var lastMoveCapture = Board.LastMoveCapture;
            foreach (var previousMove in PreviousAvailableMoves)
            {
                graphics.AvailableMoves.Add(new ChessBoardPosition()
                {
                    Column = previousMove.To.Column,
                    Row = previousMove.To.Row
                });
            }

            // Add different highlights to board: Previous moves and captures
            if (lastMove != null)
            {
                graphics.Highlights.Add(new ChessBoardGraphics()
                {
                    Column = lastMove.From.Column,
                    Row = lastMove.From.Row,
                    Data = "HighlightPreviousFrom"
                });
                if (lastMoveCapture != null)
                {
                    graphics.Highlights.Add(new ChessBoardGraphics()
                    {
                        Column = lastMoveCapture.From.Column,
                        Row = lastMoveCapture.From.Row,
                        Data = "HighlightCapture"
                    });
                }
                else
                {
                    graphics.Highlights.Add(new ChessBoardGraphics()
                    {
                        Column = lastMove.To.Column,
                        Row = lastMove.To.Row,
                        Data = "HighlightPreviousTo"
                    });
                }
            }
        }

        // Add all pieces on the board
        for (var row = 0; row < ChessBoard.BOARD_SIZE; row++)
        {
            for (var column = 0; column < ChessBoard.BOARD_SIZE; column++)
            {
                var piece = Board.GetPiece(column, row);
                if (piece.Player != PiecePlayer.None)
                {
                    if (direction != 0)
                    {
                        var inAnimation = graphics.Animations.Any(a =>
                            (a.From.Column == column && a.From.Row == row) ||
                            (a.To.Column == column && a.To.Row == row));
                        if (inAnimation)
                        {
                            // This piece is part of animation so we must not draw it to the board.
                            continue;
                        }
                    }

                    graphics.Pieces.Add(new ChessBoardGraphics()
                    {
                        Row = row,
                        Column = column,
                        Data = piece.Rank.ToString().ToLower() + "_" + piece.Player.ToString().ToLower()
                    });
                }
            }
        }

        if (graphics.Animations.Any())
        {
            await JS.InvokeVoidAsync("MyChessPlay.animate", graphics);
        }
        else
        {
            await JS.InvokeVoidAsync("MyChessPlay.draw", graphics);
        }
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

        if (CurrentMoveNumber == 0)
        {
            return string.Empty;
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

        if (CurrentMoveNumber == 0)
        {
            return string.Empty;
        }

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
    public async Task<bool> CanvasOnKeyDown(string code)
    {
        Console.WriteLine($"CanvasOnKeyDown: {code}");
        if (ShowCommentDialog || ShowPromotionDialog || ShowConfirmationDialog)
        {
            return false;
        }

        switch (code)
        {
            case "Home":
                await FirstMove();
                StateHasChanged();
                return true;
            case "ArrowLeft":
            case "ArrowDown":
            case "PageDown":
                await PreviousMove();
                StateHasChanged();
                return true;
            case "ArrowRight":
            case "ArrowUp":
            case "PageUp":
                await NextMove();
                StateHasChanged();
                return true;
            case "End":
                await LastMove();
                StateHasChanged();
                return true;
            default:
                break;
        }
        return false;
    }

    [JSInvokable]
    public async Task AnimationEnded()
    {
        Console.WriteLine($"AnimationEnded");
        if (_updateAfterAnimation)
        {
            _updateAfterAnimation = false;
            await MakeMoves(Game, CurrentMoveNumber);
        }
        else
        {
            await DrawAsync();
        }
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
        if (CurrentMoveNumber < Game.Moves.Count)
        {
            Console.WriteLine("No selection since not in last move");
            return;
        }

        var column = (int)Math.Floor(mouseEventArgs.OffsetX / _pieceSize);
        var row = (int)Math.Floor(mouseEventArgs.OffsetY / _pieceSize);
        Console.WriteLine($"CanvasOnClick: {column} - {row}");

        var animate = false;
        if (PreviousAvailableMoves.Count > 0)
        {
            var selectedMove = PreviousAvailableMoves
                .FirstOrDefault(p => p.To.Row == row && p.To.Column == column);
            if (selectedMove != null)
            {
                Board.MakeMove(selectedMove);
                animate = true;
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
        await DrawAsync(direction: animate ? 1 : 0);
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

            await JS.InvokeVoidAsync("MyChessPlay.scrollToComment");
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
        if (lastMove == null)
        {
            Error = "Move required";
            return;
        }
        var move = new MyChessGameMove
        {
            Move = lastMove.ToString(),
            Comment = Comment,
            Start = _start,
            End = DateTimeOffset.UtcNow
        };

        if (IsNew)
        {
            var game = new MyChessGame
            {
                Name = GameName
            };
            game.Players.White.ID = await GetPlayerID();
            game.Players.Black.ID = FriendID;
            game.Moves.Add(move);

            try
            {
                AppState.IsLoading = true;
                var newGame = await Client.SubmitGameAsync(game);
                NavigationManager.NavigateTo($"/play/{newGame.ID}?state=WaitingForOpponent");
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                Error = "Could not create new game. Please try again later.";
                return;
            }
            finally
            {
                AppState.IsLoading = false;
            }
        }
        else
        {

            var lastPromotion = Board.LastMovePromotion;

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
        }

        ShowGameNameDialog = false;
        ShowCommentDialog = false;
        Comment = string.Empty;
        GameName = string.Empty;

        await DrawAsync();
    }

    protected async Task FirstMove()
    {
        await Cancel();
        CurrentMoveNumber = 1;
        await MakeMoves(Game, CurrentMoveNumber, direction: 1);
    }

    protected async Task PreviousMove()
    {
        await Cancel();
        var currentVisibleMoveNumber = CurrentMoveNumber;

        currentVisibleMoveNumber = Math.Max(currentVisibleMoveNumber - 1, 0);

        Console.WriteLine($"currentVisibleMoveNumber: {currentVisibleMoveNumber}");

        if (currentVisibleMoveNumber == 0 && CurrentMoveNumber == 0)
        {
            await MakeMoves(Game, currentVisibleMoveNumber, direction: 0);
        }
        else
        {
            await MakeMoves(Game, currentVisibleMoveNumber + 1, direction: -1);
            _updateAfterAnimation = true;
        }
        CurrentMoveNumber = currentVisibleMoveNumber;
    }

    protected async Task NextMove()
    {
        await Cancel();
        var currentVisibleMoveNumber = CurrentMoveNumber;
        CurrentMoveNumber = Math.Min(CurrentMoveNumber + 1, Game.Moves.Count);
        // Do not animate if already at the latest move
        await MakeMoves(Game, CurrentMoveNumber, direction: currentVisibleMoveNumber == CurrentMoveNumber ? 0 : 1);
    }

    protected async Task LastMove()
    {
        await Cancel();
        CurrentMoveNumber = Game.Moves.Count;
        await MakeMoves(Game, CurrentMoveNumber, direction: 1);
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
            Game = new();
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
