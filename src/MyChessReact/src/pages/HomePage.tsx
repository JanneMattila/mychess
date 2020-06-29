import React from 'react';
import { GameList } from "../components/GameList";
import { GetConfiguration } from "../ConfigurationManager";
import { Link } from 'react-router-dom';

let configuration = GetConfiguration();

export function HomePage() {
    return (
        <div>
            <GameList title="Games waiting for you" endpoint={configuration.endpoint} />
            <footer className="App-footer-container">
                <Link to="/privacy" className="App-footer-link">
                    Privacy
                </Link>
                <span className="App-footer-link-separator">&nbsp;&nbsp;|&nbsp;&nbsp;</span>
                <Link to="/play/local" className="App-footer-link">
                    Play locally
                </Link>
            </footer>
        </div>
    );
}
