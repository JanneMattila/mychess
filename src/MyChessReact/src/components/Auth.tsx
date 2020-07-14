import React from "react";
import { useDispatch } from "react-redux";
import { useTypedSelector } from "../reducers";
import { ProcessState, loginRequestedEvent, logoutRequestedEvent } from "../actions";
import { Link } from "react-router-dom";
import { getAppInsights } from "./TelemetryService";
import "./Auth.css";

export function Auth() {
    const loginState = useTypedSelector(state => state.loginState);
    const account = useTypedSelector(state => state.account);

    const dispatch = useDispatch();
    const ai = getAppInsights();

    const onSignIn = () => {
        dispatch(loginRequestedEvent());
    }

    const onSignOut = () => {
        dispatch(logoutRequestedEvent());
    }

    if (loginState === ProcessState.Success) {
        return (
            <div className="Auth">
                <Link to="/settings" className="Auth-link">{account?.name}</Link>
                <button onClick={onSignOut} className="Auth-button">Sign out</button>

            </div>
        );
    }
    return (
        <div className="Auth">
            Want to play? Please
            <button onClick={onSignIn} className="Auth-button">sign In</button>
        </div>
    );
}
