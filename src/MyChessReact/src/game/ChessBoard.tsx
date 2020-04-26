import { ChessBoardChange } from "./ChessBoardChange";
import { ChessMove } from "./ChessMove";
import { ChessSpecialMove } from "./ChessSpecialMove";
import { ChessPiece } from "./ChessPiece";
import { ChessBoardLocation } from "./ChessBoardLocation";
import { ChessBoardState } from "./ChessBoardState";
import { ChessPlayer } from "./ChessPlayer";
import { ChessBoardPiece } from "./ChessBoardPiece";
import { ChessPieceSelection } from "./ChessPieceSelection";

export class ChessBoard {
    public static BOARD_SIZE: number = 8;

    public currentPlayer = ChessPlayer.White;

    private pieces: ChessBoardPiece[][] = [];
    private previousMove?: ChessMove | null = null;
    private moves = new Array();
    private boardChanges = new Array();

    constructor() {
        this.initialize();
    }

    public lastMoveCapture(): ChessMove | null {
        if (this.moves.length > 0) {
            let lastIndex: number = this.moves.length - 1
            for (let i: number = 0; i < this.moves[lastIndex].length; i++) {
                if (this.moves[lastIndex][i].specialMove == ChessSpecialMove.Capture) {
                    return this.moves[lastIndex][i];
                }
            }
        }

        return null;
    }

    public lastMovePromotion(): ChessMove | null {
        if (this.moves.length > 0) {
            let lastIndex: number = this.moves.length - 1
            for (let i: number = 0; i < this.moves[lastIndex].length; i++) {
                if (this.moves[lastIndex][i].specialMove == ChessSpecialMove.PromotionIn) {
                    return this.moves[lastIndex][i];
                }
            }
        }

        return null;
    }

    public lastMove(): ChessMove | null {
        if (this.moves.length > 0) {
            return this.moves[this.moves.length - 1][0];
        }

        return null;
    }

    public lastBoardChanges(): ChessMove[] {
        if (this.boardChanges.length > 0) {
            return this.boardChanges[this.boardChanges.length - 1];
        }

        return new Array<ChessMove>();
    }

    public previousBoardChanges(): ChessMove[] {
        if (this.boardChanges.length > 1) {
            return this.boardChanges[this.boardChanges.length - 2];
        }

        return new Array<ChessMove>();
    }

    public totalMoves(): number {
        return this.moves.length;
    }

    public initialize() {
        this.cleanUp();

        this.pieces[0][0] = new ChessBoardPiece(ChessPlayer.Black, ChessPiece.Rook);
        this.pieces[1][0] = new ChessBoardPiece(ChessPlayer.Black, ChessPiece.Knight);
        this.pieces[2][0] = new ChessBoardPiece(ChessPlayer.Black, ChessPiece.Bishop);
        this.pieces[3][0] = new ChessBoardPiece(ChessPlayer.Black, ChessPiece.Queen);
        this.pieces[4][0] = new ChessBoardPiece(ChessPlayer.Black, ChessPiece.King);
        this.pieces[5][0] = new ChessBoardPiece(ChessPlayer.Black, ChessPiece.Bishop);
        this.pieces[6][0] = new ChessBoardPiece(ChessPlayer.Black, ChessPiece.Knight);
        this.pieces[7][0] = new ChessBoardPiece(ChessPlayer.Black, ChessPiece.Rook);

        for (let i = 0; i < ChessBoard.BOARD_SIZE; i++) {
            this.pieces[i][1] = new ChessBoardPiece(ChessPlayer.Black, ChessPiece.Pawn);
        }

        this.pieces[0][7] = new ChessBoardPiece(ChessPlayer.White, ChessPiece.Rook);
        this.pieces[1][7] = new ChessBoardPiece(ChessPlayer.White, ChessPiece.Knight);
        this.pieces[2][7] = new ChessBoardPiece(ChessPlayer.White, ChessPiece.Bishop);
        this.pieces[3][7] = new ChessBoardPiece(ChessPlayer.White, ChessPiece.Queen);
        this.pieces[4][7] = new ChessBoardPiece(ChessPlayer.White, ChessPiece.King);
        this.pieces[5][7] = new ChessBoardPiece(ChessPlayer.White, ChessPiece.Bishop);
        this.pieces[6][7] = new ChessBoardPiece(ChessPlayer.White, ChessPiece.Knight);
        this.pieces[7][7] = new ChessBoardPiece(ChessPlayer.White, ChessPiece.Rook);

        for (let i = 0; i < ChessBoard.BOARD_SIZE; i++) {
            this.pieces[i][6] = new ChessBoardPiece(ChessPlayer.White, ChessPiece.Pawn);
        }
    }

