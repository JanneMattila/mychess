import { User } from "./User";

export class MyChessGamePlayers {
    white: User;
    black: User;

    constructor() {
        this.white = new User();
        this.black = new User();
    }
}
