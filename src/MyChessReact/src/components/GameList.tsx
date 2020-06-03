import React from "react";
import { MyChessGame } from "../models/MyChessGame";
import { useTypedSelector } from "../reducers";
import { ProcessState } from "../actions";
import { getAppInsights } from "./TelemetryService";
import { Link, useHistory } from "react-router-dom";
import "./GameList.css";
import { Database, DatabaseFields } from "../data/Database";
import { Player } from "../models/Player";
import { BackendService } from "./BackendService";

type GameListProps = {
    title: string;
    endpoint: string;
};

export function GameList(props: GameListProps) {
    const loginState = useTypedSelector(state => state.loginState);
    const accessToken = useTypedSelector(state => state.accessToken);
    const gamesState = useTypedSelector(state => state.gamesState);
    const games = useTypedSelector(state => state.games);

    const { push } = useHistory();
    const ai = getAppInsights();

    const friends = Database.get<Player[]>(DatabaseFields.FRIEND_LIST);
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
            <div className="row">
                {games?.map(game =>
                    <Link to={{ pathname: "/play/" + game?.id }} className="GameList-link" key={game?.id}>
                        <div className="template-1">
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
                <br />
                 or
                <button onClick={addNewGame}>add new</button> game.
            </div >
        );
    }

    const refresh = () => {
        // backendService.current.getGames();
    }

    const addNewGame = () => {
        push("/friends");
    }

    if (loginState === ProcessState.Success) {

        let contents: JSX.Element;
        switch (gamesState) {
            case ProcessState.Success:
                if (games && games?.length === 0) {
                    contents = <h6>No games found. Click to <button onClick={refresh}>refresh</button> or
                                <button onClick={addNewGame}>add new</button> game.</h6>;
                }
                else {
                    contents = renderGames(games);
                }
                break;
            case ProcessState.Error:
                contents = <h6>Oh no! Couldn't retrieve games. Click to <button onClick={refresh}>refresh</button></h6>;
                break;
            default:
                contents = <h6><em>Loading...</em></h6>;
                break;
        }

        return (
            <div>
                <h4>{props.title}</h4>
                {contents}
                <BackendService endpoint={props.endpoint} accessToken={accessToken} getGames={true} />
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
