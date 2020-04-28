import React, { Component } from 'react';
import { GameList } from "../components/GameList";
import "./Home.css";
import { GetConfiguration } from "../ConfigurationManager";

let configuration = GetConfiguration();

export class Home extends Component {
    static displayName = Home.name;

    render() {
        return (
            <div>
                <header className="Home-header">
                    <GameList title="Games waiting for you" endpoint={configuration.endpoint} />
                </header>
            </div>
        );
    }
}