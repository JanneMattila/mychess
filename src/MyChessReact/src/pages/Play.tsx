import React, { useEffect } from "react";
import "./Play.css";
import { ChessBoardView } from "../game/ChessBoardView";

export function Play() {

    let board = new ChessBoardView();

    useEffect(() => {
        board.load("/new", "");
    });

    return (
        <div>
            <header className="Play-header">
                <h4>Render board here</h4>
                <table className="table" id="table-game"></table>
            </header>
        </div>
    );
}