    private cleanUp() {
        this.currentPlayer = ChessPlayer.White;
        this.previousMove = null;
        this.moves = new Array();
        this.boardChanges = new Array();

        for (let i = 0; i < ChessBoard.BOARD_SIZE; i++) {
            this.pieces[i] = new Array<ChessBoardPiece>(ChessBoard.BOARD_SIZE);
            for (let j = 0; j < ChessBoard.BOARD_SIZE; j++) {
                this.pieces[i][j] = ChessBoardPiece.empty();
            }
        }
    }

    private isEmpty(column: number, row: number): boolean {
        if (column < 0 || column > ChessBoard.BOARD_SIZE - 1 || row < 0 || row > ChessBoard.BOARD_SIZE - 1) {
            // Out of bounds
            return false;
        }

        return this.pieces[column][row].piece == ChessPiece.None;
    }

    private isOccupiedByOpponent(player: ChessPlayer, column: number, row: number): boolean {
        if (column < 0 || column > ChessBoard.BOARD_SIZE - 1 || row < 0 || row > ChessBoard.BOARD_SIZE - 1) {
            // Out of bounds
            return false;
        }

        return this.pieces[column][row].player != player && this.pieces[column][row].player != ChessPlayer.None;
    }

    public getPiece(column: number, row: number): ChessBoardPiece {
        return this.pieces[column][row];
    }

    private getBoardThreats(ownKingUnderThreat: any, playerToEvaluate: ChessPlayer = this.currentPlayer): ChessMove[] {
        let kingLocation: ChessBoardLocation = new ChessBoardLocation(-1, -1);
        let opponentMoves: ChessMove[] = [];
        let player: ChessPlayer = playerToEvaluate == ChessPlayer.White ? ChessPlayer.Black : ChessPlayer.White;
        for (let i: number = 0; i < ChessBoard.BOARD_SIZE; i++) {
            for (let j: number = 0; j < ChessBoard.BOARD_SIZE; j++) {
                if (this.pieces[i][j].player == playerToEvaluate) {
                    let moves = this.getAvailableMovesInternal(playerToEvaluate, i, j, false);
                    moves.forEach(function (move) {
                        opponentMoves.push(move);
                    });
                }
                else if (this.pieces[i][j].player == player && this.pieces[i][j].piece == ChessPiece.King) {
                    kingLocation = new ChessBoardLocation(i, j);
                }
            }
        }

        for (let i: number = 0; i < opponentMoves.length; i++) {
            if (opponentMoves[i].to.horizontalLocation == kingLocation.horizontalLocation &&
                opponentMoves[i].to.verticalLocation == kingLocation.verticalLocation) {
                ownKingUnderThreat.value = true;
                break;
            }
        }

        if (ownKingUnderThreat.value == true) {
            // Currently board has been verified as "check".
            // Now let's verify that is it also "checkmate".
        }

        return opponentMoves;
    }

    public getAllAvailableMoves(): ChessMove[] {

        let availableMoves: ChessMove[] = [];
        for (let x = 0; x < ChessBoard.BOARD_SIZE; x++) {
            for (let y = 0; y < ChessBoard.BOARD_SIZE; y++) {
                if (this.pieces[x][y].player == this.currentPlayer) {
                    let moves: ChessMove[] = this.getAvailableMovesInternal(this.currentPlayer, x, y, true);

                    moves.forEach(function (move) {
                        availableMoves.push(move);
                    });
                }
            }
        }

        return availableMoves;
    }

    public getAvailableMoves(column: number, row: number): ChessMove[] {
        return this.getAvailableMovesInternal(this.currentPlayer, column, row, true);
    }

