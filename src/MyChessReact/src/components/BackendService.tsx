import React, { useEffect, useCallback, useState } from "react";
import { useDispatch } from "react-redux";
import { getAppInsights } from "./TelemetryService";
import { gamesLoadingEvent, ProcessState, friendsLoadingEvent, friendUpsertEvent, settingsLoadingEvent, settingsUpsertEvent, loginEvent, settingsLoadingRequestedEvent, friendsRequestedEvent, gamesCreateEvent, gamesMoveCreateEvent, gamesLoadingSingleEvent, gamesDeleteEvent } from "../actions";
import { DatabaseFields, Database } from "../data/Database";
import { ProblemDetail } from "../models/ProblemDetail";
import { User } from "../models/User";
import { useHistory, useLocation } from "react-router-dom";
import { useTypedSelector } from "../reducers";
import { UserSettings } from "../models/UserSettings";
import { GameStateFilter } from "../models/GameStateFilter";
import { Configuration, PublicClientApplication, AccountInfo, InteractionRequiredAuthError, SilentRequest, RedirectRequest } from "@azure/msal-browser";
import { MyChessGame } from "../models/MyChessGame";
import { GameQuery } from "../models/GameQuery";
import { MoveSubmit } from "../models/MoveSubmit";

type BackendServiceProps = {
    clientId: string;
    applicationIdURI: string;
    endpoint: string;
};

let publicClientApplication: PublicClientApplication;

