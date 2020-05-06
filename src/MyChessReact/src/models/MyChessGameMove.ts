export class MyChessGameMove {
    move: string;
    comment: string;
    capture: string;
    time: number;
    promotion: string;

    constructor() {
        this.move = "";
        this.comment = "";
        this.capture = "";
        this.time = 0;
        this.promotion = "";
    }
}