    private getAvailableMovesInternal(player: ChessPlayer, column: number, row: number, validateCheck: boolean): ChessMove[] {
        let availableMoves: ChessMove[] = [];
        let validMoves: ChessMove[] = [];

        if (player == ChessPlayer.None) {
            // Game over already
            return availableMoves;
        }

        if (this.pieces[column][row].player == player) {
            switch (this.pieces[column][row].piece) {
                case ChessPiece.Pawn:
                    this.getPawnMoves(player, availableMoves, column, row);
                    break;

                case ChessPiece.Knight:
                    this.getKnightMove(player, availableMoves, column, row);
                    break;

                case ChessPiece.Bishop:
                    this.getBishopMove(player, availableMoves, column, row);
                    break;

                case ChessPiece.Rook:
                    this.getRookMove(player, availableMoves, column, row);
                    break;

                case ChessPiece.Queen:
                    this.getQueenMove(player, availableMoves, column, row);
                    break;

                case ChessPiece.King:
                    this.getKingMove(player, availableMoves, column, row, validateCheck);
                    break;
                default:
                    break;
            }

            // Validate all moves
            for (let i = availableMoves.length - 1; i >= 0; i--) {
                let invalidMove: boolean = false;
                let move = availableMoves[i];

                if (move.to.horizontalLocation < 0 || move.to.horizontalLocation > ChessBoard.BOARD_SIZE - 1) {
                    // Out of bounds
                    invalidMove = true;
                }
                else if (move.to.verticalLocation < 0 || move.to.verticalLocation > ChessBoard.BOARD_SIZE - 1) {
                    // Out of bounds
                    invalidMove = true;
                }
                else if (this.pieces[move.to.horizontalLocation][move.to.verticalLocation].player == player) {
                    // Already occupied by team mate
                    invalidMove = true;
                }

                if (invalidMove == false && validateCheck == true) {
                    // Let's see if this move would cause check
                    let ownKingUnderThreat: any = { value: false };
                    ownKingUnderThreat.value = false;

                    this.makeMove(move, false);
                    this.getBoardThreats(ownKingUnderThreat);
                    if (ownKingUnderThreat.value == true) {
                        invalidMove = true;
                    }

                    this.undo();
                }

                if (invalidMove == false) {
                    validMoves.push(move);
                }
            }
        }

        return validMoves;
    }

    private getKingMove(player: ChessPlayer, moves: ChessMove[], column: number, row: number, validateCheck: boolean) {
        moves.push(new ChessMove(ChessPiece.King, player, column, row, column - 1, row - 1));
        moves.push(new ChessMove(ChessPiece.King, player, column, row, column, row - 1));
        moves.push(new ChessMove(ChessPiece.King, player, column, row, column - 1, row));

        moves.push(new ChessMove(ChessPiece.King, player, column, row, column + 1, row + 1));
        moves.push(new ChessMove(ChessPiece.King, player, column, row, column, row + 1));
        moves.push(new ChessMove(ChessPiece.King, player, column, row, column + 1, row));

        moves.push(new ChessMove(ChessPiece.King, player, column, row, column + 1, row - 1));
        moves.push(new ChessMove(ChessPiece.King, player, column, row, column - 1, row + 1));

        // Check castling moves (rules from Wikipedia: http://en.wikipedia.org/wiki/Chess)
        // - Neither of the pieces involved in castling may have been previously
        //   moved during the game.
        // - There must be no pieces between the king and the rook.
        // - The king may not be in check, nor may the king pass through squares
        //   that are under attack by enemy pieces, nor move to a square where it is in check.
        let kingStartColumn: number = 4;
        let kingStartRow: number = player == ChessPlayer.White ? 7 : 0;

        if (column == kingStartColumn && row == kingStartRow) {
            // King is at start location. Has it moved earlier?
            for (let i: number = 0; i < this.moves.length; i++) {
                if (this.moves[i][0].player == player && this.moves[i][0].piece == ChessPiece.King) {
                    // King has already moved so castling is not anymore available
                    return;
                }
            }

            // Left hand side rook
            this.getKingCastlingMove(player, moves, column, row, 0, validateCheck);

            // Right hand side rook
            this.getKingCastlingMove(player, moves, column, row, 7, validateCheck);
        }
    }

