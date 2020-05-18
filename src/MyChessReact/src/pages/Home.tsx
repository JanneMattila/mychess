import React from 'react';
import { GameList } from "../components/GameList";
import "./Home.css";
import { GetConfiguration } from "../ConfigurationManager";

let configuration = GetConfiguration();

export function Home() {
    return (
        <div>
            <header className="Home-header">
                <GameList title="Games waiting for you" endpoint={configuration.endpoint} />
            </header>
        </div>
    );
}
