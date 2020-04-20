import { useSelector, TypedUseSelectorHook } from "react-redux";
import { EventTypes, RootState, RootAction } from "./actions";

const initialState = {
    loggedIn: false,
    gamesLoaded: false
}

export const useTypedSelector: TypedUseSelectorHook<RootState> = useSelector;

export default function appReducer(state: RootState = initialState, action: RootAction) {
    switch (action.type) {
        case EventTypes.AUTH_LOGIN: {
            return Object.assign({}, state, {
                loggedIn: action.success,
                error: action.error,
                account: action.account,
                accessToken: action.accessToken
            })
        }
        case EventTypes.AUTH_LOGOUT:
        // eslint-disable-next-line
        case EventTypes.AUTH_LOGIN_EXPIRED: {
            return Object.assign({}, state, {
                loggedIn: false,
                account: null,
                accessToken: null
            })
        }

        case EventTypes.GAMES_LOADING: {
            return Object.assign({}, state, {
                loaded: action.loaded,
                error: action.error,
                games: action.games
            })
        }
        default:
            return state;
    }
}
