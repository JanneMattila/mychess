import React from "react";
import { useTypedSelector } from "../reducers";
import { ProcessState } from "../actions";
import logo from "../pages/logo.svg";
import "./ProfileLoader.css";

export function ProfileLoader() {
    const loginState = useTypedSelector(state => state.loginState);

    if (loginState === ProcessState.Processing) {
        return (
            <div className="ProfileLoader">
                <div id="Loading" className="ProfileLoader-Spinner">
                    <img src={logo} className="ProfileLoader-logo" alt="logo" /><br />
                    Loading...
                </div>
            </div>
        );
    }
    return (
        <>
        </>
    );
}
