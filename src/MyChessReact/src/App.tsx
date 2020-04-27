import React from "react";
import { BrowserRouter as Router, Switch, Route, Link } from "react-router-dom";
import { Auth } from "./components/Auth";
import { Home } from "./pages/Home";
import { Play } from "./pages/Play";
import { Settings } from "./pages/Settings";
import "./App.css";
import logo from "./pages/logo.svg";
import TelemetryProvider from "./components/TelemetryProvider";
import config from "./configuration.json";

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
                <Auth clientId={config.clientId} applicationIdURI={config.applicationIdURI} />
              </div>
            </div>
            <Switch>
              <Route exact path="/" component={Home} />
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
