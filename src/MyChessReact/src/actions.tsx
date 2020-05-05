import { Account } from "msal";
import { MyChessGame } from "./models/MyChessGame";

export enum ProcessState {
    NotStarted,
    Processing,
    Success,
    Error
};

export enum DialogType {
    None,
    NewGame,
    Promotion,
    MakeMove
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
    GAMES_LOADING = "Games/Load",

    // Game playing related events
    PLAY_LOADING = "Play/Load",
    PLAY_SHOW_DIALOG = "Play/Dialog",
};

type LoginAction = { type: EventTypes.AUTH_LOGIN, loginState: ProcessState, error?: string, account?: Account, accessToken?: string }
type LogoutAction = { type: EventTypes.AUTH_LOGOUT }
type LoginExpiredAction = { type: EventTypes.AUTH_LOGIN_EXPIRED }
type GamesLoadingAction = { type: EventTypes.GAMES_LOADING, gamesState: ProcessState, error?: string, games?: MyChessGame[] }
type PlayShowDialogAction = { type: EventTypes.PLAY_SHOW_DIALOG, dialog: DialogType, show: boolean }

// Root action
export type RootAction =
    | LoginAction
    | LogoutAction
    | LoginExpiredAction
    | GamesLoadingAction
    | PlayShowDialogAction

export interface RootState {
    readonly loginState?: ProcessState
    readonly error?: string
    readonly account?: Account
    readonly accessToken?: string

    readonly gamesState?: ProcessState
    readonly games?: MyChessGame[]

    readonly activeDialog?: DialogType
}

/*
 * Action creators
 */
export function loginEvent(loginState: ProcessState, error?: string, account?: Account, accessToken?: string): LoginAction {
    return { type: EventTypes.AUTH_LOGIN, loginState, error, account, accessToken };
}

export function gamesLoadingEvent(gamesState: ProcessState, error?: string, games?: MyChessGame[]): GamesLoadingAction {
    return { type: EventTypes.GAMES_LOADING, gamesState, error, games };
}

export function showDialogEvent(dialog: DialogType, show: boolean): PlayShowDialogAction {
    return { type: EventTypes.PLAY_SHOW_DIALOG, dialog, show };
}
