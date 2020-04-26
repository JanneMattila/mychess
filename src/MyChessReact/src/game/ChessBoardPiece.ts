import { ChessPiece } from "./ChessPiece";
import { ChessPlayer } from "./ChessPlayer";

export class ChessBoardPiece {
    player: ChessPlayer;
    piece: ChessPiece;

    constructor(player: ChessPlayer, piece: ChessPiece) {
        this.player = player;
        this.piece = piece;
    }

    public static empty(): ChessBoardPiece {
        return new ChessBoardPiece(ChessPlayer.None, ChessPiece.None);
    }
}
