import { useSelector, TypedUseSelectorHook } from "react-redux";
import { EventTypes, RootState, RootAction, ProcessState, DialogType } from "./actions";

export const getInitialState = () => {
    return {
        loginState: ProcessState.NotStarted,
        gamesState: ProcessState.NotStarted,
        friendsState: ProcessState.NotStarted
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
        case EventTypes.AUTH_LOGOUT:
        // eslint-disable-next-line
        case EventTypes.AUTH_LOGIN_EXPIRED: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                loginState: ProcessState.NotStarted,
                account: undefined,
                accessToken: undefined
            })
        }

        case EventTypes.GAMES_LOADING: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                gamesState: action.gamesState,
                error: action.error,
                games: action.games
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

        case EventTypes.PLAY_SHOW_DIALOG: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                activeDialog: action.show ? action.dialog : DialogType.None,
            })
        }
        default:
            return state;
    }
}
