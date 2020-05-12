import React from "react";
import { render } from "@testing-library/react";
import { GameList } from "./GameList";
import { Provider } from "react-redux";
import { createStore } from "redux";
import rootReducer from "../reducers";
import { loginEvent, gamesLoadingEvent, ProcessState } from "../actions";
import fetch, { enableFetchMocks } from "jest-fetch-mock";
import { BrowserRouter as Router } from "react-router-dom";

const store = createStore(rootReducer);
enableFetchMocks();

test("renders games and name is visible in output", () => {

  fetch.enableMocks();
  store.dispatch(loginEvent(ProcessState.Success, "", undefined, "abcd"));
  store.dispatch(gamesLoadingEvent(ProcessState.Success, "",
    [{ id: "1", name: "abc", opponent: "a", updated: new Date(), moves: [], players: { white: { "name": "a" }, black: { "name": "b" } } }]))

  const { getByText } = render(<Router><Provider store={store}><GameList title="custom title" endpoint="" /></Provider></Router>);
  const signInElement = getByText(/abc/i);
  expect(signInElement).toBeInTheDocument();
  fetch.disableMocks();
});
