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

    const hidden = {
        display: "none"
    }

    return (
        <div>
            <header className="Play-header">
                <table className="table" id="table-game"></table>
                <div id="confirmation" style={hidden}>
                    <button onClick={board.confirm}><span role="img" aria-label="OK">✔</span> Confirm</button>
                    <button onClick={board.cancel}><span role="img" aria-label="Cancel">❌</span> Cancel</button>
                </div>
                <div id="status" style={hidden}></div>
                <ReactModal isOpen={isOpen} contentLabel="Promotion">
                    <button onClick={closeModal}>Undo</button>
                </ReactModal>
            </header>
        </div >
    );
}
