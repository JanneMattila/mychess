import { useSelector, TypedUseSelectorHook } from "react-redux";
import { EventTypes, RootState, RootAction, ProcessState, DialogType } from "./actions";

export const getInitialState = () => {
    return {
        loginState: ProcessState.NotStarted,
        loginRequested: 0,
        logoutRequested: 0,
        gamesState: ProcessState.NotStarted,
        gamesRequested: 0,
        gamesCreateState: ProcessState.NotStarted,
        friendsState: ProcessState.NotStarted,
        friendsRequested: 0,
        settingsState: ProcessState.NotStarted,
        settingsLoadingRequested: 0
    }
}

export const useTypedSelector: TypedUseSelectorHook<RootState> = useSelector;

export default function appReducer(state: RootState = getInitialState(), action: RootAction) {
    switch (action.type) {
        case EventTypes.AUTH_LOGIN: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                loginState: action.loginState,
                error: action.error,
                account: action.account,
                accessToken: action.accessToken
            })
        }
        case EventTypes.AUTH_LOGIN_REQUESTED: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                loginRequested: state.loginRequested ? state.loginRequested + 1 : 1,
            })
        }
        case EventTypes.AUTH_LOGOUT:
        // eslint-disable-next-line
        case EventTypes.AUTH_LOGIN_EXPIRED: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                loginState: ProcessState.NotStarted,
                account: undefined,
                accessToken: undefined
            })
        }
        case EventTypes.AUTH_LOGOUT_REQUESTED: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                logoutRequested: state.logoutRequested ? state.logoutRequested + 1 : 1,
            })
        }

        case EventTypes.GAMES_LOADING: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                gamesState: action.gamesState,
                error: action.error,
                games: action.games
            })
        }
        case EventTypes.GAMES_REQUESTED: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                gamesRequested: state.gamesRequested ? state.gamesRequested + 1 : 1,
                gamesFilter: action.gamesFilter
            })
        }
        case EventTypes.GAMES_CREATE: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                gamesCreateState: action.gamesCreateState,
                error: action.error,
                errorLink: action.errorLink
            })
        }
        case EventTypes.GAMES_CREATE_REQUESTED: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                gamesCreateRequested: action.game,
            })
        }
        case EventTypes.GAMES_MOVE_CREATE: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                gamesMoveCreateState: action.gamesMoveCreateState,
                error: action.error,
                errorLink: action.errorLink
            })
        }
        case EventTypes.GAMES_MOVE_CREATE_REQUESTED: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                gamesMoveCreateRequested: action.move,
            })
        }

        case EventTypes.ME_LOADING: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                meState: action.meState,
                error: action.error,
                me: action.me
            })
        }

        case EventTypes.FRIENDS_LOADING: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                friendsState: action.friendsState,
                error: action.error,
                friends: action.friends
            })
        }
        case EventTypes.FRIENDS_REQUESTED: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                friendsRequested: state.friendsRequested ? state.friendsRequested + 1 : 1,
            })
        }
        case EventTypes.FRIENDS_UPSERT_REQUESTED: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                friendsUpsertRequested: action.friend,
            })
        }
        case EventTypes.FRIENDS_UPSERT: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                friendUpsertState: action.friendUpsertState,
                error: action.error,
                errorLink: action.errorLink
            })
        }

        case EventTypes.SETTINGS_LOADING: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                settingsState: action.settingsState,
                error: action.error,
                userSettings: action.userSettings
            })
        }
        case EventTypes.SETTINGS_LOADING_REQUESTED: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                settingsLoadingRequested: state.settingsLoadingRequested ? state.settingsLoadingRequested + 1 : 1,
            })
        }
        case EventTypes.SETTINGS_UPSERT: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                settingsUpsertState: action.settingsUpsertState,
                error: action.error,
                errorLink: action.errorLink
            })
        }
        case EventTypes.SETTINGS_UPSERT_REQUESTED: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                settingsUpsertRequested: action.userSettings,
            })
        }

        case EventTypes.PLAY_SHOW_DIALOG: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                activeDialog: action.show ? action.dialog : DialogType.None,
            })
        }
        default:
            return state;
    }
}
