import React from "react";
import { BrowserRouter as Router, Switch, Route, Link } from "react-router-dom";
import logo from "./logo.svg";
import "./App.css";

function App() {
  return (
    <Router>
      <div>
        <div className="App">
          <Link to="/" className="App-link">My Chess</Link>

          <Switch>
            <Route exact path="/">
              <header className="App-header">
                <img src={logo} className="App-logo" alt="My Chess" />
                <br />My Chess is coming. Stay tuned.
              </header>
            </Route>
            <Route path="/settings">
            </Route>
          </Switch>
        </div>
      </div>
    </Router>
  );
}

export default App;
