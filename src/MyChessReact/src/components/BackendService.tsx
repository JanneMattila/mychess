import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { getAppInsights } from "./TelemetryService";
import { gamesLoadingEvent, ProcessState, friendsLoadingEvent, friendUpsertEvent } from "../actions";
import { DatabaseFields, Database } from "../data/Database";
import { ProblemDetail } from "../models/ProblemDetail";
import { Player } from "../models/Player";
import { useHistory } from "react-router-dom";
import { useTypedSelector } from "../reducers";

type BackendServiceProps = {
    endpoint: string;

    getFriends?: number;
    getGames?: number;
    upsertFriend?: Player;
};

export function BackendService(props: BackendServiceProps) {

    const accessToken = useTypedSelector(state => state.accessToken);
    const dispatch = useDispatch();
    const history = useHistory();
    const ai = getAppInsights();

    const endpoint = props.endpoint;

    useEffect(() => {
        const getFriends = async () => {
            dispatch(friendsLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));
            const request: RequestInit = {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + "/api/users/me/friends", request);
                const data = await response.json();

                Database.set(DatabaseFields.FRIEND_LIST, data);

                dispatch(friendsLoadingEvent(ProcessState.Success, "" /* Clear error message */, data));
            } catch (error) {
                ai.trackException(error);

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve friends.";
                dispatch(friendsLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (accessToken) {
            if (props.getFriends !== 0) {
                getFriends();
            }
        }
    }, [props.getFriends, ai, dispatch, endpoint, accessToken]);

    useEffect(() => {
        const getGames = async () => {
            dispatch(gamesLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));
            const request: RequestInit = {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + "/api/games", request);
                const data = await response.json();

                dispatch(gamesLoadingEvent(ProcessState.Success, "" /* Clear error message */, data));
            } catch (error) {
                console.log(error);
                ai.trackException(error);

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve games.";
                dispatch(gamesLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (accessToken) {
            if (props.getGames !== 0) {
                getGames();
            }
        }
    }, [props.getGames, ai, dispatch, endpoint, accessToken]);


    useEffect(() => {
        const upsertFriend = async (player: Player) => {
            dispatch(friendUpsertEvent(ProcessState.NotStarted, "" /* Clear error message */, "" /* Clear error link*/));
            const request: RequestInit = {
                method: "POST",
                body: JSON.stringify(player),
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + "/api/users/me/friends", request);
                const data = await response.json();
                console.log(data);

                if (response.ok) {
                    dispatch(friendUpsertEvent(ProcessState.Success, "" /* Clear error message */, "" /* Clear error link*/));
                    history.push("/friends");
                } else {
                    const ex = data as ProblemDetail;
                    if (ex.title !== undefined && ex.instance !== undefined) {
                        console.log(ex);
                        dispatch(friendUpsertEvent(ProcessState.Error, ex.title, ex.instance));
                    }
                }
            } catch (error) {
                ai.trackException(error);

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to modify friend.";
                dispatch(friendUpsertEvent(ProcessState.Error, errorMessage, ""));

                console.log(error);
                console.log(errorMessage);
            }
        }

        if (accessToken) {
            if (props.upsertFriend) {
                upsertFriend(props.upsertFriend);
            }
        }
    }, [props.upsertFriend, ai, dispatch, history, endpoint, accessToken]);

    return (
        <>
        </>
    );
}
