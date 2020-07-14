import React, { useEffect, useCallback, useState } from "react";
import { useDispatch } from "react-redux";
import { getAppInsights } from "./TelemetryService";
import { gamesLoadingEvent, ProcessState, friendsLoadingEvent, friendUpsertEvent, settingsLoadingEvent, settingsUpsertEvent, loginEvent, settingsLoadingRequestedEvent, friendsRequestedEvent } from "../actions";
import { DatabaseFields, Database } from "../data/Database";
import { ProblemDetail } from "../models/ProblemDetail";
import { User } from "../models/User";
import { useHistory, useLocation } from "react-router-dom";
import { useTypedSelector } from "../reducers";
import { UserSettings } from "../models/UserSettings";
import { GameStateFilter } from "../models/GameStateFilter";
import { Configuration, UserAgentApplication } from "msal";

type BackendServiceProps = {
    clientId: string;
    applicationIdURI: string;
    endpoint: string;
};

let userAgentApplication: UserAgentApplication;

export function BackendService(props: BackendServiceProps) {

    const loginRequested = useTypedSelector(state => state.loginRequested);
    const logoutRequested = useTypedSelector(state => state.logoutRequested);
    const settingsLoadingRequested = useTypedSelector(state => state.settingsLoadingRequested);
    const settingsUpsertRequested = useTypedSelector(state => state.settingsUpsertRequested);
    const friendsRequested = useTypedSelector(state => state.friendsRequested);
    const friendsUpsertRequested = useTypedSelector(state => state.friendsUpsertRequested);
    const gamesRequested = useTypedSelector(state => state.gamesRequested);

    const [loginProcessed, setLoginProcessed] = useState(0);
    const [logoutProcessed, setLogoutProcessed] = useState(0);
    const [settingsLoadingProcessed, setSettingsLoadingProcessed] = useState(0);
    const [settingsUpsertProcessed, setSettingsUpsertProcessed] = useState<UserSettings | undefined>(undefined);
    const [friendsProcessed, setFriendsProcessed] = useState(0);
    const [friendsUpsertProcessed, setFriendsUpsertProcessed] = useState<User | undefined>(undefined);
    const [gamesProcessed, setGamesProcessed] = useState(0);

    const dispatch = useDispatch();
    const location = useLocation();
    const history = useHistory();
    const ai = getAppInsights();

    const endpoint = props.endpoint;

    const accessTokenRequest = {
        scopes: [
            props.applicationIdURI + "/User.ReadWrite",
            props.applicationIdURI + "/Games.ReadWrite"
        ]
    };

    const config: Configuration = {
        auth: {
            clientId: props.clientId,
            authority: "https://login.microsoftonline.com/common",
            navigateToLoginRequestUrl: false,
            redirectUri: window.location.origin,
            postLogoutRedirectUri: window.location.origin
        },
        cache: {
            cacheLocation: "localStorage",
            storeAuthStateInCookie: true
        },
        system: {
            navigateFrameWait: 0
        }
    };

    const preAuthEvent = useCallback(() => {
        ai.trackEvent({
            name: "Auth-PreEvent", properties: {
                pathname: location.pathname,
            }
        });

        if (location.pathname !== "/") {
            Database.set(DatabaseFields.AUTH_REDIRECT, location.pathname);
        }
    }, [ai, location.pathname]);

    const postAuthEvent = useCallback(() => {
        const redirectUrl = Database.get<string>(DatabaseFields.AUTH_REDIRECT);

        ai.trackEvent({
            name: "Auth-PostEvent", properties: {
                pathname: redirectUrl,
            }
        });

        Database.delete(DatabaseFields.AUTH_REDIRECT);
        if (redirectUrl) {
            history.push(redirectUrl);
        }
    }, [ai, history]);

    const authEvent = useCallback((accessToken: string) => {
        const loggedInAccount = userAgentApplication.getAccount();
        if (loggedInAccount) {
            dispatch(loginEvent(ProcessState.Success, "" /* Clear error message */, loggedInAccount, accessToken));
        }
        postAuthEvent();
    }, [dispatch, postAuthEvent]);

    const acquireTokenSilent = useCallback(async () => {
        try {
            const accessTokenResponse = await userAgentApplication.acquireTokenSilent(accessTokenRequest);

            // Acquire token silent success
            ai.trackEvent({
                name: "Auth-AcquireTokenSilent", properties: {
                    success: true
                }
            });

            authEvent(accessTokenResponse.accessToken);
            return accessTokenResponse.accessToken;
        }
        catch (error) {
            ai.trackEvent({
                name: "Auth-AcquireTokenSilent", properties: {
                    success: false
                }
            });
            return undefined;
        }
    }, [accessTokenRequest, ai, authEvent]);

    const acquireTokenSilentOnly = useCallback(async () => {
        try {
            const accessTokenResponse = await userAgentApplication.acquireTokenSilent(accessTokenRequest);

            // Acquire token silent success
            ai.trackEvent({
                name: "Auth-AcquireTokenSilentOnly", properties: {
                    success: true
                }
            });

            return accessTokenResponse.accessToken;
        }
        catch (error) {
            ai.trackEvent({
                name: "Auth-AcquireTokenSilentOnly", properties: {
                    success: false
                }
            });
            return undefined;
        }
    }, [accessTokenRequest, ai]);

    useEffect(() => {
        if (!userAgentApplication) {
            userAgentApplication = new UserAgentApplication(config);
            userAgentApplication.handleRedirectCallback((error, response) => {
                if (error) {
                    console.log("Auth error");
                    console.log(error);
                    const errorMessage = error.errorMessage ? error.errorMessage : "Unable to acquire access token.";
                    dispatch(loginEvent(ProcessState.Error, errorMessage));
                }
                else if (response) {
                    // Acquire token after login
                    authEvent(response.accessToken);
                }
            });

            acquireTokenSilent();
            setInterval(() => {
                ai.trackEvent({ name: "Auth-BackgroundUpdate" });

                acquireTokenSilent();
            }, 1000 * 60 * 45);
        }
    });

    useEffect(() => {
        if (loginRequested && loginRequested >= loginProcessed) {
            setLoginProcessed(loginRequested);
            ai.trackEvent({ name: "Auth-SignIn" });

            preAuthEvent();
            userAgentApplication.loginRedirect(accessTokenRequest);
        }
    }, [loginRequested, ai, preAuthEvent, accessTokenRequest, loginProcessed]);

    useEffect(() => {
        if (logoutRequested && logoutRequested >= logoutProcessed) {
            setLogoutProcessed(logoutRequested);
            ai.trackEvent({ name: "Auth-SignOut" });

            Database.clear();
            userAgentApplication.logout();
        }
    }, [logoutRequested, ai, logoutProcessed]);


    useEffect(() => {
        const getSettings = async () => {
            dispatch(settingsLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));

            const accessToken = await acquireTokenSilentOnly();

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
                ai.trackException({ exception: error });

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve games.";
                dispatch(settingsLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (settingsLoadingRequested && settingsLoadingRequested > settingsLoadingProcessed) {
            setSettingsLoadingProcessed(settingsLoadingRequested);
            ai.trackEvent({ name: "Settings-Load" });
            getSettings();

            const friends = Database.get<UserSettings>(DatabaseFields.FRIEND_LIST);
            if (!friends) {
                // No friends available. Force fetching that information.
                ai.trackEvent({ name: "Settings-FriendsMissing" });
                dispatch(friendsRequestedEvent());
            }
        }
    }, [settingsLoadingRequested, ai, dispatch, endpoint, acquireTokenSilentOnly, settingsLoadingProcessed]);

    useEffect(() => {
        const getFriends = async () => {
            dispatch(friendsLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));

            const accessToken = await acquireTokenSilentOnly();

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
                console.log(error);
                ai.trackException({ exception: error });

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve friends.";
                dispatch(friendsLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (friendsRequested && friendsRequested > friendsProcessed) {
            setFriendsProcessed(friendsRequested);
            ai.trackEvent({ name: "Friends-Load" });
            getFriends();
        }
    }, [friendsRequested, ai, dispatch, endpoint, acquireTokenSilentOnly, friendsProcessed]);

    useEffect(() => {
        const getGames = async () => {
            dispatch(gamesLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));

            const accessToken = await acquireTokenSilentOnly();

            const request: RequestInit = {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + "/api/games?state=" + GameStateFilter.WAITING_FOR_YOU, request);
                const data = await response.json();

                dispatch(gamesLoadingEvent(ProcessState.Success, "" /* Clear error message */, data));
            } catch (error) {
                console.log(error);
                ai.trackException({ exception: error });

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve games.";
                dispatch(gamesLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (gamesRequested && gamesRequested > gamesProcessed) {
            setGamesProcessed(gamesRequested);
            ai.trackEvent({ name: "Games-Load" });
            getGames();

            const userSettings = Database.get<UserSettings>(DatabaseFields.ME_SETTINGS);
            if (!userSettings) {
                // No user settings available. Force fetching that information.
                ai.trackEvent({ name: "Games-SettingsMissing" });
                dispatch(settingsLoadingRequestedEvent());
            }
        }
    }, [gamesRequested, ai, dispatch, endpoint, acquireTokenSilentOnly, gamesProcessed]);

    useEffect(() => {
        const upsertFriend = async (player: User) => {
            dispatch(friendUpsertEvent(ProcessState.NotStarted, "" /* Clear error message */, "" /* Clear error link*/));

            const accessToken = await acquireTokenSilentOnly();

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
                    const userSettings = Database.get<UserSettings>(DatabaseFields.ME_SETTINGS);
                    if (userSettings) {
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
                console.log(error);
                ai.trackException({ exception: error });

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to modify friend.";
                dispatch(friendUpsertEvent(ProcessState.Error, errorMessage, ""));

                console.log(errorMessage);
            }
        }

        if (friendsUpsertRequested && friendsUpsertRequested !== friendsUpsertProcessed) {
            setFriendsUpsertProcessed(friendsUpsertRequested);
            ai.trackEvent({ name: "Friends-Upsert" });
            upsertFriend(friendsUpsertRequested);
        }
    }, [friendsUpsertRequested, ai, dispatch, history, endpoint, acquireTokenSilentOnly, friendsUpsertProcessed]);

    useEffect(() => {
        const upsertSettings = async (playerSettings: UserSettings) => {
            dispatch(settingsUpsertEvent(ProcessState.NotStarted, "" /* Clear error message */, "" /* Clear error link*/));

            const accessToken = await acquireTokenSilentOnly();

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
                    history.push("/");
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
                console.log(error);
                ai.trackException({ exception: error });

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to update settings.";
                dispatch(settingsUpsertEvent(ProcessState.Error, errorMessage, ""));

                console.log(error);
                console.log(errorMessage);
            }
        }

        if (settingsUpsertRequested && settingsUpsertRequested !== settingsUpsertProcessed) {
            setSettingsUpsertProcessed(settingsUpsertRequested);
            ai.trackEvent({ name: "Settings-Upsert" });
            upsertSettings(settingsUpsertRequested);
        }
    }, [settingsUpsertRequested, ai, dispatch, history, endpoint, acquireTokenSilentOnly, settingsUpsertProcessed]);

    return (
        <>
        </>
    );
}