    private getKingCastlingMove(player: ChessPlayer, moves: ChessMove[], column: number, row: number, rookColumn: number, validateCheck: boolean) {
        let rookPosition = this.pieces[rookColumn][row];
        if (rookPosition.player != player || rookPosition.piece != ChessPiece.Rook) {
            // Not current players rook
            return;
        }

        let rookRow = player == ChessPlayer.White ? 1 : 8;

        // Rook is at start location. Has it moved earlier?
        let rooksEarlierMove: boolean = false;
        for (let i: number = 0; i < this.moves.length; i++) {
            if (this.moves[i][0].player == player && this.moves[i][0].piece == ChessPiece.King &&
                this.moves[i][0].verticalLocation == rookRow &&
                this.moves[i][0].horizontalLocation == rookColumn) {
                // Rook has already moved so castling is not anymore available
                return;
            }
        }

        let ownKingUnderThreat: any = { value: false };
        ownKingUnderThreat.value = false;

        let opponentMoves: ChessMove[] = [];
        if (validateCheck == true) {
            opponentMoves = this.getBoardThreats(
                ownKingUnderThreat,
                this.currentPlayer == ChessPlayer.White ? ChessPlayer.Black : ChessPlayer.White);
        }

        if (ownKingUnderThreat.value == false) {
            // Set defaults to left hand side rook castling
            let delta: number = -2;
            let startColumn: number = 2;
            let endColumn: number = column;

            if (rookColumn > endColumn) {
                delta = 2;
                startColumn = column + 1;
                endColumn = rookColumn;
            }

            for (let currentColumn: number = startColumn; currentColumn < endColumn; currentColumn++) {
                if (this.pieces[currentColumn][row].player != ChessPlayer.None) {
                    return;
                }

                opponentMoves.forEach(function (opponentMove) {
                    if (opponentMove.to.horizontalLocation == currentColumn && opponentMove.to.verticalLocation == row) {
                        return;
                    }
                });
            }

            // This castling move is valid
            moves.push(new ChessMove(ChessPiece.King, player, column, row, column + delta, row, ChessSpecialMove.Castling));
        }
    }

    private getPawnMoves(player: ChessPlayer, moves: ChessMove[], column: number, row: number) {
        let specialMove = ChessSpecialMove.None;
        if (player == ChessPlayer.White) {
            if (row == 1) {
                specialMove = ChessSpecialMove.Promotion;
            }

            if (this.isOccupiedByOpponent(player, column, row - 1) == false) {
                moves.push(new ChessMove(ChessPiece.Pawn, player, column, row, column, row - 1, specialMove));
            }

            if (row == 6 && this.isEmpty(column, row - 1) == true && this.isEmpty(column, row - 2) == true) {
                moves.push(new ChessMove(ChessPiece.Pawn, player, column, row, column, row - 2));
            }

            // Move only available for pawn if there is opponent piece:
            if (this.isOccupiedByOpponent(player, column + 1, row - 1) == true) {
                moves.push(new ChessMove(ChessPiece.Pawn, player, column, row, column + 1, row - 1, specialMove));
            }

            if (this.isOccupiedByOpponent(player, column - 1, row - 1) == true) {
                moves.push(new ChessMove(ChessPiece.Pawn, player, column, row, column - 1, row - 1, specialMove));
            }

            this.getPawnMovesEnPassant(player, moves, column, row, -1, -1);
            this.getPawnMovesEnPassant(player, moves, column, row, 1, -1);
        }
        else {
            if (row == 6) {
                specialMove = ChessSpecialMove.Promotion;
            }

            if (this.isOccupiedByOpponent(player, column, row + 1) == false) {
                moves.push(new ChessMove(ChessPiece.Pawn, player, column, row, column, row + 1, specialMove));
            }

            if (row == 1 && this.isEmpty(column, row + 1) == true && this.isEmpty(column, row + 2) == true) {
                moves.push(new ChessMove(ChessPiece.Pawn, player, column, row, column, row + 2));
            }

            // Move only available for pawn if there is opponent piece:
            if (this.isOccupiedByOpponent(player, column + 1, row + 1) == true) {
                moves.push(new ChessMove(ChessPiece.Pawn, player, column, row, column + 1, row + 1, specialMove));
            }

            if (this.isOccupiedByOpponent(player, column - 1, row + 1) == true) {
                moves.push(new ChessMove(ChessPiece.Pawn, player, column, row, column - 1, row + 1, specialMove));
            }

            this.getPawnMovesEnPassant(player, moves, column, row, -1, 1);
            this.getPawnMovesEnPassant(player, moves, column, row, 1, 1);
        }
    }

