import React, { useEffect } from "react";
import "./Play.css";
import { ChessBoardLocalView } from "../game/ChessBoardLocalView";
import ReactModal from "react-modal";

export function PlayLocal() {

    let board = new ChessBoardLocalView();
    let isOpen = false;

    useEffect(() => {
        board.load();
    });

    const closeModal = () => {
        isOpen = false;
    }

    return (
        <div>
            <header className="Play-header">
                <table className="table" id="table-game"></table>
                <ReactModal isOpen={isOpen} contentLabel="Promotion">
                    <button onClick={closeModal}>Undo</button>
                </ReactModal>
            </header>
        </div>
    );
}
