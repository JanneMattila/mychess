import React from "react";
import { BrowserRouter as Router, Switch, Route, Link } from "react-router-dom";
import { Auth } from "./components/Auth";
import { Home } from "./pages/Home";
import { Play } from "./pages/Play";
import { Privacy } from "./pages/Privacy";
import { Settings } from "./pages/Settings";
import "./App.css";
import logo from "./pages/logo.svg";
import TelemetryProvider from "./components/TelemetryProvider";
import { GetConfiguration } from "./ConfigurationManager";

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
                <Auth clientId={configuration.clientId} applicationIdURI={configuration.applicationIdURI} />
              </div>
              <div className="App-privacy-container">
                <Link to="/privacy" className="App-privacy">
                  Privacy
              </Link>
              </div>
            </div>
            <Switch>
              <Route exact path="/" component={Home} />
              <Route exact path="/privacy" component={Privacy} />
              <Route exact path="/play/:id" component={Play} />
              <Route path="/settings" component={Settings} />
            </Switch>
          </div>
        </div>
      </TelemetryProvider>
    </Router>
  );
}

export default App;
