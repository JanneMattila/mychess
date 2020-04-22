import { Move } from "./Move";

export interface Game {
    id: string;
    name: string;
    opponent: string;
    updated: Date;
    time: string;
    moves: Move[];
}
