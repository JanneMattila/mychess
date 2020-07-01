import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { getAppInsights } from "./TelemetryService";
import { gamesLoadingEvent, ProcessState, friendsLoadingEvent, friendUpsertEvent, settingsLoadingEvent, settingsUpsertEvent, meLoadingEvent } from "../actions";
import { DatabaseFields, Database } from "../data/Database";
import { ProblemDetail } from "../models/ProblemDetail";
import { User } from "../models/User";
import { useHistory } from "react-router-dom";
import { useTypedSelector } from "../reducers";
import { UserSettings } from "../models/UserSettings";

type BackendServiceProps = {
    endpoint: string;

    getFriends?: number;
    upsertFriend?: User;

    getGames?: number;

    getSettings?: number;
    upsertSettings?: UserSettings;

    getMe?: number;
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

        if (accessToken && props.getFriends) {
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

        if (accessToken && props.getGames) {
            if (props.getGames !== 0) {
                getGames();
            }
        }
    }, [props.getGames, ai, dispatch, endpoint, accessToken]);

    useEffect(() => {
        const upsertFriend = async (player: User) => {
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
                    const meID = Database.get<string>(DatabaseFields.ME_ID);
                    if (meID) {
                        history.push("/friends");
                    }
                    else {
                        history.push("/settings");
                    }
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

        if (accessToken && props.upsertFriend) {
            upsertFriend(props.upsertFriend);
        }
    }, [props.upsertFriend, ai, dispatch, history, endpoint, accessToken]);


    useEffect(() => {
        const getSettings = async () => {
            dispatch(settingsLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));
            const request: RequestInit = {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + "/api/users/me/settings", request);
                const data = await response.json();

                Database.set(DatabaseFields.ME_SETTINGS, data);

                dispatch(settingsLoadingEvent(ProcessState.Success, "" /* Clear error message */, data));
            } catch (error) {
                console.log(error);
                ai.trackException(error);

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve games.";
                dispatch(settingsLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (accessToken && props.getSettings) {
            if (props.getSettings !== 0) {
                getSettings();
            }
        }
    }, [props.getSettings, ai, dispatch, endpoint, accessToken]);

    useEffect(() => {
        const upsertSettings = async (playerSettings: UserSettings) => {
            dispatch(settingsUpsertEvent(ProcessState.NotStarted, "" /* Clear error message */, "" /* Clear error link*/));
            const request: RequestInit = {
                method: "POST",
                body: JSON.stringify(playerSettings),
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + "/api/users/me/settings", request);
                if (response.ok) {
                    dispatch(settingsUpsertEvent(ProcessState.Success, "" /* Clear error message */, "" /* Clear error link*/));
                } else {
                    const data = await response.json();
                    console.log(data);

                    const ex = data as ProblemDetail;
                    if (ex.title !== undefined && ex.instance !== undefined) {
                        console.log(ex);
                        dispatch(settingsUpsertEvent(ProcessState.Error, ex.title, ex.instance));
                    }
                }
            } catch (error) {
                ai.trackException(error);

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to update settings.";
                dispatch(settingsUpsertEvent(ProcessState.Error, errorMessage, ""));

                console.log(error);
                console.log(errorMessage);
            }
        }

        if (accessToken && props.upsertSettings) {
            upsertSettings(props.upsertSettings);
        }
    }, [props.upsertSettings, ai, dispatch, history, endpoint, accessToken]);

    useEffect(() => {
        const getMe = async () => {
            dispatch(meLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));
            const request: RequestInit = {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + "/api/users/me", request);
                const data = await response.json() as User;
                console.log(data);

                Database.set(DatabaseFields.ME_ID, data.id);

                dispatch(meLoadingEvent(ProcessState.Success, "" /* Clear error message */, data.id));
            } catch (error) {
                ai.trackException(error);
                console.log(error);

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve settings.";
                console.log(errorMessage);
                dispatch(meLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (accessToken && props.getMe) {
            if (props.getMe !== 0) {
                getMe();
            }
        }
    }, [props.getMe, ai, dispatch, history, endpoint, accessToken]);

    return (
        <>
        </>
    );
}
