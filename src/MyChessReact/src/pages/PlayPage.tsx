import React, { useEffect, MouseEvent } from "react";
import "./PlayPage.css";
import { ChessBoardView } from "../game/ChessBoardView";
import { useTypedSelector } from "../reducers";
import { Database, DatabaseFields } from "../data/Database";

type PlayProps = {
    endpoint: string;
};

export function PlayPage(props: PlayProps) {
    const accessToken = useTypedSelector(state => state.accessToken);
    const meID = Database.get<string>(DatabaseFields.ME_ID);

    let board = new ChessBoardView();
    let isOpen = false;
    let isEllipse = false;

    useEffect(() => {
        board.load(props.endpoint, accessToken, meID);
    });

    const closeModal = () => {
        isOpen = false;
    }

    const confirmMove = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        board.confirmMove();
    }

    const confirmPromotion = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        board.changePromotion("Promotion");
        board.confirmMove();
    }

    const confirmComment = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        board.confirmComment();
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
        <div className="Play-container">
            <div id="status"></div>
            <table id="table-game"><tbody><tr><td>Loading...</td></tr></tbody></table>
            <div id="confirmation" className="Play-Form">
                <button onClick={confirmMove}><span role="img" aria-label="OK">✅</span> Confirm</button>
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
            <div id="commentDialog" className="Play-Form">
                <div id="gameNameDialog">
                    Game name:<br />
                    <input id="gameName" type="text" name="gameName" title="Game name" placeholder="Name of the game" />
                    <br />
                </div>
                    Comment:<br />
                <label>
                    <textarea id="comment" name="comment" title="Comment" placeholder="Add your comment here" rows={4} cols={50} />
                </label><br />
                <button onClick={confirmComment}><span role="img" aria-label="OK">✅</span> Confirm</button>
                <button onClick={cancel}><span role="img" aria-label="Cancel">❌</span> Cancel</button>
            </div>
            <div id="LastComment"></div>
            <div id="ellipse">
                <button onClick={toggleEllipse}><span role="img" aria-label="Ellipse">&nbsp; &hellip; &nbsp;</span></button>
            </div>
            <div id="ellipseContent" style={hidden}>
                <button onClick={cancel}><span role="img" aria-label="Resign">🛑</span> Resign game</button>
                <div id="ThinkTime"></div>
            </div>
        </div >
    );
}
