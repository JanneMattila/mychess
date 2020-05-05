import { MyChessGameMove } from "./MyChessGameMove";
import { MyChessGamePlayers } from "./MyChessGamePlayers";

export class MyChessGame {
    id: string;
    name: string;
    opponent: string;
    updated: Date;
    moves: MyChessGameMove[];
    players: MyChessGamePlayers;

    constructor() {
        this.id = "";
        this.name = "";
        this.opponent = "";
        this.updated = new Date();
        this.moves = [];
        this.players = new MyChessGamePlayers();
    }
}