export function BackendService(props: BackendServiceProps) {

    const loginRequested = useTypedSelector(state => state.loginRequested);
    const logoutRequested = useTypedSelector(state => state.logoutRequested);
    const settingsLoadingRequested = useTypedSelector(state => state.settingsLoadingRequested);
    const settingsUpsertRequested = useTypedSelector(state => state.settingsUpsertRequested);
    const friendsRequested = useTypedSelector(state => state.friendsRequested);
    const friendsUpsertRequested = useTypedSelector(state => state.friendsUpsertRequested);
    const gamesRequested = useTypedSelector(state => state.gamesRequested);
    const gamesSingleQuery = useTypedSelector(state => state.gamesSingleQuery);
    const gamesCreateRequested = useTypedSelector(state => state.gamesCreateRequested);
    const gamesMoveCreateRequested = useTypedSelector(state => state.gamesMoveCreateRequested);
    const gamesDeleteRequested = useTypedSelector(state => state.gamesDeleteRequested);
    const gamesFilter = useTypedSelector(state => state.gamesFilter);

    const [account, setAccount] = useState(Database.get<AccountInfo>(DatabaseFields.ACCOUNT));

    const [loginProcessed, setLoginProcessed] = useState(0);
    const [logoutProcessed, setLogoutProcessed] = useState(0);
    const [settingsLoadingProcessed, setSettingsLoadingProcessed] = useState(0);
    const [gamesSingleProcessed, setGamesSingleProcessed] = useState<GameQuery | undefined>(undefined);
    const [gamesCreateProcessed, setGamesCreateProcessed] = useState<MyChessGame | undefined>(undefined);
    const [gamesMoveCreateProcessed, setMoveGamesCreateProcessed] = useState<MoveSubmit | undefined>(undefined);
    const [gamesDeleteProcessed, setGamesDeleteProcessed] = useState<string>("");

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
            "openid",
            "profile",
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
            storeAuthStateInCookie: false
        },
        system: {

        }
    };

    const preAuthEvent = useCallback(() => {
        ai.trackEvent({
            name: "Auth-PreEvent", properties: {
                pathname: location.pathname,
            }
        });

        Database.clear();
        if (account) {
            Database.set(DatabaseFields.ACCOUNT, account);
        }

        if (location.pathname !== "/") {
            Database.set(DatabaseFields.AUTH_REDIRECT, location.pathname);
        }
    }, [ai, location.pathname, account]);

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
        const accounts = publicClientApplication.getAllAccounts();

        if (accounts) {
            // TODO: Support account switcher
            const loggedInAccount = accounts[0];
            Database.set(DatabaseFields.ACCOUNT, loggedInAccount);
            setAccount(loggedInAccount);
            dispatch(loginEvent(ProcessState.Success, "" /* Clear error message */, loggedInAccount, accessToken));
            postAuthEvent();
        }
    }, [dispatch, postAuthEvent]);

    const acquireTokenSilent = useCallback(async () => {
        if (!account) {
            return;
        }

        dispatch(loginEvent(ProcessState.Processing, "" /* No error message */));

        let interactionRequired = false;
        const accessTokenRequestSilent: SilentRequest = {
            ...accessTokenRequest,
            account: account
        };

        try {
            const accessTokenResponse = await publicClientApplication.acquireTokenSilent(accessTokenRequestSilent);

            // Acquire token silent success
            ai.trackEvent({
                name: "Auth-AcquireTokenSilent", properties: {
                    success: true
                }
            });

            authEvent(accessTokenResponse.accessToken);
        }
        catch (error) {
            console.log(JSON.stringify(error));
            const errorMessage: string = error.errorCode ? error.errorCode : error.toString();

            if (error instanceof InteractionRequiredAuthError) {
                interactionRequired = true;
                console.log("Auth-AcquireTokenSilent -> Interaction required");
            }
            else {
                ai.trackEvent({
                    name: "Auth-AcquireTokenSilent", properties: {
                        success: false
                    }
                });
                ai.trackException({ exception: error });
                dispatch(loginEvent(ProcessState.Error, errorMessage));
                return;
            }
        }

        if (interactionRequired) {
            const accessTokenRequestRedirect: RedirectRequest = {
                ...accessTokenRequest,
                loginHint: account.username
            };
            await publicClientApplication.acquireTokenRedirect(accessTokenRequestRedirect);
        }
    }, [accessTokenRequest, ai, authEvent, account, dispatch]);

    const acquireTokenSilentOnly = useCallback(async () => {
        if (!account) {
            return undefined;
        }

        const accessTokenRequestSilent: SilentRequest = {
            ...accessTokenRequest,
            account: account
        };

        try {
            const accessTokenResponse = await publicClientApplication.acquireTokenSilent(accessTokenRequestSilent);

            // Acquire token silent success
            ai.trackEvent({
                name: "Auth-AcquireTokenSilentOnly", properties: {
                    success: true
                }
            });

            return accessTokenResponse.accessToken;
        }
        catch (error) {
            console.log(JSON.stringify(error));

            if (error instanceof InteractionRequiredAuthError) {
                console.log("Auth-AcquireTokenSilentOnly -> Interaction required");
                const accessTokenRequestRedirect: RedirectRequest = {
                    ...accessTokenRequest,
                    loginHint: account.username
                };
                await publicClientApplication.acquireTokenRedirect(accessTokenRequestRedirect);
            }
            else {
                ai.trackEvent({
                    name: "Auth-AcquireTokenSilentOnly", properties: {
                        success: false
                    }
                });

                ai.trackException({ exception: error });
                return undefined;
            }
        }
    }, [accessTokenRequest, ai, account]);

    useEffect(() => {
        if (!publicClientApplication) {
            publicClientApplication = new PublicClientApplication(config);

            publicClientApplication.handleRedirectPromise().then((response) => {
                if (response) {
                    // Acquire token after login
                    authEvent(response.accessToken);
                }
            }).catch((error) => {
                console.log("Auth error");
                console.log(error);
                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to acquire access token.";
                dispatch(loginEvent(ProcessState.Error, errorMessage));
            });

            acquireTokenSilent();
        }
    });

    useEffect(() => {
        if (loginRequested && loginRequested >= loginProcessed) {
            setLoginProcessed(loginRequested);
            ai.trackEvent({ name: "Auth-SignIn" });

            const accessTokenRequestSilent = {
                ...accessTokenRequest,
                account: account
            };

            preAuthEvent();
            publicClientApplication.loginRedirect(accessTokenRequestSilent);
        }
    }, [loginRequested, ai, preAuthEvent, accessTokenRequest, loginProcessed, account]);

    useEffect(() => {
        if (logoutRequested && logoutRequested >= logoutProcessed) {
            setLogoutProcessed(logoutRequested);
            ai.trackEvent({ name: "Auth-SignOut" });

            Database.clear();
            publicClientApplication.logout();
        }
    }, [logoutRequested, ai, logoutProcessed]);


    useEffect(() => {
        const getSettings = async () => {
            dispatch(settingsLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));

            const accessToken = await acquireTokenSilentOnly();
            if (!accessToken) {
                dispatch(settingsLoadingEvent(ProcessState.Error, "Authentication missing"));
                return;
            }

            const request: RequestInit = {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + "/api/users/me/settings", request);
                if (response.ok) {
                    const data = await response.json();

                    Database.set(DatabaseFields.ME_SETTINGS, data);

                    dispatch(settingsLoadingEvent(ProcessState.Success, "" /* Clear error message */, data));

                    const friends = Database.get<UserSettings>(DatabaseFields.FRIEND_LIST);
                    if (!friends) {
                        // No friends available. Force fetching that information.
                        ai.trackEvent({ name: "Settings-FriendsMissing" });
                        dispatch(friendsRequestedEvent());
                    }
                }
                else {
                    ai.trackEvent({
                        name: "Settings-LoadFailed", properties: {
                            status: response.status,
                            statusText: response.statusText,
                        }
                    });
                    dispatch(settingsLoadingEvent(ProcessState.Error, "Unable to retrieve settings."));
                }
            }
            catch (error) {
                console.log(error);
                ai.trackException({ exception: error });

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve settings.";
                dispatch(settingsLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (settingsLoadingRequested && settingsLoadingRequested > settingsLoadingProcessed) {
            setSettingsLoadingProcessed(settingsLoadingRequested);
            ai.trackEvent({ name: "Settings-Load" });
            getSettings();
        }
    }, [settingsLoadingRequested, ai, dispatch, endpoint, acquireTokenSilentOnly, settingsLoadingProcessed]);

    useEffect(() => {
        const getFriends = async () => {
            dispatch(friendsLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));

            const accessToken = await acquireTokenSilentOnly();
            if (!accessToken) {
                dispatch(friendsLoadingEvent(ProcessState.Error, "Authentication missing"));
                return;
            }

            const request: RequestInit = {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + "/api/users/me/friends", request);
                if (response.ok) {
                    const data = await response.json();

                    Database.set(DatabaseFields.FRIEND_LIST, data);

                    dispatch(friendsLoadingEvent(ProcessState.Success, "" /* Clear error message */, data));
                }
                else {
                    ai.trackEvent({
                        name: "Friends-LoadFailed", properties: {
                            status: response.status,
                            statusText: response.statusText,
                        }
                    });
                    dispatch(friendsLoadingEvent(ProcessState.Error, "Unable to retrieve friends."));
                }
            }
            catch (error) {
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
        const getGames = async (filter?: string) => {
            dispatch(gamesLoadingEvent(ProcessState.NotStarted, "" /* Clear error message */));

            const accessToken = await acquireTokenSilentOnly();
            if (!accessToken) {
                dispatch(gamesLoadingEvent(ProcessState.Error, "Authentication missing"));
                return;
            }

            dispatch(gamesLoadingEvent(ProcessState.Processing, "" /* Clear error message */));
            const request: RequestInit = {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            const requestFilter = filter ? filter : GameStateFilter.WAITING_FOR_YOU;

            try {
                const response = await fetch(endpoint + "/api/games?state=" + requestFilter, request);
                if (response.ok) {
                    const data = await response.json();

                    dispatch(gamesLoadingEvent(ProcessState.Success, "" /* Clear error message */, data));

                    const userSettings = Database.get<UserSettings>(DatabaseFields.ME_SETTINGS);
                    if (!userSettings) {
                        // No user settings available. Force fetching that information.
                        ai.trackEvent({ name: "Games-SettingsMissing" });
                        dispatch(settingsLoadingRequestedEvent());
                    }
                }
                else {
                    ai.trackEvent({
                        name: "Games-LoadFailed", properties: {
                            status: response.status,
                            statusText: response.statusText,
                        }
                    });
                    dispatch(gamesLoadingEvent(ProcessState.Error, "Unable to retrieve games."));
                }
            }
            catch (error) {
                console.log(error);
                ai.trackException({ exception: error });

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve games.";
                dispatch(gamesLoadingEvent(ProcessState.Error, errorMessage));
            }
        }

        if (gamesRequested && gamesRequested > gamesProcessed) {
            setGamesProcessed(gamesRequested);
            ai.trackEvent({ name: "Games-Load" });
            getGames(gamesFilter);
        }
    }, [gamesRequested, ai, dispatch, endpoint, acquireTokenSilentOnly, gamesProcessed, gamesFilter]);

    useEffect(() => {
        const getGame = async (query: GameQuery) => {
            dispatch(gamesLoadingSingleEvent(ProcessState.NotStarted, "" /* Clear error message */));

            const accessToken = await acquireTokenSilentOnly();
            if (!accessToken) {
                dispatch(gamesLoadingSingleEvent(ProcessState.Error, "Authentication missing"));
                return;
            }

            dispatch(gamesLoadingSingleEvent(ProcessState.Processing, "" /* Clear error message */));
            const request: RequestInit = {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + `/api/games/${query.id}?state=${query.filter}`, request);
                if (response.ok) {
                    const data = await response.json() as MyChessGame;

                    dispatch(gamesLoadingSingleEvent(ProcessState.Success, "" /* Clear error message */, data));
                }
                else {
                    ai.trackEvent({
                        name: "Games-Single-LoadFailed", properties: {
                            status: response.status,
                            statusText: response.statusText,
                        }
                    });
                    dispatch(gamesLoadingSingleEvent(ProcessState.Error, "Unable to retrieve game."));
                }
            }
            catch (error) {
                console.log(error);
                ai.trackException({ exception: error });

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to retrieve game.";
                dispatch(gamesLoadingSingleEvent(ProcessState.Error, errorMessage));
            }
        }

        if (gamesSingleQuery && gamesSingleQuery !== gamesSingleProcessed) {
            setGamesSingleProcessed(gamesSingleQuery);
            ai.trackEvent({ name: "Games-Single-Load" });
            getGame(gamesSingleQuery);
        }
    }, [gamesSingleQuery, ai, dispatch, endpoint, acquireTokenSilentOnly, gamesSingleProcessed]);

    useEffect(() => {
        const deleteGame = async (id: string) => {
            dispatch(gamesDeleteEvent(ProcessState.NotStarted, "" /* Clear error message */));

            const accessToken = await acquireTokenSilentOnly();
            if (!accessToken) {
                dispatch(gamesDeleteEvent(ProcessState.Error, "Authentication missing"));
                return;
            }

            dispatch(gamesDeleteEvent(ProcessState.Processing, "" /* Clear error message */));
            const request: RequestInit = {
                method: "DELETE",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + `/api/games/${id}`, request);
                if (response.ok) {
                    dispatch(gamesDeleteEvent(ProcessState.Success, "" /* Clear error message */));
                    history.push("/");
                }
                else {
                    ai.trackEvent({
                        name: "Games-DeleteFailed", properties: {
                            status: response.status,
                            statusText: response.statusText,
                        }
                    });
                    dispatch(gamesDeleteEvent(ProcessState.Error, "Unable to delete game."));
                }
            }
            catch (error) {
                console.log(error);
                ai.trackException({ exception: error });

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to delete game.";
                dispatch(gamesDeleteEvent(ProcessState.Error, errorMessage));
            }
        }

        if (gamesDeleteRequested && gamesDeleteRequested !== gamesDeleteProcessed) {
            setGamesDeleteProcessed(gamesDeleteRequested);
            ai.trackEvent({ name: "Games-Delete" });
            deleteGame(gamesDeleteRequested);
        }
    }, [gamesDeleteRequested, ai, dispatch, history, endpoint, acquireTokenSilentOnly, gamesDeleteProcessed]);

    useEffect(() => {
        const createGame = async (game: MyChessGame) => {
            dispatch(gamesCreateEvent(ProcessState.NotStarted, "" /* Clear error message */, "" /* Clear error link*/));

            const accessToken = await acquireTokenSilentOnly();
            if (!accessToken) {
                dispatch(gamesCreateEvent(ProcessState.Error, "Authentication missing"));
                return;
            }

            const request: RequestInit = {
                method: "POST",
                body: JSON.stringify(game),
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + "/api/games", request);
                if (response.ok) {
                    dispatch(gamesCreateEvent(ProcessState.Success, "" /* Clear error message */, "" /* Clear error link*/));
                    const gameCreated = await response.json() as MyChessGame;

                    history.push(`/play/${gameCreated.id}?state=${GameStateFilter.WAITING_FOR_OPPONENT}`);
                }
                else if (response.status === 400 /* Bad request */) {
                    const data = await response.json();
                    console.log(data);

                    const ex = data as ProblemDetail;
                    if (ex.title !== undefined && ex.instance !== undefined) {
                        console.log(ex);
                        dispatch(gamesCreateEvent(ProcessState.Error, ex.title, ex.instance));
                    }
                }
                else {
                    ai.trackEvent({
                        name: "Games-CreateFailed", properties: {
                            status: response.status,
                            statusText: response.statusText,
                        }
                    });
                    dispatch(gamesCreateEvent(ProcessState.Error, "Unable to create game."));
                }
            }
            catch (error) {
                console.log(error);
                ai.trackException({ exception: error });

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to create game.";
                dispatch(gamesCreateEvent(ProcessState.Error, errorMessage, ""));

                console.log(error);
                console.log(errorMessage);
            }
        }

        if (gamesCreateRequested && gamesCreateRequested !== gamesCreateProcessed) {
            setGamesCreateProcessed(gamesCreateRequested);
            ai.trackEvent({ name: "Games-Create" });
            createGame(gamesCreateRequested);
        }
    }, [gamesCreateRequested, ai, dispatch, history, endpoint, acquireTokenSilentOnly, gamesCreateProcessed]);

    useEffect(() => {
        const createMove = async (moveSubmit: MoveSubmit) => {
            dispatch(gamesMoveCreateEvent(ProcessState.NotStarted, "" /* Clear error message */, "" /* Clear error link*/));

            const accessToken = await acquireTokenSilentOnly();
            if (!accessToken) {
                dispatch(gamesMoveCreateEvent(ProcessState.Error, "Authentication missing"));
                return;
            }

            dispatch(gamesMoveCreateEvent(ProcessState.Processing, "" /* Clear error message */, "" /* Clear error link*/));
            const request: RequestInit = {
                method: "POST",
                body: JSON.stringify(moveSubmit.move),
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + accessToken
                }
            };

            try {
                const response = await fetch(endpoint + `/api/games/${moveSubmit.id}/moves`, request);
                console.log(response);
                if (response.ok) {
                    dispatch(gamesMoveCreateEvent(ProcessState.Success, "" /* Clear error message */, "" /* Clear error link*/));
                }
                else if (response.status === 400 /* Bad request */) {
                    const data = await response.json();
                    console.log(data);

                    const ex = data as ProblemDetail;
                    if (ex.title !== undefined && ex.instance !== undefined) {
                        console.log(ex);
                        dispatch(gamesMoveCreateEvent(ProcessState.Error, ex.title, ex.instance));
                    }
                }
                else {
                    ai.trackEvent({
                        name: "Games-Move-CreateFailed", properties: {
                            status: response.status,
                            statusText: response.statusText,
                        }
                    });
                    dispatch(gamesMoveCreateEvent(ProcessState.Error, "Unable to create game."));
                }
            }
            catch (error) {
                console.log(error);
                ai.trackException({ exception: error });

                const errorMessage = error.errorMessage ? error.errorMessage : "Unable to create game.";
                dispatch(gamesMoveCreateEvent(ProcessState.Error, errorMessage, ""));

                console.log(error);
                console.log(errorMessage);
            }
        }

        if (gamesMoveCreateRequested && gamesMoveCreateRequested !== gamesMoveCreateProcessed) {
            setMoveGamesCreateProcessed(gamesMoveCreateRequested);
            ai.trackEvent({ name: "Games-Move-Create" });
            createMove(gamesMoveCreateRequested);
        }
    }, [gamesMoveCreateRequested, ai, dispatch, history, endpoint, acquireTokenSilentOnly, gamesMoveCreateProcessed]);

    useEffect(() => {
        const upsertFriend = async (player: User) => {
            dispatch(friendUpsertEvent(ProcessState.NotStarted, "" /* Clear error message */, "" /* Clear error link*/));

            const accessToken = await acquireTokenSilentOnly();
            if (!accessToken) {
                dispatch(friendUpsertEvent(ProcessState.Error, "Authentication missing"));
                return;
            }

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
                if (response.ok) {
                    dispatch(friendUpsertEvent(ProcessState.Success, "" /* Clear error message */, "" /* Clear error link*/));
                    const userSettings = Database.get<UserSettings>(DatabaseFields.ME_SETTINGS);
                    if (userSettings) {
                        history.push("/friends");
                    }
                    else {
                        history.push("/settings");
                    }
                }
                else if (response.status === 400 /* Bad request */) {
                    const data = await response.json();
                    const ex = data as ProblemDetail;
                    if (ex.title !== undefined && ex.instance !== undefined) {
                        console.log(ex);
                        dispatch(friendUpsertEvent(ProcessState.Error, ex.title, ex.instance));
                    }
                }
                else {
                    ai.trackEvent({
                        name: "Friends-UpsertFailed", properties: {
                            status: response.status,
                            statusText: response.statusText,
                        }
                    });
                    dispatch(friendUpsertEvent(ProcessState.Error, "Unable to modify friend."));
                }
            }
            catch (error) {
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
            if (!accessToken) {
                dispatch(friendUpsertEvent(ProcessState.Error, "Authentication missing"));
                return;
            }

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
                    dispatch(settingsLoadingEvent(ProcessState.Success, "" /* Clear error message */, playerSettings));
                    Database.set(DatabaseFields.ME_SETTINGS, playerSettings);

                    history.push("/");
                }
                else if (response.status === 400 /* Bad request */) {
                    const data = await response.json();
                    console.log(data);

                    const ex = data as ProblemDetail;
                    if (ex.title !== undefined && ex.instance !== undefined) {
                        console.log(ex);
                        dispatch(settingsUpsertEvent(ProcessState.Error, ex.title, ex.instance));
                    }
                }
                else {
                    ai.trackEvent({
                        name: "Settings-UpsertFailed", properties: {
                            status: response.status,
                            statusText: response.statusText,
                        }
                    });
                    dispatch(settingsUpsertEvent(ProcessState.Error, "Unable to update settings."));
                }
            }
            catch (error) {
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
