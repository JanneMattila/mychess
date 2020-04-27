import { MoveModel } from "./MoveModel";

export class GameModel {
    id: string;
    name: string;
    opponent: string;
    updated: Date;
    moves: MoveModel[];

    constructor() {
        this.id = "";
        this.name = "";
        this.opponent = "";
        this.updated = new Date();
        this.moves = [];
    }
}
