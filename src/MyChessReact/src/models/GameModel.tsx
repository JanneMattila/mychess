import { MoveModel } from "./MoveModel";

export interface GameModel {
    id: string;
    name: string;
    opponent: string;
    updated: Date;
    moves: MoveModel[];
}
