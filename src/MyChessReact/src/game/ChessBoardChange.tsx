import { ChessPieceSelection } from "./ChessPieceSelection";
import { ChessBoardLocation } from "./ChessBoardLocation";

export class ChessBoardChange {

    location: ChessBoardLocation;

    selection: ChessPieceSelection;

    constructor(horizontalLocation: number, verticalLocation: number, pieceSelection: ChessPieceSelection) {
        this.location = new ChessBoardLocation(horizontalLocation, verticalLocation);
        this.selection = pieceSelection;
    }
}
