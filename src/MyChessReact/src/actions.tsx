/*
 * Action event types
 */
export enum EventTypes {
    LOGIN = "LOGIN",
    LOGIN_EXPIRED = "LOGIN_EXPIRED",
    LOGOUT = "LOGOUT"
};


/*
 * Action creators
 */
export function loginEvent(success: boolean) {
    return { type: EventTypes.LOGIN, success };
}
