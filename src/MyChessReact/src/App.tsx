import React from "react";
import { BrowserRouter as Router, Switch, Route, Link } from "react-router-dom";
import { Auth } from "./components/Auth";
import { HomePage } from "./pages/HomePage";
import { PrivacyPage } from "./pages/PrivacyPage";
import { SettingsPage } from "./pages/SettingsPage";
import { FriendsPage } from "./pages/FriendsPage";
import "./App.css";
import logo from "./pages/logo.svg";
import TelemetryProvider from "./components/TelemetryProvider";
import { GetConfiguration } from "./ConfigurationManager";
import { ModifyFriendPage } from "./pages/ModifyFriendPage";
import { PlayPage } from "./pages/PlayPage";
import CookieConsent from "react-cookie-consent";

let configuration = GetConfiguration();

function App() {
  return (
    <Router>
      <TelemetryProvider>
        <div>
          <div className="App">
            <div>
              <Link to="/" className="App-link">
                <img src={logo} alt="My Chess" className="App-link-image" />
              </Link>
              <div className="App-auth">
                <Auth clientId={configuration.clientId} applicationIdURI={configuration.applicationIdURI} endpoint={configuration.endpoint} />
              </div>
              <div className="App-footer-container">
                <Link to="/privacy" className="App-footer-link">
                  Privacy
                </Link>
                <span className="App-footer-link-separator">&nbsp;&nbsp;|&nbsp;&nbsp;</span>
                <Link to="/play/local" className="App-footer-link">
                  Play locally
                </Link>
              </div>
            </div>
            <Switch>
              <Route exact path="/" component={HomePage} />
              <Route exact path="/privacy" component={PrivacyPage} />
              <Route exact path="/play/local" component={() => <PlayPage endpoint={configuration.endpoint} />} />
              <Route exact path="/play/:id" component={() => <PlayPage endpoint={configuration.endpoint} />} />
              <Route path="/settings" component={() => <SettingsPage endpoint={configuration.endpoint} />} />
              <Route exact path="/friends/add" component={() => <ModifyFriendPage title="Add friend" endpoint={configuration.endpoint} />} />
              <Route path="/friends/add/:id" component={() => <ModifyFriendPage title="Add friend" endpoint={configuration.endpoint} />} />
              <Route path="/friends/:id" component={() => <ModifyFriendPage title="Modify friend" endpoint={configuration.endpoint} />} />
              <Route path="/friends" component={FriendsPage} />
            </Switch>
          </div>
          <CookieConsent
            location="bottom"
            buttonText="Accept"
            cookieName="mychess-cookie-consent"
            style={{ background: "darkgrey", color: "black", display: "" }}
            buttonStyle={{ color: "#4e503b" }}>
            This website uses cookies to enhance the user experience.
          </CookieConsent>
        </div>
      </TelemetryProvider>
    </Router >
  );
}

export default App;
