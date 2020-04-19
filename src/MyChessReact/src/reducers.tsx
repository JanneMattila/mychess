import { useSelector, TypedUseSelectorHook } from "react-redux";
import { Account } from "msal";
import { EventTypes } from "./actions";

type RootAction =
    | { type: EventTypes.LOGIN, success: boolean, error?: string, account?: Account }
    | { type: EventTypes.LOGOUT }
    | { type: EventTypes.LOGIN_EXPIRED }

export interface RootState {
    readonly loggedIn: boolean
    readonly error?: string
    readonly account?: Account
}

const initialState = {
    loggedIn: false,
}

export const useTypedSelector: TypedUseSelectorHook<RootState> = useSelector;

export default function appReducer(state: RootState = initialState, action: RootAction) {
    switch (action.type) {
        case EventTypes.LOGIN: {
            return Object.assign({}, state, {
                loggedIn: action.success,
                error: action.error,
                account: action.account
            })
        }
        case EventTypes.LOGOUT:
        // eslint-disable-next-line
        case EventTypes.LOGIN_EXPIRED: {
            return Object.assign({}, state, {
                loggedIn: false
            })
        }
        default:
            return state
    }
}
