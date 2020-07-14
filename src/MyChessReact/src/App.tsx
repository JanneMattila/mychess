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
import { BackendService } from "./components/BackendService";

let configuration = GetConfiguration();

function App() {
  return (
    <Router>
      <TelemetryProvider>
        <div className="App">
          <div className="App-header-container">
            <Link to="/">
              <img src={logo} alt="My Chess" className="App-logo" />
            </Link>
            <div className="App-auth">
              <Auth />
            </div>
          </div>

          <Switch>
            <Route exact path="/" component={HomePage} />
            <Route exact path="/privacy" component={PrivacyPage} />
            <Route exact path="/play/local" component={() => <PlayPage endpoint={configuration.endpoint} />} />
            <Route exact path="/play/:id" component={() => <PlayPage endpoint={configuration.endpoint} />} />
            <Route path="/settings" component={() => <SettingsPage endpoint={configuration.endpoint} webPushPublicKey={configuration.webPushPublicKey} />} />
            <Route exact path="/friends/add" component={() => <ModifyFriendPage title="Add friend" endpoint={configuration.endpoint} />} />
            <Route path="/friends/add/:id" component={() => <ModifyFriendPage title="Add friend" endpoint={configuration.endpoint} />} />
            <Route path="/friends/:id" component={() => <ModifyFriendPage title="Modify friend" endpoint={configuration.endpoint} />} />
            <Route path="/friends" component={FriendsPage} />
          </Switch>
          <CookieConsent
            location="bottom"
            buttonText="Accept"
            cookieName="mychess-cookie-consent"
            style={{ background: "darkgrey", color: "black", display: "" }}
            buttonStyle={{ color: "#4e503b" }}
            sameSite="strict">
            This website uses cookies to enhance the user experience.
          </CookieConsent>
          <BackendService clientId={configuration.clientId} applicationIdURI={configuration.applicationIdURI} endpoint={configuration.endpoint} />
        </div>
      </TelemetryProvider>
    </Router >
  );
}

export default App;
