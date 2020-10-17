import { AccountInfo } from "@azure/msal-browser";
import { GameQuery } from "./models/GameQuery";
import { MyChessGame } from "./models/MyChessGame";
import { MyChessGameMove } from "./models/MyChessGameMove";
import { User } from "./models/User";
import { UserSettings } from "./models/UserSettings";

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
    AUTH_LOGIN_REQUESTED = "Auth/LoginRequested",
    AUTH_LOGIN_EXPIRED = "Auth/LoginExpired",
    AUTH_LOGOUT_REQUESTED = "Auth/Logout/LoginRequested",
    AUTH_LOGOUT = "Auth/Logout",

    // Game loading related events
    GAMES_LOADING = "Games/Loading",
    GAMES_REQUESTED = "Games/Loading/Requested",
    GAMES_SINGLE = "Games/Loading/Single",
    GAMES_SINGLE_REQUESTED = "Games/Loading/Single/Requested",
    GAMES_CREATE = "Games/Create",
    GAMES_CREATE_REQUESTED = "Games/Create/Requested",
    GAMES_MOVE_CREATE = "Games/Move/Create",
    GAMES_MOVE_CREATE_REQUESTED = "Games/Move/Create/Requested",

    // Me loading related events
    ME_LOADING = "Me/Loading",

    // Game loading related events
    FRIENDS_LOADING = "Friends/Loading",
    FRIENDS_REQUESTED = "Friends/Loading/Requested",
    FRIENDS_UPSERT = "Friends/Upsert",
    FRIENDS_UPSERT_REQUESTED = "Friends/Upsert/Requested",

    // User settings related events
    SETTINGS_LOADING = "Settings/Loading",
    SETTINGS_LOADING_REQUESTED = "Settings/Loading/Requested",
    SETTINGS_UPSERT = "Settings/Upsert",
    SETTINGS_UPSERT_REQUESTED = "Settings/Upsert/Requested",

    // Game playing related events
    PLAY_LOADING = "Play/Loading",
    PLAY_SHOW_DIALOG = "Play/Dialog",
};

type LoginAction = { type: EventTypes.AUTH_LOGIN, loginState: ProcessState, error?: string, account?: AccountInfo, accessToken?: string }
type LoginRequestedAction = { type: EventTypes.AUTH_LOGIN_REQUESTED }
type LogoutRequestedAction = { type: EventTypes.AUTH_LOGOUT_REQUESTED }
type LogoutAction = { type: EventTypes.AUTH_LOGOUT }
type LoginExpiredAction = { type: EventTypes.AUTH_LOGIN_EXPIRED }
type GamesLoadingAction = { type: EventTypes.GAMES_LOADING, gamesState: ProcessState, error?: string, games?: MyChessGame[] }
type GamesRequestedAction = { type: EventTypes.GAMES_REQUESTED, gamesFilter: string }
type GamesLoadingSingleAction = { type: EventTypes.GAMES_SINGLE, gamesSingleState: ProcessState, error?: string, errorLink?: string, game?: MyChessGame }
type GamesSingleRequestedAction = { type: EventTypes.GAMES_SINGLE_REQUESTED, gamesFilter: string, gameID: string }
type GamesCreateAction = { type: EventTypes.GAMES_CREATE, gamesCreateState: ProcessState, error?: string, errorLink?: string }
type GamesCreateRequestedAction = { type: EventTypes.GAMES_CREATE_REQUESTED, game: MyChessGame }
type GamesMoveCreateAction = { type: EventTypes.GAMES_MOVE_CREATE, gamesMoveCreateState: ProcessState, error?: string, errorLink?: string }
type GamesMoveCreateRequestedAction = { type: EventTypes.GAMES_MOVE_CREATE_REQUESTED, move: MyChessGameMove }
type MeLoadingAction = { type: EventTypes.ME_LOADING, meState: ProcessState, error?: string, me?: string }
type FriendsLoadingAction = { type: EventTypes.FRIENDS_LOADING, friendsState: ProcessState, error?: string, friends?: User[] }
type FriendsRequestedAction = { type: EventTypes.FRIENDS_REQUESTED }
type FriendUpsertAction = { type: EventTypes.FRIENDS_UPSERT, friendUpsertState: ProcessState, error?: string, errorLink?: string }
type FriendsUpsertRequestedAction = { type: EventTypes.FRIENDS_UPSERT_REQUESTED, friend: User }
type SettingsLoadingAction = { type: EventTypes.SETTINGS_LOADING, settingsState: ProcessState, error?: string, userSettings?: UserSettings }
type SettingsLoadingRequestedAction = { type: EventTypes.SETTINGS_LOADING_REQUESTED }
type SettingsUpsertAction = { type: EventTypes.SETTINGS_UPSERT, settingsUpsertState: ProcessState, error?: string, errorLink?: string }
type SettingsUpsertRequestedAction = { type: EventTypes.SETTINGS_UPSERT_REQUESTED, userSettings?: UserSettings }
type PlayShowDialogAction = { type: EventTypes.PLAY_SHOW_DIALOG, dialog: DialogType, show: boolean }

// Root action
export type RootAction =
    | LoginAction
    | LoginRequestedAction
    | LogoutRequestedAction
    | LogoutAction
    | LoginExpiredAction
    | GamesLoadingAction
    | GamesRequestedAction
    | GamesLoadingSingleAction
    | GamesSingleRequestedAction
    | GamesCreateAction
    | GamesCreateRequestedAction
    | GamesMoveCreateAction
    | GamesMoveCreateRequestedAction
    | MeLoadingAction
    | FriendsLoadingAction
    | FriendsRequestedAction
    | FriendUpsertAction
    | FriendsUpsertRequestedAction
    | SettingsLoadingAction
    | SettingsLoadingRequestedAction
    | SettingsUpsertAction
    | SettingsUpsertRequestedAction
    | PlayShowDialogAction

