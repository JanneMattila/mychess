import { EventTypes } from "./actions";

type Action =
    | { type: EventTypes.LOGIN, success: boolean }
    | { type: EventTypes.LOGOUT }
    | { type: EventTypes.LOGIN_EXPIRED }

const initialState = {
    loggedIn: false,
}

export default function appReducer(state = initialState, action: Action) {
    switch (action.type) {
        case EventTypes.LOGIN: {
            return Object.assign({}, state, {
                loggedIn: action.success
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
