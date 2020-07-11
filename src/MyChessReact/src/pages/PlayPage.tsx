import React, { useEffect, MouseEvent } from "react";
import "./PlayPage.css";
import { ChessBoardView } from "../game/ChessBoardView";
import { useTypedSelector } from "../reducers";
import { Database, DatabaseFields } from "../data/Database";
import logo from "../pages/logo.svg";

type PlayProps = {
    endpoint: string;
};

export function PlayPage(props: PlayProps) {
    const accessToken = useTypedSelector(state => state.accessToken);
    const meID = Database.get<string>(DatabaseFields.ME_ID);

    const board = new ChessBoardView();
    let isEllipse = false;

    useEffect(() => {
        board.addEventHandlers();
        board.load(props.endpoint, accessToken, meID);
        return () => {
            board.removeEventHandlers();
        }
    }, [props.endpoint, accessToken, meID, board]);

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

    const firstMove = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        board.firstMove();
    }

    const previousMove = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        board.previousMove();
    }

    const nextMove = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        board.nextMove();
    }

    const lastMove = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();
        board.lastMove();
    }

    const resignGame = (event: MouseEvent<HTMLButtonElement>) => {
        event.preventDefault();

        if (window.confirm("Do you really want to resign the current game?")) {
            board.resignGame();
            toggleEllipse(event);
        }
    }

    const hidden = {
        display: "none",
    }

    return (
        <div className="Play-container">
            <div id="status"></div>
            <div id="error" className="Play-Error"></div>
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
                    <input id="gameName" type="text" name="gameName" title="Game name" placeholder="Name your game here" className="Play-GameName" />
                    <br />
                </div>
                    Comment:<br />
                <label>
                    <textarea id="comment" name="comment" title="Comment" placeholder="Add your comment here" rows={3} cols={50} />
                </label><br />
                <button onClick={confirmComment}><span role="img" aria-label="OK">✅</span> Confirm</button>
                <button onClick={cancel}><span role="img" aria-label="Cancel">❌</span> Cancel</button>
            </div>
            <div id="Loading" className="Play-Spinner">
                <img src={logo} className="Play-logo" alt="logo" />
                Submitting Your move...
                </div>
            <div id="LastComment"></div>
            <div id="ellipse">
                <button onClick={toggleEllipse}><span role="img" aria-label="Ellipse">&nbsp; &hellip; &nbsp;</span></button>
            </div>
            <div id="ellipseContent" style={hidden}>
                <button onClick={firstMove}><span role="img" aria-label="Move to first move">&nbsp; &#9664; &#9664; &nbsp;</span></button>
                <button onClick={previousMove}><span role="img" aria-label="Move to previous move">&nbsp; &#9664; &nbsp;</span></button>
                <button onClick={nextMove}><span role="img" aria-label="Move to next move">&nbsp; &#9654; &nbsp;</span></button>
                <button onClick={lastMove}><span role="img" aria-label="Move to last move">&nbsp; &#9654; &#9654; &nbsp;</span></button>

                <div id="ThinkTime"></div>

                <br />
                <hr />
                <br />

                <button onClick={resignGame}><span role="img" aria-label="Resign">🛑</span> Resign game</button>
            </div>
        </div >
    );
}
