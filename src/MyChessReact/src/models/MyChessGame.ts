import { MyChessGameMove } from "./MyChessGameMove";
import { MyChessGamePlayers } from "./MyChessGamePlayers";

export class MyChessGame {
    id: string;
    name: string;
    state: string;
    stateText: string;
    updated: Date;
    moves: MyChessGameMove[];
    players: MyChessGamePlayers;

    constructor() {
        this.id = "";
        this.name = "";
        this.state = "";
        this.stateText = "";
        this.updated = new Date();
        this.moves = [];
        this.players = new MyChessGamePlayers();
    }
}