    private getPawnMovesEnPassant(player: ChessPlayer, moves: ChessMove[], column: number, row: number, columnDelta: number, rowDelta: number) {
        // En passant move:
        if (this.previousMove != null && this.isOccupiedByOpponent(player, column + columnDelta, row) == true) {
            let piece = this.getPiece(column + columnDelta, row);
            if (piece.piece == ChessPiece.Pawn && piece.player != player) {
                if (this.previousMove.to.horizontalLocation == column + columnDelta && this.previousMove.to.verticalLocation == row && Math.abs(this.previousMove.to.verticalLocation - this.previousMove.from.verticalLocation) == 2) {
                    moves.push(new ChessMove(ChessPiece.Pawn, player, column, row, column + columnDelta, row + rowDelta, ChessSpecialMove.EnPassant));
                }
            }
        }
    }

    private getRookMove(player: ChessPlayer, moves: ChessMove[], column: number, row: number) {
        this.getHorizontalAndVerticalMoves(ChessPiece.Rook, player, moves, column, row);
    }

    private getBishopMove(player: ChessPlayer, moves: ChessMove[], column: number, row: number) {
        this.getDiagonalMoves(ChessPiece.Bishop, player, moves, column, row);
    }

    private getQueenMove(player: ChessPlayer, moves: ChessMove[], column: number, row: number) {
        this.getHorizontalAndVerticalMoves(ChessPiece.Queen, player, moves, column, row);
        this.getDiagonalMoves(ChessPiece.Queen, player, moves, column, row);
    }

    private getKnightMove(player: ChessPlayer, moves: ChessMove[], column: number, row: number) {
        moves.push(new ChessMove(ChessPiece.Knight, player, column, row, column - 1, row + 2));
        moves.push(new ChessMove(ChessPiece.Knight, player, column, row, column - 1, row - 2));

        moves.push(new ChessMove(ChessPiece.Knight, player, column, row, column + 1, row + 2));
        moves.push(new ChessMove(ChessPiece.Knight, player, column, row, column + 1, row - 2));

        moves.push(new ChessMove(ChessPiece.Knight, player, column, row, column - 2, row + 1));
        moves.push(new ChessMove(ChessPiece.Knight, player, column, row, column - 2, row - 1));

        moves.push(new ChessMove(ChessPiece.Knight, player, column, row, column + 2, row + 1));
        moves.push(new ChessMove(ChessPiece.Knight, player, column, row, column + 2, row - 1));
    }

    private getDiagonalMoves(piece: ChessPiece, player: ChessPlayer, moves: ChessMove[], column: number, row: number) {
        // To north-east
        for (let i: number = 1; i <= ChessBoard.BOARD_SIZE; i++) {
            if (this.isOccupiedByOpponent(player, column + i, row - i) == true) {
                moves.push(new ChessMove(piece, player, column, row, column + i, row - i));
            }
            else if (this.isEmpty(column + i, row - i) == true) {
                moves.push(new ChessMove(piece, player, column, row, column + i, row - i));
                continue;
            }

            break;
        }

        // To south-east
        for (let i: number = 1; i <= ChessBoard.BOARD_SIZE; i++) {
            if (this.isOccupiedByOpponent(player, column + i, row + i) == true) {
                moves.push(new ChessMove(piece, player, column, row, column + i, row + i));
            }
            else if (this.isEmpty(column + i, row + i) == true) {
                moves.push(new ChessMove(piece, player, column, row, column + i, row + i));
                continue;
            }

            break;
        }

        // To north-west
        for (let i: number = 1; i <= ChessBoard.BOARD_SIZE; i++) {
            if (this.isOccupiedByOpponent(player, column - i, row - i) == true) {
                moves.push(new ChessMove(piece, player, column, row, column - i, row - i));
            }
            else if (this.isEmpty(column - i, row - i) == true) {
                moves.push(new ChessMove(piece, player, column, row, column - i, row - i));
                continue;
            }

            break;
        }

        // To south-west
        for (let i: number = 1; i <= ChessBoard.BOARD_SIZE; i++) {
            if (this.isOccupiedByOpponent(player, column - i, row + i) == true) {
                moves.push(new ChessMove(piece, player, column, row, column - i, row + i));
            }
            else if (this.isEmpty(column - i, row + i) == true) {
                moves.push(new ChessMove(piece, player, column, row, column - i, row + i));
                continue;
            }

            break;
        }
    }

