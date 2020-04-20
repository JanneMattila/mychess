import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { Game } from "../models/Game";
import { useTypedSelector } from "../reducers";
import { gamesLoadingEvent, RootState, RootAction } from "../actions";

type GameListProps = {
    filter: string;
};

type GameListState = {
    games: Game[];
    loading: boolean;
};

export function GameList() {
    const selectorLoggedIn = (state: RootState) => state.loggedIn;
    const selectorAccessToken = (state: RootState) => state.accessToken;
    const selectorGamesLoaded = (state: RootState) => state.gamesLoaded;
    const selectorGames = (state: RootState) => state.games;

    const loggedIn = useTypedSelector(selectorLoggedIn);
    const accessToken = useTypedSelector(selectorAccessToken);
    const gamesLoaded = useTypedSelector(selectorGamesLoaded);
    const games = useTypedSelector(selectorGames);

    const dispatch = useDispatch();

    useEffect(() => {
        if (loggedIn && !gamesLoaded) {
            populateGames();
        }
    });

    const populateGames = async () => {
        const request: RequestInit = {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Authorization": "Bearer " + accessToken
            }
        };

        try {
            const response = await fetch("games", request);
            const data = await response.json();

            dispatch(gamesLoadingEvent(true, "" /* Clear error message */, data));
        } catch (error) {
            const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve games.";
            dispatch(gamesLoadingEvent(false, errorMessage));
        }
    }

    const renderGames = (games: Game[]) => {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {games.map(game =>
                        <tr key={game.id}>
                            <td>{game.title}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    if (loggedIn) {
        let contents = gamesLoaded
            ? <p><em>Loading...</em></p>
            : renderGames(games ? games : []);

        return (
            <div>
                <h1>Games</h1>
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
