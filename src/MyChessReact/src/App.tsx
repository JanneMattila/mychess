import React from "react";
import { BrowserRouter as Router, Switch, Route, Link } from "react-router-dom";
import { Profile } from "./components/Profile";
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
import { PlayPage2 } from "./pages/PlayPage2";
import CookieConsent from "react-cookie-consent";
import { BackendService } from "./components/BackendService";
import { ProfileLoader } from "./components/ProfileLoader";
import { AboutPage } from "./pages/AboutPage";

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
              <Profile />
            </div>
          </div>

          <Switch>
            <Route exact path="/" component={HomePage} />
            <Route exact path="/about" component={AboutPage} />
            <Route exact path="/privacy" component={PrivacyPage} />
            <Route exact path="/play/local" component={() => <PlayPage endpoint={configuration.endpoint} />} />
            <Route exact path="/play/:id" component={() => <PlayPage endpoint={configuration.endpoint} />} />
            <Route exact path="/play2/local" component={() => <PlayPage2 />} />
            <Route exact path="/play2/:id" component={() => <PlayPage2 />} />
            <Route path="/settings" component={() => <SettingsPage endpoint={configuration.endpoint} webPushPublicKey={configuration.webPushPublicKey} />} />
            <Route exact path="/friends/add" component={() => <ModifyFriendPage title="Add friend" />} />
            <Route path="/friends/add/:id" component={() => <ModifyFriendPage title="Add friend" />} />
            <Route path="/friends/:id" component={() => <ModifyFriendPage title="Modify friend" />} />
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
          <ProfileLoader />
          <BackendService clientId={configuration.clientId} applicationIdURI={configuration.applicationIdURI} endpoint={configuration.endpoint} />
        </div>
      </TelemetryProvider>
    </Router >
  );
}

export default App;
