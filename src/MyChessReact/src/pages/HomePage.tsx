import React from 'react';
import { GameList } from "../components/GameList";
import "./HomePage.css";
import { GetConfiguration } from "../ConfigurationManager";

let configuration = GetConfiguration();

export function HomePage() {
    return (
        <GameList title="Games waiting for you" endpoint={configuration.endpoint} />
    );
}
