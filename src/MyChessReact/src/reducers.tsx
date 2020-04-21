import { useSelector, TypedUseSelectorHook } from "react-redux";
import { EventTypes, RootState, RootAction } from "./actions";

export const getInitialState = () => {
    return {
        loggedIn: false,
        gamesLoaded: false
    }
}

export const useTypedSelector: TypedUseSelectorHook<RootState> = useSelector;

export default function appReducer(state: RootState = getInitialState(), action: RootAction) {
    switch (action.type) {
        case EventTypes.AUTH_LOGIN: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                loggedIn: action.success,
                error: action.error,
                account: action.account,
                accessToken: action.accessToken
            })
        }
        case EventTypes.AUTH_LOGOUT:
        // eslint-disable-next-line
        case EventTypes.AUTH_LOGIN_EXPIRED: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                loggedIn: false,
                account: undefined,
                accessToken: undefined
            })
        }

        case EventTypes.GAMES_LOADING: {
            return Object.assign<RootState, RootState, RootState>(getInitialState(), state, {
                gamesLoaded: action.gamesLoaded,
                error: action.error,
                games: action.games
            })
        }
        default:
            return state;
    }
}