    private getHorizontalAndVerticalMoves(piece: ChessPiece, player: ChessPlayer, moves: ChessMove[], column: number, row: number) {
        // To left
        for (let i: number = column - 1; i >= 0; i--) {
            if (this.isOccupiedByOpponent(player, i, row) == true) {
                moves.push(new ChessMove(piece, player, column, row, i, row));
            }
            else if (this.isEmpty(i, row) == true) {
                moves.push(new ChessMove(piece, player, column, row, i, row));
                continue;
            }

            break;
        }

        // To right
        for (let i: number = column + 1; i < ChessBoard.BOARD_SIZE; i++) {
            if (this.isOccupiedByOpponent(player, i, row) == true) {
                moves.push(new ChessMove(piece, player, column, row, i, row));
            }
            else if (this.isEmpty(i, row) == true) {
                moves.push(new ChessMove(piece, player, column, row, i, row));
                continue;
            }

            break;
        }

        // To up
        for (let i: number = row - 1; i >= 0; i--) {
            if (this.isOccupiedByOpponent(player, column, i) == true) {
                moves.push(new ChessMove(piece, player, column, row, column, i));
            }
            else if (this.isEmpty(column, i) == true) {
                moves.push(new ChessMove(piece, player, column, row, column, i));
                continue;
            }

            break;
        }

        // To down
        for (let i: number = row + 1; i < ChessBoard.BOARD_SIZE; i++) {
            if (this.isOccupiedByOpponent(player, column, i) == true) {
                moves.push(new ChessMove(piece, player, column, row, column, i));
            }
            else if (this.isEmpty(column, i) == true) {
                moves.push(new ChessMove(piece, player, column, row, column, i));
                continue;
            }

            break;
        }
    }

