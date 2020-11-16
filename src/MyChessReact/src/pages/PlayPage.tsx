import React from "react";
import { ChessBoardView } from "../game/ChessBoardView";
import "./PlayPage.css";

export function PlayPage() {
    return (
        <div className="Play-container">
            <ChessBoardView />
        </div >
    );
}
