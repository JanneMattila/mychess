export class ChessBoardLocation {
    static OUTSIDE_BOARD = -1;

    horizontalLocation: number;

    verticalLocation: number;

    constructor(horizontalLocation: number, verticalLocation: number) {
        this.horizontalLocation = horizontalLocation;
        this.verticalLocation = verticalLocation;
    }

    public compareTo(otherLocation: ChessBoardLocation): number {
        if (this.horizontalLocation === otherLocation.horizontalLocation &&
            this.verticalLocation === otherLocation.verticalLocation) {
            return 0;
        }

        return -1;
    }

    public static outsideBoard(): ChessBoardLocation {
        return new ChessBoardLocation(ChessBoardLocation.OUTSIDE_BOARD, ChessBoardLocation.OUTSIDE_BOARD);
    }
}
