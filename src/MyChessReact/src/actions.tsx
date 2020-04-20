import { Account } from "msal";
import { Game } from "./models/Game";

/*
 * Action event types
 */
export enum EventTypes {
    // Auth related events
    AUTH_LOGIN = "Auth/Login",
    AUTH_LOGIN_EXPIRED = "Auth/Login Expired",
    AUTH_LOGOUT = "Auth/Logout",

    // Game loading related events
    GAMES_LOADING = "Games/Load"
};

// Root action
export type RootAction =
    | { type: EventTypes.AUTH_LOGIN, success: boolean, error?: string, account?: Account, accessToken?: string }
    | { type: EventTypes.AUTH_LOGOUT }
    | { type: EventTypes.AUTH_LOGIN_EXPIRED }
    | { type: EventTypes.GAMES_LOADING, loaded: boolean, error?: string, games?: Game[] }

export interface RootState {
    readonly loggedIn: boolean
    readonly error?: string
    readonly account?: Account
    readonly accessToken?: string

    readonly gamesLoaded: boolean
    readonly games?: Game[]
}

/*
 * Action creators
 */
export function loginEvent(success: boolean, error?: string, account?: Account, accessToken?: string) {
    return { type: EventTypes.AUTH_LOGIN, success, error, account, accessToken };
}

export function gamesLoadingEvent(loaded: boolean, error?: string, games?: Game[]) {
    return { type: EventTypes.GAMES_LOADING, loaded, error, games };
}
