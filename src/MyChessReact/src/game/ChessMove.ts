import { ChessBoardLocation } from "./ChessBoardLocation";
import { ChessPiece } from "./ChessPiece";
import { ChessSpecialMove } from "./ChessSpecialMove";
import { ChessPlayer } from "./ChessPlayer";

export class ChessMove {
    piece: ChessPiece;
    player: ChessPlayer;

    from: ChessBoardLocation;
    to: ChessBoardLocation;

    specialMove: ChessSpecialMove;

    comment: string;

    time: string

    constructor(piece: ChessPiece, player: ChessPlayer, horizontalLocationFrom: number, verticalLocationFrom: number, horizontalLocationTo: number, verticalLocationTo: number, specialMove?: ChessSpecialMove, comment?: string, time?: string) {
        this.piece = piece;
        this.player = player;
        this.from = new ChessBoardLocation(horizontalLocationFrom, verticalLocationFrom);
        this.to = new ChessBoardLocation(horizontalLocationTo, verticalLocationTo);
        this.specialMove = specialMove != null ? specialMove : ChessSpecialMove.None;
        this.comment = comment != null ? comment : "";
        this.time = time != null ? time : "";
    }

    public compareTo(otherMove: ChessMove): number {
        let compare: number = this.from.compareTo(this.from);
        if (compare === 0) {
            compare = this.to.compareTo(otherMove.to);
            if (compare === 0) {
                if (this.player === otherMove.player) {
                    return 0;
                }
            }
        }

        return compare;
    }

    public getMoveString(): string {
        let horizontalFrom: string = String.fromCharCode(this.from.horizontalLocation + "A".charCodeAt(0));
        let verticalFrom: string = String.fromCharCode("8".charCodeAt(0) - this.from.verticalLocation);
        let horizontalTo: string = String.fromCharCode(this.to.horizontalLocation + "A".charCodeAt(0));
        let verticalTo: string = String.fromCharCode("8".charCodeAt(0) - this.to.verticalLocation);
        return horizontalFrom + verticalFrom + horizontalTo + verticalTo;
    }
}
