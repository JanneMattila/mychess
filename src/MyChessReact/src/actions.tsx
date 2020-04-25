import { Account } from "msal";
import { GameModel } from "./models/GameModel";

export enum ProcessState {
    NotStarted,
    Processing,
    Success,
    Error
};

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

type LoginAction = { type: EventTypes.AUTH_LOGIN, loginState: ProcessState, error?: string, account?: Account, accessToken?: string }
type LogoutAction = { type: EventTypes.AUTH_LOGOUT }
type LoginExpiredAction = { type: EventTypes.AUTH_LOGIN_EXPIRED }
type GamesLoadingAction = { type: EventTypes.GAMES_LOADING, gamesState: ProcessState, error?: string, games?: GameModel[] }

// Root action
export type RootAction =
    | LoginAction
    | LogoutAction
    | LoginExpiredAction
    | GamesLoadingAction

export interface RootState {
    readonly loginState?: ProcessState
    readonly error?: string
    readonly account?: Account
    readonly accessToken?: string

    readonly gamesState?: ProcessState
    readonly games?: GameModel[]
}

/*
 * Action creators
 */
export function loginEvent(loginState: ProcessState, error?: string, account?: Account, accessToken?: string): LoginAction {
    return { type: EventTypes.AUTH_LOGIN, loginState, error, account, accessToken };
}

export function gamesLoadingEvent(gamesState: ProcessState, error?: string, games?: GameModel[]): GamesLoadingAction {
    return { type: EventTypes.GAMES_LOADING, gamesState, error, games };
}
