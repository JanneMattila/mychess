import { useSelector, TypedUseSelectorHook } from "react-redux";
import { EventTypes, RootState, RootAction, ProcessState } from "./actions";

export const getInitialState = () => {
    return {
        loginState: ProcessState.NotStarted,
        gamesState: ProcessState.NotStarted
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
        default:
            return state;
    }
}