export interface RootState {
    readonly loginState?: ProcessState
    readonly loginRequested?: number,
    readonly logoutRequested?: number,
    readonly error?: string
    readonly errorLink?: string
    readonly account?: AccountInfo
    readonly accessToken?: string

    readonly gamesState?: ProcessState
    readonly gamesRequested?: number,
    readonly gamesFilter?: string,
    readonly games?: MyChessGame[]
    readonly gamesSingleQuery?: GameQuery
    readonly gamesSingleState?: ProcessState
    readonly gamesSingleRequested?: MyChessGame,
    readonly gamesCreateState?: ProcessState
    readonly gamesCreateRequested?: MyChessGame,
    readonly gamesMoveCreateState?: ProcessState
    readonly gamesMoveCreateRequested?: MyChessGameMove,

    readonly me?: string;
    readonly meState?: ProcessState

    readonly friendsState?: ProcessState
    readonly friendsRequested?: number,
    readonly friends?: User[]
    readonly friendsUpsertRequested?: User,

    readonly friendUpsertState?: ProcessState

    readonly settingsState?: ProcessState
    readonly settingsLoadingRequested?: number,
    readonly userSettings?: UserSettings

    readonly settingsUpsertState?: ProcessState
    readonly settingsUpsertRequested?: UserSettings,

    readonly activeDialog?: DialogType
}

/*
 * Action creators
 */
export function loginEvent(loginState: ProcessState, error?: string, account?: AccountInfo, accessToken?: string): LoginAction {
    return { type: EventTypes.AUTH_LOGIN, loginState, error, account, accessToken };
}

export function loginRequestedEvent(): LoginRequestedAction {
    return { type: EventTypes.AUTH_LOGIN_REQUESTED };
}

export function logoutRequestedEvent(): LogoutRequestedAction {
    return { type: EventTypes.AUTH_LOGOUT_REQUESTED };
}

export function gamesLoadingEvent(gamesState: ProcessState, error?: string, games?: MyChessGame[]): GamesLoadingAction {
    return { type: EventTypes.GAMES_LOADING, gamesState, error, games };
}

export function gamesRequestedEvent(gamesFilter: string): GamesRequestedAction {
    return { type: EventTypes.GAMES_REQUESTED, gamesFilter: gamesFilter };
}

export function gamesLoadingSingleEvent(gamesSingleState: ProcessState, error?: string, game?: MyChessGame): GamesLoadingSingleAction {
    return { type: EventTypes.GAMES_SINGLE, gamesSingleState, error, game };
}

export function gamesSingleRequestedEvent(gamesFilter: string, gameID: string): GamesSingleRequestedAction {
    return { type: EventTypes.GAMES_SINGLE_REQUESTED, gamesFilter, gameID };
}

export function gamesCreateEvent(gamesCreateState: ProcessState, error?: string, errorLink?: string): GamesCreateAction {
    return { type: EventTypes.GAMES_CREATE, gamesCreateState, error, errorLink };
}

export function gamesCreateRequestedEvent(game: MyChessGame): GamesCreateRequestedAction {
    return { type: EventTypes.GAMES_CREATE_REQUESTED, game };
}

export function gamesMoveCreateEvent(gamesMoveCreateState: ProcessState, error?: string, errorLink?: string): GamesMoveCreateAction {
    return { type: EventTypes.GAMES_MOVE_CREATE, gamesMoveCreateState, error, errorLink };
}

export function gamesMoveCreateRequestedEvent(move: MyChessGameMove): GamesMoveCreateRequestedAction {
    return { type: EventTypes.GAMES_MOVE_CREATE_REQUESTED, move };
}

export function meLoadingEvent(meState: ProcessState, error?: string, me?: string): MeLoadingAction {
    return { type: EventTypes.ME_LOADING, meState, error, me };
}

export function friendsLoadingEvent(friendsState: ProcessState, error?: string, friends?: User[]): FriendsLoadingAction {
    return { type: EventTypes.FRIENDS_LOADING, friendsState, error, friends };
}

export function friendsRequestedEvent(): FriendsRequestedAction {
    return { type: EventTypes.FRIENDS_REQUESTED };
}

export function friendUpsertEvent(friendUpsertState: ProcessState, error?: string, errorLink?: string): FriendUpsertAction {
    return { type: EventTypes.FRIENDS_UPSERT, friendUpsertState, error, errorLink };
}

export function friendUpsertRequestedEvent(friend: User): FriendsUpsertRequestedAction {
    return { type: EventTypes.FRIENDS_UPSERT_REQUESTED, friend };
}

export function settingsLoadingEvent(settingsState: ProcessState, error?: string, userSettings?: UserSettings): SettingsLoadingAction {
    return { type: EventTypes.SETTINGS_LOADING, settingsState, error, userSettings };
}

export function settingsLoadingRequestedEvent(): SettingsLoadingRequestedAction {
    return { type: EventTypes.SETTINGS_LOADING_REQUESTED };
}

export function settingsUpsertEvent(settingsUpsertState: ProcessState, error?: string, errorLink?: string): SettingsUpsertAction {
    return { type: EventTypes.SETTINGS_UPSERT, settingsUpsertState, error, errorLink };
}

export function settingsUpsertRequestedEvent(userSettings: UserSettings): SettingsUpsertRequestedAction {
    return { type: EventTypes.SETTINGS_UPSERT_REQUESTED, userSettings };
}

export function showDialogEvent(dialog: DialogType, show: boolean): PlayShowDialogAction {
    return { type: EventTypes.PLAY_SHOW_DIALOG, dialog, show };
}
