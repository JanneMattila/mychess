import EventNames from "./eventNames";

type Action =
    | { type: EventNames.LOGIN, success: boolean }
    | { type: EventNames.LOGOUT }
    | { type: EventNames.LOGIN_EXPIRED }

const initialState = {
    loggedIn: false,
}

export default function appReducer(state = initialState, action: Action) {
    switch (action.type) {
        case EventNames.LOGIN: {
            return Object.assign({}, state, {
                loggedIn: action.success
            })
        }
        case EventNames.LOGOUT:
        // eslint-disable-next-line
        case EventNames.LOGIN_EXPIRED: {
            return Object.assign({}, state, {
                loggedIn: false
            })
        }
        default:
            return state
    }
}
