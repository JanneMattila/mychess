import React from "react";
import { BrowserRouter as Router, Switch, Route, Link } from "react-router-dom";
import { Auth } from "./components/Auth";
import { Home } from "./pages/Home";
import { Play } from "./pages/Play";
import "./App.css";
import TelemetryProvider from "./components/TelemetryProvider";
import config from "./configuration.json";

function App() {
  return (
    <Router>
      <TelemetryProvider>
        <div>
          <div className="App">
            <Link to="/" className="App-link">My Chess</Link>
            <Auth clientId={config.clientId} applicationIdURI={config.applicationIdURI} />

            <Switch>
              <Route exact path='/' component={Home} />
              <Route exact path='/play/:id' component={Play} />
              <Route path="/settings" />
            </Switch>
          </div>
        </div>
      </TelemetryProvider>
    </Router>
  );
}

export default App;