    public makeMove(move: ChessMove, validateCheck: boolean = true): ChessMove[] {
        let executedMoves: ChessMove[] = [];
        let boardChanges: ChessBoardChange[] = [];
        let availableMoves = this.getAvailableMovesInternal(this.currentPlayer, move.from.horizontalLocation, move.from.verticalLocation, validateCheck);

        let selectedMove: ChessMove | null = null;

        for (let index = 0; index < availableMoves.length; index++) {
            const element = availableMoves[index];
            if (element.compareTo(move) == 0) {
                selectedMove = move;
                break;
            }
        }

        if (selectedMove == null) {
            throw new Error("Invalid move!");
        }

        executedMoves.push(selectedMove);

        switch (selectedMove.specialMove) {
            case ChessSpecialMove.EnPassant:
                let enPassantPiece = this.pieces[selectedMove.to.horizontalLocation][selectedMove.from.verticalLocation];

                executedMoves.push(
                    new ChessMove(
                        enPassantPiece.piece, enPassantPiece.player,
                        selectedMove.to.horizontalLocation, selectedMove.from.verticalLocation,
                        ChessBoardLocation.OUTSIDE_BOARD, ChessBoardLocation.OUTSIDE_BOARD, ChessSpecialMove.Capture));

                this.pieces[selectedMove.to.horizontalLocation][selectedMove.from.verticalLocation] = ChessBoardPiece.empty();

                // Capture in board change (in en passant case):
                boardChanges.push(new ChessBoardChange(selectedMove.to.horizontalLocation, selectedMove.from.verticalLocation, ChessPieceSelection.Capture));
                break;

            case ChessSpecialMove.Castling:
                let rookStartColumn: number;
                let rookEndColumn: number;
                if (selectedMove.from.horizontalLocation < selectedMove.to.horizontalLocation) {
                    // Right hand side castling
                    rookStartColumn = 7;
                    rookEndColumn = selectedMove.to.horizontalLocation - 1;
                }
                else {
                    // Left hand side castling
                    rookStartColumn = 0;
                    rookEndColumn = selectedMove.to.horizontalLocation + 1;
                }

                let rookPiece = this.pieces[rookStartColumn][selectedMove.from.verticalLocation];
                executedMoves.push(
                    new ChessMove(
                        rookPiece.piece, rookPiece.player,
                        rookStartColumn, selectedMove.from.verticalLocation,
                        rookEndColumn, selectedMove.to.verticalLocation, ChessSpecialMove.Castling));

                this.pieces[rookStartColumn][selectedMove.from.verticalLocation] = ChessBoardPiece.empty();
                this.pieces[rookEndColumn][selectedMove.from.verticalLocation] = new ChessBoardPiece(rookPiece.player, rookPiece.piece);
                break;

            case ChessSpecialMove.Promotion:
                executedMoves.push(
                    new ChessMove(
                        selectedMove.piece, selectedMove.player,
                        selectedMove.from.horizontalLocation, selectedMove.from.verticalLocation,
                        ChessBoardLocation.OUTSIDE_BOARD, ChessBoardLocation.OUTSIDE_BOARD, ChessSpecialMove.PromotionOut));

                executedMoves.push(
                    new ChessMove(
                        ChessPiece.Queen, selectedMove.player,
                        ChessBoardLocation.OUTSIDE_BOARD, ChessBoardLocation.OUTSIDE_BOARD,
                        selectedMove.to.horizontalLocation, selectedMove.to.verticalLocation, ChessSpecialMove.PromotionIn));

                // Use default promotion to queen:
                let piece: ChessBoardPiece = this.pieces[selectedMove.from.horizontalLocation][selectedMove.from.verticalLocation];
                piece = new ChessBoardPiece(piece.player, ChessPiece.Queen);
                this.pieces[selectedMove.from.horizontalLocation][selectedMove.from.verticalLocation] = piece;
                break;

            case ChessSpecialMove.Check:
                break;

            case ChessSpecialMove.CheckMate:
                break;

            case ChessSpecialMove.None:
                break;
        }

        if (this.pieces[selectedMove.to.horizontalLocation][selectedMove.to.verticalLocation].piece != ChessPiece.None) {
            // Capture so let's store it
            let capturePiece = this.pieces[selectedMove.to.horizontalLocation][selectedMove.to.verticalLocation];
            executedMoves.push(
                new ChessMove(
                    capturePiece.piece, capturePiece.player,
                    selectedMove.to.horizontalLocation, selectedMove.to.verticalLocation,
                    ChessBoardLocation.OUTSIDE_BOARD, ChessBoardLocation.OUTSIDE_BOARD, ChessSpecialMove.Capture));

            // Capture in board change:
            boardChanges.push(new ChessBoardChange(selectedMove.to.horizontalLocation, selectedMove.to.verticalLocation, ChessPieceSelection.Capture));
        }
        else {
            // Move to as board change:
            boardChanges.push(new ChessBoardChange(selectedMove.to.horizontalLocation, selectedMove.to.verticalLocation, ChessPieceSelection.PreviousMoveTo));
        }

        this.pieces[selectedMove.to.horizontalLocation][selectedMove.to.verticalLocation] = this.pieces[selectedMove.from.horizontalLocation][selectedMove.from.verticalLocation];
        this.pieces[selectedMove.from.horizontalLocation][selectedMove.from.verticalLocation] = ChessBoardPiece.empty();

        // Move from as board change:
        boardChanges.push(new ChessBoardChange(selectedMove.from.horizontalLocation, selectedMove.from.verticalLocation, ChessPieceSelection.PreviousMoveFrom));

        this.previousMove = selectedMove;
        this.currentPlayer = this.currentPlayer == ChessPlayer.White ? ChessPlayer.Black : ChessPlayer.White;

        this.moves.push(executedMoves);
        this.boardChanges.push(boardChanges);

        return executedMoves;
    }

    public undo(): ChessMove[] {
        if (this.moves.length > 0) {
            let undoMoves: ChessMove[] = this.moves.pop();

            for (let i: number = 0; i < undoMoves.length; i++) {
                let undoMove: ChessMove = undoMoves[i];
                if (undoMove.from.compareTo(ChessBoardLocation.outsideBoard()) != 0) {
                    this.pieces[undoMove.from.horizontalLocation][undoMove.from.verticalLocation] = new ChessBoardPiece(undoMove.player, undoMove.piece);
                }

                if (undoMove.to.compareTo(ChessBoardLocation.outsideBoard()) != 0) {
                    this.pieces[undoMove.to.horizontalLocation][undoMove.to.verticalLocation] = ChessBoardPiece.empty();
                }
            }

            this.currentPlayer = this.currentPlayer == ChessPlayer.White ? ChessPlayer.Black : ChessPlayer.White;
            if (this.moves.length > 0) {
                this.previousMove = this.moves[this.moves.length - 1][0];
            }
            else {
                this.previousMove = null;
            }

            // Also undo the board changes:
            this.boardChanges.pop();
        }

        if (this.moves.length > 0) {
            return this.moves[this.moves.length - 1];
        }

        return [];
    }

