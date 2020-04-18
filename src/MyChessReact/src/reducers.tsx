type Action =
    | { type: "login", success: boolean }
    | { type: "logout" }
    | { type: "login expired" }

const initialState = {
    loggedIn: false,
}

export default function appReducer(state = initialState, action: Action) {
    switch (action.type) {
        case "login": {
            return Object.assign({}, state, {
                loggedIn: action.success
            })
        }
        case "logout":
        // eslint-disable-next-line
        case "logout": {
            return Object.assign({}, state, {
                loggedIn: false
            })
        }
        default:
            return state
    }
}
