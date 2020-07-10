import React, { useState } from "react";
import { MyChessGame } from "../models/MyChessGame";
import { useTypedSelector } from "../reducers";
import { ProcessState } from "../actions";
import { getAppInsights } from "./TelemetryService";
import { Link, useHistory } from "react-router-dom";
import "./GameList.css";
import { Database, DatabaseFields } from "../data/Database";
import { User } from "../models/User";
import { BackendService } from "./BackendService";
import { GameStateFilter } from "../models/GameStateFilter";

type GameListProps = {
    title: string;
    endpoint: string;
};

export function GameList(props: GameListProps) {
    const loginState = useTypedSelector(state => state.loginState);
    const gamesState = useTypedSelector(state => state.gamesState);
    const games = useTypedSelector(state => state.games);

    const [executeGetGames, setExecuteGetGames] = useState(1);

    const { push } = useHistory();
    const ai = getAppInsights();

    const friends = Database.get<User[]>(DatabaseFields.FRIEND_LIST);
    const meID = Database.get<string>(DatabaseFields.ME_ID);

    const getOpponent = (game: MyChessGame) => {
        if (friends) {
            let friendID = game.players.white.id;
            if (game.players.white.id === meID) {
                friendID = game.players.black.id;
            }

            const friend = friends.find(p => p.id === friendID);
            if (friend) {
                return friend.name;
            }
        }
        return "";
    }

    const renderGames = (games?: MyChessGame[]) => {
        return (
            <div>
                <div className="row">
                    {games?.map(game =>
                        <Link to={{ pathname: `/play/${game?.id}?state=${GameStateFilter.WAITING_FOR_YOU}` }} className="GameList-link" key={game?.id}>
                            <div className="gameTemplate">
                                <div className="nameTemplate">
                                    {game?.name}
                                </div>
                                <div className="commentTemplate">
                                    {(game?.moves.length > 0 ? game?.moves[game?.moves.length - 1].comment : "")}
                                </div>
                                <div className="opponentTemplate">
                                    {getOpponent(game)}
                                </div>
                            </div>
                        </Link>
                    )
                    }
                </div>
                <div className="subtitle">
                    Or <button onClick={addNewGame}>add new</button> game.
                </div>

            </div >
        );
    }

    const refresh = () => {
        ai.trackEvent({ name: "GameList-Refresh" });

        setExecuteGetGames(executeGetGames + 1);
    }

    const addNewGame = () => {
        ai.trackEvent({ name: "GameList-Add" });

        push("/friends");
    }

    if (loginState === ProcessState.Success) {

        let contents: JSX.Element;
        switch (gamesState) {
            case ProcessState.Success:
                if (games && games?.length === 0) {
                    contents = <div className="subtitle">No games found. Click to <button onClick={refresh}>refresh</button> or
                                <button onClick={addNewGame}>add new</button> game.</div>;
                }
                else {
                    contents = renderGames(games);
                }
                break;
            case ProcessState.Error:
                contents = <div className="subtitle">Oh no! Couldn't retrieve games. Click to <button onClick={refresh}>refresh</button></div>;
                break;
            default:
                contents = <div className="subtitle"><em>Loading...</em></div>;
                break;
        }

        return (
            <div>
                <div className="title">{props.title}</div>
                {contents}
                <BackendService endpoint={props.endpoint} getGames={executeGetGames} />
            </div>
        );
    }
    else {
        // Not logged in so render blank.
        return (
            <>
            </>
        );
    }
}
