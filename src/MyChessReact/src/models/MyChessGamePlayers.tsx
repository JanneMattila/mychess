import { Player } from "./Player";

export class MyChessGamePlayers {
    white: Player;
    black: Player;

    constructor() {
        this.white = new Player();
        this.black = new Player();
    }
}
