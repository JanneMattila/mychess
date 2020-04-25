import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { GameModel } from "../models/GameModel";
import { useTypedSelector } from "../reducers";
import { gamesLoadingEvent, RootState, ProcessState } from "../actions";
import { getAppInsights } from "./TelemetryService";
import { Link } from "react-router-dom";
import "./GameList.css";

type GameListProps = {
    endpoint: string;
};

export function GameList(props: GameListProps) {
    const selectorLoginState = (state: RootState) => state.loginState;
    const selectorAccessToken = (state: RootState) => state.accessToken;
    const selectorGamesState = (state: RootState) => state.gamesState;
    const selectorGames = (state: RootState) => state.games;

    const loginState = useTypedSelector(selectorLoginState);
    const accessToken = useTypedSelector(selectorAccessToken);
    const gamesState = useTypedSelector(selectorGamesState);
    const games = useTypedSelector(selectorGames);

    const dispatch = useDispatch();
    const ai = getAppInsights();

    useEffect(() => {
        const populateGames = async () => {
            const request: RequestInit = {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(props.endpoint + "/api/games", request);
                const data = await response.json();

                dispatch(gamesLoadingEvent(ProcessState.Success, "" /* Clear error message */, data));
            } catch (error) {
                ai.trackException(error);

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve games.";
                dispatch(gamesLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (loginState && !gamesState) {
            populateGames();
        }
    }, [loginState, gamesState, accessToken, ai, props, dispatch]);

    const renderGames = (games?: GameModel[]) => {
        return (
            <div className="row">
                {games?.map(game =>
                    <Link to={{ pathname: "/play/" + game?.id }} className="GameList-link" key={game?.id}>
                        <div className="template-1">
                            <div className="nameTemplate">
                                {game?.name}
                            </div>
                            <div className="commentTemplate">
                                {game?.comment}
                            </div>
                            <div className="opponentTemplate">
                                {game?.opponent}
                            </div>
                        </div>
                    </Link>
                )
                }
            </div >
        );
    }

    const refresh = () => {
        dispatch(gamesLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));
    }

    if (loginState === ProcessState.Success) {

        let contents: JSX.Element;
        switch (gamesState) {
            case ProcessState.Success:
                contents = renderGames(games);
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
                <h4>Games</h4>
                {contents}
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
