import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { GameModel } from "../models/GameModel";
import { useTypedSelector } from "../reducers";
import { gamesLoadingEvent, RootState } from "../actions";
import { getAppInsights } from "./TelemetryService";
import "./GameList.css";

type GameListProps = {
    endpoint: string;
};

export function GameList(props: GameListProps) {
    const selectorLoggedIn = (state: RootState) => state.loggedIn;
    const selectorAccessToken = (state: RootState) => state.accessToken;
    const selectorGamesLoaded = (state: RootState) => state.gamesLoaded;
    const selectorGames = (state: RootState) => state.games;

    const loggedIn = useTypedSelector(selectorLoggedIn);
    const accessToken = useTypedSelector(selectorAccessToken);
    const gamesLoaded = useTypedSelector(selectorGamesLoaded);
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

                dispatch(gamesLoadingEvent(true, "" /* Clear error message */, data));
            } catch (error) {
                ai.trackException(error);

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve games.";
                dispatch(gamesLoadingEvent(false, errorMessage));
            }
        }

        if (loggedIn && !gamesLoaded) {
            populateGames();
        }
    }, [loggedIn, gamesLoaded, accessToken, ai, props, dispatch]);

    const renderGames = (games?: GameModel[]) => {
        return (
            <div className="row">
                {games?.map(game =>
                    <div className="template-1" key={game?.id}>
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
                )
                }
            </div >
        );
    }

    if (loggedIn) {
        let contents = gamesLoaded
            ? renderGames(games)
            : <h6><em>Loading...</em></h6>;

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
