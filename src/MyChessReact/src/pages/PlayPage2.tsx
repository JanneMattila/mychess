import React, { useEffect, MouseEvent } from "react";
import "./PlayPage.css";
import { ChessBoardView2 } from "../game/ChessBoardView2";
import { useTypedSelector } from "../reducers";
import { Database, DatabaseFields } from "../data/Database";
import logo from "../pages/logo.svg";
import { UserSettings } from "../models/UserSettings";

export function PlayPage2() {
    return (
        <div className="Play-container">
            <ChessBoardView2 />
        </div >
    );
}