    public toString(): string {
        let lineFeed: string = "\r\n";
        let board: string = "";

        for (let i = 0; i < ChessBoard.BOARD_SIZE; i++) {
            for (let j = 0; j < ChessBoard.BOARD_SIZE; j++) {

                let piece: ChessPiece = this.pieces[j][i].piece;
                let pieceString: string;

                switch (piece) {
                    case ChessPiece.Pawn:
                        pieceString = "P";
                        break;
                    case ChessPiece.King:
                        pieceString = "K";
                        break;
                    case ChessPiece.Knight:
                        pieceString = "N";
                        break;
                    case ChessPiece.Bishop:
                        pieceString = "B";
                        break;
                    case ChessPiece.Queen:
                        pieceString = "Q";
                        break;
                    case ChessPiece.Rook:
                        pieceString = "R";
                        break;
                    default:
                        pieceString = "-";
                        break;
                }

                if (this.pieces[j][i].player == ChessPlayer.Black) {
                    pieceString = pieceString.toLowerCase();
                }

                board += pieceString;
            }

            board += lineFeed;
        }
        return board;
    }

    public makeMoveFromString(move: string): ChessMove[] {
        let horizontalFrom: number = move.charCodeAt(0) - "A".charCodeAt(0);
        let verticalFrom: number = "8".charCodeAt(0) - move.charCodeAt(1);
        let horizontalTo: number = move.charCodeAt(2) - "A".charCodeAt(0);
        let verticalTo: number = "8".charCodeAt(0) - move.charCodeAt(3);

        return this.makeMoveInternal(horizontalFrom, verticalFrom, horizontalTo, verticalTo);
    }

    private makeMoveInternal(horizontalFrom: number, verticalFrom: number, horizontalTo: number, verticalTo: number): ChessMove[] {
        let moves: ChessMove[] = this.getAvailableMoves(horizontalFrom, verticalFrom);
        for (let i: number = 0; i < moves.length; i++) {
            if (moves[i].to.horizontalLocation == horizontalTo &&
                moves[i].to.verticalLocation == verticalTo) {
                return this.makeMove(moves[i]);
            }
        }

        throw new Error("Couldn't find available moves for given selection!");
    }

    public load(moves: string[]) {
        this.initialize();
        for (let i: number = 0; i < moves.length; i++) {
            this.makeMoveFromString(moves[i]);
        }
    }

    public changePromotion(promotionPiece: ChessPiece) {
        if (promotionPiece == ChessPiece.King || promotionPiece == ChessPiece.None || promotionPiece == ChessPiece.Pawn) {
            // Not valid promotion.
            return;
        }

        if (this.moves.length > 0) {
            let lastIndex: number = this.moves.length - 1
            for (let i: number = 0; i < this.moves[lastIndex].length; i++) {
                let promotionMove: ChessMove = this.moves[lastIndex][i];
                if (promotionMove.specialMove == ChessSpecialMove.PromotionIn) {
                    this.moves[lastIndex][i] = new ChessMove(promotionPiece, promotionMove.player,
                        promotionMove.from.horizontalLocation, promotionMove.from.verticalLocation,
                        promotionMove.to.horizontalLocation, promotionMove.to.verticalLocation,
                        promotionMove.specialMove);
                    this.pieces[promotionMove.to.horizontalLocation][promotionMove.to.verticalLocation] =
                        new ChessBoardPiece(promotionMove.player, promotionPiece);

                    return;
                }
            }
        }
    }

    public GetBoardState(): ChessBoardState {
        let state = ChessBoardState.Normal;
        let ownKingUnderThreat: any = { value: false };
        ownKingUnderThreat.value = false;
        this.getBoardThreats(
            ownKingUnderThreat,
            this.currentPlayer == ChessPlayer.White ? ChessPlayer.Black : ChessPlayer.White);
        let movesAvailable: number = this.getAllAvailableMoves().length;

        if (ownKingUnderThreat.value == true) {
            if (movesAvailable == 0) {
                state = ChessBoardState.CheckMate;
            }
            else {
                state = ChessBoardState.Check;
            }
        }
        else if (movesAvailable == 0) {
            state = ChessBoardState.StaleMate;
        }
        return state;
    }
}
