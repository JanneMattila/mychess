import { MoveModel } from "./MoveModel";

export interface GameModel {
    id: string;
    name: string;
    opponent: string;
    comment: string;
    updated: Date;
    time: string;
    moves: MoveModel[];
}
