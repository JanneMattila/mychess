using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace MyChess
{
    public class ChessBoard
    {
        public static readonly int BOARD_SIZE = 8;
        private ChessBoardPiece[,] _pieces = new ChessBoardPiece[BOARD_SIZE, BOARD_SIZE];

        private ChessMove? _previousMove = null;

        public PiecePlayer CurrentPlayer { get; internal set; }

        private readonly ILogger<ChessBoard> _log;
        private readonly Stack<List<ChessMove>> _moves = new Stack<List<ChessMove>>();
        private readonly Stack<List<ChessBoardChange>> _boardChanges = new Stack<List<ChessBoardChange>>();

        public ChessMove? LastMoveCapture
        {
            get
            {
                if (_moves.Count > 0)
                {
                    return _moves.Peek().Where(m => m.SpecialMove == ChessSpecialMove.Capture).FirstOrDefault();
                }

                return null;
            }
        }

        public ChessMove? LastMovePromotion
        {
            get
            {
                if (_moves.Count > 0)
                {
                    return _moves.Peek().Where(m => m.SpecialMove == ChessSpecialMove.PromotionIn).FirstOrDefault();
                }

                return null;
            }
        }

        public ChessMove? LastMove
        {
            get
            {
                if (_moves.Count > 0)
                {
                    return _moves.Peek().FirstOrDefault();
                }

                return null;
            }
        }

        public ChessBoardChange[] LastBoardChanges
        {
            get
            {
                if (_boardChanges.Count > 0)
                {
                    return _boardChanges.Peek().ToArray();
                }

                return new ChessBoardChange[0];
            }
        }

        public ChessBoardChange[] PreviousBoardChanges
        {
            get
            {
                if (_boardChanges.Count > 1)
                {
                    return _boardChanges.Skip(1).First().ToArray();
                }

                return new ChessBoardChange[0];
            }
        }

        public ChessBoard(ILogger<ChessBoard> log)
        {
            _log = log;
        }

        public ChessBoardState GetBoardState()
        {
            ChessBoardState state = ChessBoardState.Normal;
            GetBoardThreats(
                out var ownKingUnderThreat,
                CurrentPlayer == PiecePlayer.White ? PiecePlayer.Black : PiecePlayer.White);
            int movesAvailable = GetAllAvailableMoves().Count();

            if (ownKingUnderThreat == true)
            {
                if (movesAvailable == 0)
                {
                    state = ChessBoardState.CheckMate;
                }
                else
                {
                    state = ChessBoardState.Check;
                }
            }
            else if (movesAvailable == 0)
            {
                state = ChessBoardState.StaleMate;
            }

            return state;
        }

        internal void SetBoard(string board)
        {
            CleanUp();

            string[] rows = board.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < rows.Length; i++)
            {
                for (int j = 0; j < rows[i].Length; j++)
                {
                    char piece = rows[i][j];
                    PiecePlayer pieceColor = char.ToLower(piece) == piece ? PiecePlayer.Black : PiecePlayer.White;
                    PieceRank pieceRank;

                    switch (char.ToLower(piece))
                    {
                        case '-':
                            pieceColor = PiecePlayer.None;
                            pieceRank = PieceRank.None;
                            break;

                        case 'k':
                            pieceRank = PieceRank.King;
                            break;

                        case 'q':
                            pieceRank = PieceRank.Queen;
                            break;

                        case 'b':
                            pieceRank = PieceRank.Bishop;
                            break;

                        case 'r':
                            pieceRank = PieceRank.Rook;
                            break;

                        case 'p':
                            pieceRank = PieceRank.Pawn;
                            break;

                        case 'n':
                            pieceRank = PieceRank.Knight;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("board");
                    }

                    _pieces[j, i] = new ChessBoardPiece(pieceColor, pieceRank);
                }
            }
        }

        public void Initialize()
        {
            CleanUp();

            _pieces[0, 0] = new ChessBoardPiece(PiecePlayer.Black, PieceRank.Rook);
            _pieces[1, 0] = new ChessBoardPiece(PiecePlayer.Black, PieceRank.Knight);
            _pieces[2, 0] = new ChessBoardPiece(PiecePlayer.Black, PieceRank.Bishop);
            _pieces[3, 0] = new ChessBoardPiece(PiecePlayer.Black, PieceRank.Queen);
            _pieces[4, 0] = new ChessBoardPiece(PiecePlayer.Black, PieceRank.King);
            _pieces[5, 0] = new ChessBoardPiece(PiecePlayer.Black, PieceRank.Bishop);
            _pieces[6, 0] = new ChessBoardPiece(PiecePlayer.Black, PieceRank.Knight);
            _pieces[7, 0] = new ChessBoardPiece(PiecePlayer.Black, PieceRank.Rook);
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                _pieces[i, 1] = new ChessBoardPiece(PiecePlayer.Black, PieceRank.Pawn);
            }

            _pieces[0, 7] = new ChessBoardPiece(PiecePlayer.White, PieceRank.Rook);
            _pieces[1, 7] = new ChessBoardPiece(PiecePlayer.White, PieceRank.Knight);
            _pieces[2, 7] = new ChessBoardPiece(PiecePlayer.White, PieceRank.Bishop);
            _pieces[3, 7] = new ChessBoardPiece(PiecePlayer.White, PieceRank.Queen);
            _pieces[4, 7] = new ChessBoardPiece(PiecePlayer.White, PieceRank.King);
            _pieces[5, 7] = new ChessBoardPiece(PiecePlayer.White, PieceRank.Bishop);
            _pieces[6, 7] = new ChessBoardPiece(PiecePlayer.White, PieceRank.Knight);
            _pieces[7, 7] = new ChessBoardPiece(PiecePlayer.White, PieceRank.Rook);
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                _pieces[i, 6] = new ChessBoardPiece(PiecePlayer.White, PieceRank.Pawn);
            }
        }

        private void CleanUp()
        {
            _pieces = new ChessBoardPiece[BOARD_SIZE, BOARD_SIZE];
            CurrentPlayer = PiecePlayer.White;
            _previousMove = null;
            _moves.Clear();
            _boardChanges.Clear();

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    _pieces[i, j] = ChessBoardPiece.Empty;
                }
            }
        }

        public ChessBoardPiece GetPiece(int column, int row)
        {
            return _pieces[column, row];
        }

        internal ChessMove[] GetBoardThreats(out bool ownKingUnderThreat)
        {
            return GetBoardThreats(out ownKingUnderThreat, CurrentPlayer);
        }

        internal ChessMove[] GetBoardThreats(out bool ownKingUnderThreat, PiecePlayer playerToEvaluate)
        {
            ChessBoardLocation kingLocation = new ChessBoardLocation(-1, -1);
            List<ChessMove> opponentMoves = new List<ChessMove>();
            var player = playerToEvaluate == PiecePlayer.White ? PiecePlayer.Black : PiecePlayer.White;
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    if (_pieces[i, j].Player == playerToEvaluate)
                    {
                        var moves = GetAvailableMoves(playerToEvaluate, i, j, false);
                        opponentMoves.AddRange(moves);
                    }
                    else if (_pieces[i, j].Player == player && _pieces[i, j].Rank == PieceRank.King)
                    {
                        kingLocation = new ChessBoardLocation(i, j);
                    }
                }
            }

            ownKingUnderThreat = opponentMoves.Where(o =>
                o.To.HorizontalLocation == kingLocation.HorizontalLocation &&
                o.To.VerticalLocation == kingLocation.VerticalLocation).Any();

            if (ownKingUnderThreat == true)
            {
                // Currently board has been verified as "check".
                // Now let's verify that is it also "checkmate".
            }

            return opponentMoves.ToArray();
        }

        public ChessMove[] GetAllAvailableMoves()
        {
            List<ChessMove> moves = new List<ChessMove>();
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    if (_pieces[i, j].Player == CurrentPlayer)
                    {
                        moves.AddRange(GetAvailableMoves(CurrentPlayer, i, j, true));
                    }
                }
            }

            return moves.ToArray();
        }

        public ChessMove[] GetAvailableMoves(int column, int row)
        {
            return GetAvailableMoves(CurrentPlayer, column, row, true);
        }

        private ChessMove[] GetAvailableMoves(PiecePlayer player, int column, int row, bool validateCheck)
        {
            List<ChessMove> moves = new List<ChessMove>();

            if (player == PiecePlayer.None)
            {
                // Game over already
                return moves.ToArray();
            }

            if (_pieces[column, row].Player == player)
            {
                switch (_pieces[column, row].Rank)
                {
                    case PieceRank.Pawn:
                        GetPawnMoves(player, moves, column, row);
                        break;

                    case PieceRank.Knight:
                        GetKnightMove(player, moves, column, row);
                        break;

                    case PieceRank.Bishop:
                        GetBishopMove(player, moves, column, row);
                        break;

                    case PieceRank.Rook:
                        GetRookMove(player, moves, column, row);
                        break;

                    case PieceRank.Queen:
                        GetQueenMove(player, moves, column, row);
                        break;

                    case PieceRank.King:
                        GetKingMove(player, moves, column, row, validateCheck);
                        break;
                    default:
                        break;
                }

                // Validate all moves
                for (int i = moves.Count - 1; i >= 0; i--)
                {
                    bool invalidMove = false;
                    ChessMove move = moves[i];

                    if (move.To.HorizontalLocation < 0 || move.To.HorizontalLocation > BOARD_SIZE - 1)
                    {
                        // Out of bounds
                        invalidMove = true;
                    }
                    else if (move.To.VerticalLocation < 0 || move.To.VerticalLocation > BOARD_SIZE - 1)
                    {
                        // Out of bounds
                        invalidMove = true;
                    }
                    else if (_pieces[move.To.HorizontalLocation, move.To.VerticalLocation].Player == player)
                    {
                        // Already occupied by team mate
                        invalidMove = true;
                    }

                    if (invalidMove == false && validateCheck == true)
                    {
                        // Let's see if this move would cause check
                        MakeMove(move, false);
                        GetBoardThreats(out var ownKingUnderThreat);
                        if (ownKingUnderThreat == true)
                        {
                            invalidMove = true;
                        }

                        Undo();
                    }

                    if (invalidMove == true)
                    {
                        moves.RemoveAt(i);
                    }
                }
            }

            return moves.ToArray();
        }

        private void GetKingMove(PiecePlayer player, List<ChessMove> moves, int column, int row, bool validateCheck)
        {
            moves.Add(new ChessMove(PieceRank.King, player, column, row, column - 1, row - 1));
            moves.Add(new ChessMove(PieceRank.King, player, column, row, column, row - 1));
            moves.Add(new ChessMove(PieceRank.King, player, column, row, column - 1, row));

            moves.Add(new ChessMove(PieceRank.King, player, column, row, column + 1, row + 1));
            moves.Add(new ChessMove(PieceRank.King, player, column, row, column, row + 1));
            moves.Add(new ChessMove(PieceRank.King, player, column, row, column + 1, row));

            moves.Add(new ChessMove(PieceRank.King, player, column, row, column + 1, row - 1));
            moves.Add(new ChessMove(PieceRank.King, player, column, row, column - 1, row + 1));

            // Check castling moves (rules from Wikipedia: http://en.wikipedia.org/wiki/Chess)
            // - Neither of the pieces involved in castling may have been previously
            //   moved during the game.
            // - There must be no pieces between the king and the rook.
            // - The king may not be in check, nor may the king pass through squares
            //   that are under attack by enemy pieces, nor move to a square where it is in check.
            int kingStartColumn = 4;
            int kingStartRow = player == PiecePlayer.White ? 7 : 0;

            if (column == kingStartColumn && row == kingStartRow)
            {
                // King is at start location. Has it moved earlier?
                var kingsEarlierMove = _moves.Where(
                    m => m.First().Player == player && m.First().Rank == PieceRank.King).FirstOrDefault();
                if (kingsEarlierMove != null)
                {
                    // King has already moved so castling is not anymore available
                    return;
                }

                // Left hand side rook
                GetKingCastlingMove(player, moves, column, row, 0, validateCheck);

                // Right hand side rook
                GetKingCastlingMove(player, moves, column, row, 7, validateCheck);
            }
        }

        private void GetKingCastlingMove(PiecePlayer player, List<ChessMove> moves, int column, int row, int rookColumn, bool validateCheck)
        {
            var rookPosition = _pieces[rookColumn, row];
            if (rookPosition.Player != player || rookPosition.Rank != PieceRank.Rook)
            {
                // Not current players rook
                return;
            }

            var rookRow = player == PiecePlayer.White ? 1 : 8;

            // Rook is at start location. Has it moved earlier?
            var rooksEarlierMove = _moves.Where(
                m => m.First().Player == player && m.First().Rank == PieceRank.Rook &&
                     m.First().From.VerticalLocation == rookRow &&
                     m.First().From.HorizontalLocation == rookColumn).FirstOrDefault();

            if (rooksEarlierMove != null)
            {
                // Rook has already moved so castling is not anymore available
                return;
            }

            bool ownKingUnderThreat = false;
            ChessMove[] opponentMoves = new ChessMove[0];
            if (validateCheck == true)
            {
                opponentMoves = GetBoardThreats(
                    out ownKingUnderThreat,
                    CurrentPlayer == PiecePlayer.White ? PiecePlayer.Black : PiecePlayer.White);
            }

            if (ownKingUnderThreat == false)
            {
                // Set defaults to left hand side rook castling
                int delta = -2;
                int startColumn = 2;
                int endColumn = column;

                if (rookColumn > endColumn)
                {
                    delta = 2;
                    startColumn = column + 1;
                    endColumn = rookColumn;
                }

                for (int currentColumn = startColumn; currentColumn < endColumn; currentColumn++)
                {
                    if (_pieces[currentColumn, row].Player != PiecePlayer.None)
                    {
                        return;
                    }

                    ChessMove opponentMove = opponentMoves.Where(
                        o => o.To.HorizontalLocation == currentColumn && o.To.VerticalLocation == row).FirstOrDefault();
                    if (opponentMove != null)
                    {
                        return;
                    }
                }

                // This castling move is valid
                moves.Add(new ChessMove(PieceRank.King, player, column, row, column + delta, row, ChessSpecialMove.Castling));
            }
        }

        private bool IsOccupiedByOpponent(PiecePlayer player, int column, int row)
        {
            if (column < 0 || column > BOARD_SIZE - 1 || row < 0 || row > BOARD_SIZE - 1)
            {
                // Out of bounds
                return false;
            }

            return _pieces[column, row].Player != player && _pieces[column, row].Player != PiecePlayer.None;
        }

        private bool IsEmpty(int column, int row)
        {
            if (column < 0 || column > BOARD_SIZE - 1 || row < 0 || row > BOARD_SIZE - 1)
            {
                // Out of bounds
                return false;
            }

            return _pieces[column, row].Rank == PieceRank.None;
        }

        private void GetPawnMoves(PiecePlayer player, List<ChessMove> moves, int column, int row)
        {
            ChessSpecialMove specialMove = ChessSpecialMove.None;
            if (player == PiecePlayer.White)
            {
                if (row == 1)
                {
                    specialMove = ChessSpecialMove.Promotion;
                }

                if (IsOccupiedByOpponent(player, column, row - 1) == false)
                {
                    moves.Add(new ChessMove(PieceRank.Pawn, player, column, row, column, row - 1, specialMove));
                }

                if (row == 6 && IsEmpty(column, row - 1) == true && IsEmpty(column, row - 2) == true)
                {
                    moves.Add(new ChessMove(PieceRank.Pawn, player, column, row, column, row - 2));
                }

                // Move only available for pawn if there is opponent piece:
                if (IsOccupiedByOpponent(player, column + 1, row - 1) == true)
                {
                    moves.Add(new ChessMove(PieceRank.Pawn, player, column, row, column + 1, row - 1, specialMove));
                }

                if (IsOccupiedByOpponent(player, column - 1, row - 1) == true)
                {
                    moves.Add(new ChessMove(PieceRank.Pawn, player, column, row, column - 1, row - 1, specialMove));
                }

                GetPawnMovesEnPassant(player, moves, column, row, -1, -1);
                GetPawnMovesEnPassant(player, moves, column, row, 1, -1);
            }
            else
            {
                if (row == 6)
                {
                    specialMove = ChessSpecialMove.Promotion;
                }

                if (IsOccupiedByOpponent(player, column, row + 1) == false)
                {
                    moves.Add(new ChessMove(PieceRank.Pawn, player, column, row, column, row + 1, specialMove));
                }

                if (row == 1 && IsEmpty(column, row + 1) == true && IsEmpty(column, row + 2) == true)
                {
                    moves.Add(new ChessMove(PieceRank.Pawn, player, column, row, column, row + 2));
                }

                // Move only available for pawn if there is opponent piece:
                if (IsOccupiedByOpponent(player, column + 1, row + 1) == true)
                {
                    moves.Add(new ChessMove(PieceRank.Pawn, player, column, row, column + 1, row + 1, specialMove));
                }

                if (IsOccupiedByOpponent(player, column - 1, row + 1) == true)
                {
                    moves.Add(new ChessMove(PieceRank.Pawn, player, column, row, column - 1, row + 1, specialMove));
                }

                GetPawnMovesEnPassant(player, moves, column, row, -1, 1);
                GetPawnMovesEnPassant(player, moves, column, row, 1, 1);
            }
        }

        private void GetPawnMovesEnPassant(PiecePlayer player, List<ChessMove> moves, int column, int row, int columnDelta, int rowDelta)
        {
            // En passant move:
            if (_previousMove != null && IsOccupiedByOpponent(player, column + columnDelta, row) == true)
            {
                ChessBoardPiece piece = GetPiece(column + columnDelta, row);
                if (piece.Rank == PieceRank.Pawn && piece.Player != player)
                {
                    if (_previousMove.To.HorizontalLocation == column + columnDelta && _previousMove.To.VerticalLocation == row && Math.Abs(_previousMove.To.VerticalLocation - _previousMove.From.VerticalLocation) == 2)
                    {
                        moves.Add(new ChessMove(PieceRank.Pawn, player, column, row, column + columnDelta, row + rowDelta, ChessSpecialMove.EnPassant));
                    }
                }
            }
        }

        private void GetKnightMove(PiecePlayer player, List<ChessMove> moves, int column, int row)
        {
            moves.Add(new ChessMove(PieceRank.Knight, player, column, row, column - 1, row + 2));
            moves.Add(new ChessMove(PieceRank.Knight, player, column, row, column - 1, row - 2));

            moves.Add(new ChessMove(PieceRank.Knight, player, column, row, column + 1, row + 2));
            moves.Add(new ChessMove(PieceRank.Knight, player, column, row, column + 1, row - 2));

            moves.Add(new ChessMove(PieceRank.Knight, player, column, row, column - 2, row + 1));
            moves.Add(new ChessMove(PieceRank.Knight, player, column, row, column - 2, row - 1));

            moves.Add(new ChessMove(PieceRank.Knight, player, column, row, column + 2, row + 1));
            moves.Add(new ChessMove(PieceRank.Knight, player, column, row, column + 2, row - 1));
        }

        private void GetRookMove(PiecePlayer player, List<ChessMove> moves, int column, int row)
        {
            GetHorizontalAndVerticalMoves(PieceRank.Rook, player, moves, column, row);
        }

        private void GetBishopMove(PiecePlayer player, List<ChessMove> moves, int column, int row)
        {
            GetDiagonalMoves(PieceRank.Bishop, player, moves, column, row);
        }

        private void GetQueenMove(PiecePlayer player, List<ChessMove> moves, int column, int row)
        {
            GetHorizontalAndVerticalMoves(PieceRank.Queen, player, moves, column, row);
            GetDiagonalMoves(PieceRank.Queen, player, moves, column, row);
        }

        private void GetDiagonalMoves(PieceRank rank, PiecePlayer player, List<ChessMove> moves, int column, int row)
        {
            // To north-east
            for (int i = 1; i <= BOARD_SIZE; i++)
            {
                if (IsOccupiedByOpponent(player, column + i, row - i) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, column + i, row - i));
                }
                else if (IsEmpty(column + i, row - i) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, column + i, row - i));
                    continue;
                }

                break;
            }

            // To south-east
            for (int i = 1; i <= BOARD_SIZE; i++)
            {
                if (IsOccupiedByOpponent(player, column + i, row + i) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, column + i, row + i));
                }
                else if (IsEmpty(column + i, row + i) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, column + i, row + i));
                    continue;
                }

                break;
            }

            // To north-west
            for (int i = 1; i <= BOARD_SIZE; i++)
            {
                if (IsOccupiedByOpponent(player, column - i, row - i) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, column - i, row - i));
                }
                else if (IsEmpty(column - i, row - i) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, column - i, row - i));
                    continue;
                }

                break;
            }

            // To south-west
            for (int i = 1; i <= BOARD_SIZE; i++)
            {
                if (IsOccupiedByOpponent(player, column - i, row + i) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, column - i, row + i));
                }
                else if (IsEmpty(column - i, row + i) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, column - i, row + i));
                    continue;
                }

                break;
            }
        }

        private void GetHorizontalAndVerticalMoves(PieceRank rank, PiecePlayer player, List<ChessMove> moves, int column, int row)
        {
            // To left
            for (int i = column - 1; i >= 0; i--)
            {
                if (IsOccupiedByOpponent(player, i, row) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, i, row));
                }
                else if (IsEmpty(i, row) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, i, row));
                    continue;
                }

                break;
            }

            // To right
            for (int i = column + 1; i < BOARD_SIZE; i++)
            {
                if (IsOccupiedByOpponent(player, i, row) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, i, row));
                }
                else if (IsEmpty(i, row) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, i, row));
                    continue;
                }

                break;
            }

            // To up
            for (int i = row - 1; i >= 0; i--)
            {
                if (IsOccupiedByOpponent(player, column, i) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, column, i));
                }
                else if (IsEmpty(column, i) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, column, i));
                    continue;
                }

                break;
            }

            // To down
            for (int i = row + 1; i < BOARD_SIZE; i++)
            {
                if (IsOccupiedByOpponent(player, column, i) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, column, i));
                }
                else if (IsEmpty(column, i) == true)
                {
                    moves.Add(new ChessMove(rank, player, column, row, column, i));
                    continue;
                }

                break;
            }
        }

        public List<ChessMove> MakeMove(ChessMove move)
        {
            return MakeMove(move, true);
        }

        private List<ChessMove> MakeMove(ChessMove move, bool validateCheck)
        {
            var executedMoves = new List<ChessMove>();
            var boardChanges = new List<ChessBoardChange>();
            var availableMoves = GetAvailableMoves(CurrentPlayer, move.From.HorizontalLocation, move.From.VerticalLocation, validateCheck);

            ChessMove selectedMove = availableMoves.Where(o => o.CompareTo(move) == 0).FirstOrDefault();

            if (selectedMove == null)
            {
                throw new ArgumentOutOfRangeException("move", "Invalid move!");
            }

            executedMoves.Add(selectedMove);

            switch (selectedMove.SpecialMove)
            {
                case ChessSpecialMove.EnPassant:
                    var enPassantPiece = _pieces[selectedMove.To.HorizontalLocation, selectedMove.From.VerticalLocation];
                    executedMoves.Add(
                        new ChessMove(
                            enPassantPiece.Rank, enPassantPiece.Player,
                            selectedMove.To.HorizontalLocation, selectedMove.From.VerticalLocation,
                            ChessBoardLocation.OUTSIDE_BOARD, ChessBoardLocation.OUTSIDE_BOARD, ChessSpecialMove.Capture));

                    _pieces[selectedMove.To.HorizontalLocation, selectedMove.From.VerticalLocation] = ChessBoardPiece.Empty;

                    // Capture in board change (in en passant case):
                    boardChanges.Add(new ChessBoardChange(selectedMove.To.HorizontalLocation, selectedMove.From.VerticalLocation, PieceSelection.Capture));
                    break;

                case ChessSpecialMove.Castling:
                    int rookStartColumn;
                    int rookEndColumn;
                    if (selectedMove.From.HorizontalLocation < selectedMove.To.HorizontalLocation)
                    {
                        // Right hand side castling
                        rookStartColumn = 7;
                        rookEndColumn = selectedMove.To.HorizontalLocation - 1;
                    }
                    else
                    {
                        // Left hand side castling
                        rookStartColumn = 0;
                        rookEndColumn = selectedMove.To.HorizontalLocation + 1;
                    }

                    var rookPiece = _pieces[rookStartColumn, selectedMove.From.VerticalLocation];
                    executedMoves.Add(
                        new ChessMove(
                            rookPiece.Rank, rookPiece.Player,
                            rookStartColumn, selectedMove.From.VerticalLocation,
                            rookEndColumn, selectedMove.To.VerticalLocation, ChessSpecialMove.Castling));

                    _pieces[rookStartColumn, selectedMove.From.VerticalLocation] = ChessBoardPiece.Empty;
                    _pieces[rookEndColumn, selectedMove.From.VerticalLocation] = new ChessBoardPiece(rookPiece.Player, rookPiece.Rank);
                    break;

                case ChessSpecialMove.Promotion:
                    executedMoves.Add(
                        new ChessMove(
                            selectedMove.Rank, selectedMove.Player,
                            selectedMove.From.HorizontalLocation, selectedMove.From.VerticalLocation,
                            ChessBoardLocation.OUTSIDE_BOARD, ChessBoardLocation.OUTSIDE_BOARD, ChessSpecialMove.PromotionOut));

                    executedMoves.Add(
                        new ChessMove(
                            PieceRank.Queen, selectedMove.Player,
                            ChessBoardLocation.OUTSIDE_BOARD, ChessBoardLocation.OUTSIDE_BOARD,
                            selectedMove.To.HorizontalLocation, selectedMove.To.VerticalLocation, ChessSpecialMove.PromotionIn));

                    // Use default promotion to queen:
                    ChessBoardPiece piece = _pieces[selectedMove.From.HorizontalLocation, selectedMove.From.VerticalLocation];
                    piece = new ChessBoardPiece(piece.Player, PieceRank.Queen);
                    _pieces[selectedMove.From.HorizontalLocation, selectedMove.From.VerticalLocation] = piece;
                    break;

                case ChessSpecialMove.Check:
                    break;

                case ChessSpecialMove.CheckMate:
                    break;

                case ChessSpecialMove.None:
                    break;
            }

            if (_pieces[selectedMove.To.HorizontalLocation, selectedMove.To.VerticalLocation].Rank != PieceRank.None)
            {
                // Capture so let's store it
                var capturePiece = _pieces[selectedMove.To.HorizontalLocation, selectedMove.To.VerticalLocation];
                executedMoves.Add(
                    new ChessMove(
                        capturePiece.Rank, capturePiece.Player,
                        selectedMove.To.HorizontalLocation, selectedMove.To.VerticalLocation,
                        ChessBoardLocation.OUTSIDE_BOARD, ChessBoardLocation.OUTSIDE_BOARD, ChessSpecialMove.Capture));

                // Capture in board change:
                boardChanges.Add(new ChessBoardChange(selectedMove.To.HorizontalLocation, selectedMove.To.VerticalLocation, PieceSelection.Capture));
            }
            else
            {
                // Move to as board change:
                boardChanges.Add(new ChessBoardChange(selectedMove.To.HorizontalLocation, selectedMove.To.VerticalLocation, PieceSelection.PreviousMoveTo));
            }

            _pieces[selectedMove.To.HorizontalLocation, selectedMove.To.VerticalLocation] = _pieces[selectedMove.From.HorizontalLocation, selectedMove.From.VerticalLocation];
            _pieces[selectedMove.From.HorizontalLocation, selectedMove.From.VerticalLocation] = ChessBoardPiece.Empty;

            // Move from as board change:
            boardChanges.Add(new ChessBoardChange(selectedMove.From.HorizontalLocation, selectedMove.From.VerticalLocation, PieceSelection.PreviousMoveFrom));

            _previousMove = selectedMove;
            CurrentPlayer = CurrentPlayer == PiecePlayer.White ? PiecePlayer.Black : PiecePlayer.White;

            _moves.Push(executedMoves);
            _boardChanges.Push(boardChanges);

            return executedMoves;
        }

        public List<ChessMove> Undo()
        {
            if (_moves.Count > 0)
            {
                var undoMoves = _moves.Pop();
                foreach (var undoMove in undoMoves)
                {
                    if (undoMove.From.CompareTo(ChessBoardLocation.OutsideBoard) != 0)
                    {
                        _pieces[undoMove.From.HorizontalLocation, undoMove.From.VerticalLocation] = new ChessBoardPiece(undoMove.Player, undoMove.Rank);
                    }

                    if (undoMove.To.CompareTo(ChessBoardLocation.OutsideBoard) != 0)
                    {
                        _pieces[undoMove.To.HorizontalLocation, undoMove.To.VerticalLocation] = ChessBoardPiece.Empty;
                    }
                }

                CurrentPlayer = CurrentPlayer == PiecePlayer.White ? PiecePlayer.Black : PiecePlayer.White;
                if (_moves.Count > 0)
                {
                    _previousMove = _moves.Peek().First();
                }
                else
                {
                    _previousMove = null;
                }

                // Also undo the board changes:
                _boardChanges.Pop();
            }

            if (_moves.Count > 0)
            {
                return _moves.Peek();
            }

            return new List<ChessMove>();
        }

        public override string ToString()
        {
            string content = Environment.NewLine;
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    string rank = "-";
                    if (_pieces[j, i].Player == PiecePlayer.White)
                    {
                        rank = _pieces[j, i].Rank.ToString().ToUpper().Substring(0, 1);
                    }
                    else if (_pieces[j, i].Player == PiecePlayer.Black)
                    {
                        rank = _pieces[j, i].Rank.ToString().ToLower().Substring(0, 1);
                    }

                    content += rank;
                }

                content += Environment.NewLine;
            }

            return content;
        }

        public List<ChessMove> MakeMove(string move)
        {
            int horizontalFrom = move[0] - 'A';
            int verticalFrom = '8' - move[1];
            int horizontalTo = move[2] - 'A';
            int verticalTo = '8' - move[3];

            return MakeMove(move, horizontalFrom, verticalFrom, horizontalTo, verticalTo);
        }

        internal List<ChessMove> MakeMove(string moveText, int horizontalFrom, int verticalFrom, int horizontalTo, int verticalTo)
        {
            var movesAvailable = GetAvailableMoves(horizontalFrom, verticalFrom);
            var move = movesAvailable
                .Where(m => m.To.HorizontalLocation == horizontalTo && m.To.VerticalLocation == verticalTo)
                .FirstOrDefault();
            if (move == null)
            {
                var availableMovesText = string.Join(", ", movesAvailable.Select(m => m.ToString()));
                var errorText = $"Cannot make move \"{moveText}\" since it's not available. Available moves are: \"{availableMovesText}\".";
                Trace.TraceError(errorText);
                
                throw new ApplicationException(errorText);
            }
            return MakeMove(move);
        }

        public void Load(string[] moves)
        {
            Initialize();
            foreach (var move in moves)
            {
                MakeMove(move);
            }
        }

        public void ChangePromotion(PieceRank promotionRank)
        {
            if (promotionRank == PieceRank.King || promotionRank == PieceRank.None || promotionRank == PieceRank.Pawn)
            {
                // Not valid promotion.
                return;
            }

            if (_moves.Count > 0)
            {
                List<ChessMove> lastMoves = _moves.Peek();
                for (int i = 0; i < lastMoves.Count; i++)
                {
                    ChessMove promotionMove = lastMoves[i];
                    if (promotionMove.SpecialMove == ChessSpecialMove.PromotionIn)
                    {
                        lastMoves[i] = new ChessMove(promotionRank, promotionMove.Player,
                            promotionMove.From.HorizontalLocation, promotionMove.From.VerticalLocation,
                            promotionMove.To.HorizontalLocation, promotionMove.To.VerticalLocation,
                            promotionMove.SpecialMove);

                        _pieces[promotionMove.To.HorizontalLocation, promotionMove.To.VerticalLocation] =
                            new ChessBoardPiece(promotionMove.Player, promotionRank);
                        return;
                    }
                }
            }
        }
    }
}
