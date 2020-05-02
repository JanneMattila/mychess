import React, { useEffect } from "react";
import "./Play.css";
import { ChessBoardView } from "../game/ChessBoardView";
import ReactModal from "react-modal";

export function Play() {

    let board = new ChessBoardView();
    let isOpen = false;

    useEffect(() => {
        board.load("/new", "");
    });

    const closeModal = () => {
        isOpen = false;
    }

    return (
        <div>
            <header className="Play-header">
                <h4>Render board here</h4>
                <table className="table" id="table-game"></table>
                <ReactModal isOpen={isOpen} contentLabel="Promotion">
                    <button onClick={closeModal}>Undo</button>
                </ReactModal>
            </header>
        </div>
    );
}
