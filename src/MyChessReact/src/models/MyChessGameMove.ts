export class MyChessGameMove {
    move: string;
    comment: string;
    capture: string;
    start: string;
    end: string;
    promotion: string;

    constructor() {
        this.move = "";
        this.comment = "";
        this.capture = "";
        this.start = "";
        this.end = "";
        this.promotion = "";
    }
}
