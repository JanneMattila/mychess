import { Account } from "msal";
import { GameModel } from "./models/GameModel";

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

type LoginAction = { type: EventTypes.AUTH_LOGIN, success: boolean, error?: string, account?: Account, accessToken?: string }
type LogoutAction = { type: EventTypes.AUTH_LOGOUT }
type LoginExpiredAction = { type: EventTypes.AUTH_LOGIN_EXPIRED }
type GamesLoadingAction = { type: EventTypes.GAMES_LOADING, gamesLoaded: boolean, error?: string, games?: GameModel[] }

// Root action
export type RootAction =
    | LoginAction
    | LogoutAction
    | LoginExpiredAction
    | GamesLoadingAction

export interface RootState {
    readonly loggedIn?: boolean
    readonly error?: string
    readonly account?: Account
    readonly accessToken?: string

    readonly gamesLoaded?: boolean
    readonly games?: GameModel[]
}

/*
 * Action creators
 */
export function loginEvent(success: boolean, error?: string, account?: Account, accessToken?: string): LoginAction {
    return { type: EventTypes.AUTH_LOGIN, success, error, account, accessToken };
}

export function gamesLoadingEvent(gamesLoaded: boolean, error?: string, games?: GameModel[]): GamesLoadingAction {
    return { type: EventTypes.GAMES_LOADING, gamesLoaded, error, games };
}
