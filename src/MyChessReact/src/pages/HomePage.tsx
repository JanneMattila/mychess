import React from 'react';
import { GameList } from "../components/GameList";
import { GetConfiguration } from "../ConfigurationManager";
import { Link } from 'react-router-dom';
import { useTypedSelector } from '../reducers';
import { ProcessState } from '../actions';
import "./HomePage.css";

let configuration = GetConfiguration();

export function HomePage() {
    const loginState = useTypedSelector(state => state.loginState);

    const renderWelcomeMessage = () => {
        if (loginState !== ProcessState.Success) {
            return <div>
                <div className="title">Welcome to My Chess!</div>
                <div className="welcomeText">
                    My Chess is social (and not that serious) chess game where
                    you can play chess online with your friends.
                    You can comment your moves and put some pressure to your friends (in fun way of course!).
                        <br />
                </div>
            </div>;
        }
        return "";
    }


    return (
        <div>
            <GameList title="Games waiting for you" endpoint={configuration.endpoint} />
            {renderWelcomeMessage()}
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
