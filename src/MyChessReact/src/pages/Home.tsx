import React, { Component } from 'react';
import { GameList } from "../components/GameList";
import "./Home.css";
import config from "../configuration.json";

export class Home extends Component {
    static displayName = Home.name;

    render() {
        return (
            <div>
                <header className="Home-header">
                    <GameList title="Games waiting for you" endpoint={config.endpoint} />
                </header>
            </div>
        );
    }
}