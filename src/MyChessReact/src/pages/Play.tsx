import React, { useEffect, MouseEvent } from "react";
import "./Play.css";
import { ChessBoardView } from "../game/ChessBoardView";
import ReactModal from "react-modal";

export function Play() {

    let board = new ChessBoardView();
    let isOpen = false;
    let isEllipse = false;

    useEffect(() => {
        // Determine is this is
        // - local game
        // - new game *WITH* friendID in url
        // - existing game
        board.load("", "");
    });

    const closeModal = () => {
        isOpen = false;
    }

    const confirm = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        board.confirm();
    }

    const confirmPromotion = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        board.changePromotion("Promotion");
        board.confirm();
    }

    const cancel = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        board.cancel();
    }

    const toggleEllipse = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        isEllipse = !isEllipse;
        const element = document.getElementById("ellipseContent");
        if (element) {
            element.style.display = isEllipse ? "inline" : "none";
        }
    }

    const hidden = {
        display: "none",
    }

    return (
        <div>
            <header className="Play-header">
                <table className="table" id="table-game"></table>
                <div id="confirmation" className="Play-Form">
                    <button onClick={confirm}><span role="img" aria-label="OK">✅</span> Confirm</button>
                    <button onClick={cancel}><span role="img" aria-label="Cancel">❌</span> Cancel</button>
                </div>
                <div id="promotionDialog" className="Play-Form">
                    Promote pawn to:<br />
                    <label>
                        <input id="promotionRadioQueen" type="radio" name="Promotion" value="Queen" title="Queen" defaultChecked={true} />
                        Queen
                    </label><br />
                    <label>
                        <input id="promotionRadioKnight" type="radio" name="Promotion" value="Knight" title="Knight" />
                        Knight
                    </label><br />
                    <label>
                        <input id="promotionRadioRook" type="radio" name="Promotion" value="Rook" title="Rook" />
                        Rook
                    </label><br />
                    <label>
                        <input id="promotionRadioBishop" type="radio" name="Promotion" value="Bishop" title="Bishop" />
                        Bishop
                    </label><br />
                    <button onClick={confirmPromotion}><span role="img" aria-label="OK">✅</span> Confirm</button>
                    <button onClick={cancel}><span role="img" aria-label="Cancel">❌</span> Cancel</button>
                </div>
                <div id="status" style={hidden}></div>
                <div id="ellipse">
                    <button onClick={toggleEllipse}><span role="img" aria-label="Ellipse">&nbsp; &hellip; &nbsp;</span></button>
                </div>
                <div id="ellipseContent" style={hidden}>
                    No content yet
                </div>
                <ReactModal isOpen={isOpen} contentLabel="Promotion">
                    <button onClick={closeModal}>Undo</button>
                </ReactModal>
            </header>
        </div >
    );
}
