import { Account } from "msal";

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
export function loginEvent(success: boolean, error?: string, account?: Account) {
    return { type: EventTypes.LOGIN, success, error, account };
}
