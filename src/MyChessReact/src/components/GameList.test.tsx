import React from "react";
import { render } from "@testing-library/react";
import { GameList } from "./GameList";
import { Provider } from "react-redux";
import { createStore } from "redux";
import rootReducer from "../reducers";
import { loginEvent, gamesLoadingEvent } from "../actions";

const store = createStore(rootReducer);

test("renders games and name is visible in output", () => {
  store.dispatch(loginEvent(true, "", undefined, "abcd"));
  store.dispatch(gamesLoadingEvent(true, "", [{ id: "1", name: "abc", opponent: "a", updated: new Date() }]))

  const { getByText } = render(<Provider store={store}><GameList endpoint="" /></Provider>);
  const signInElement = getByText(/abc/i);
  expect(signInElement).toBeInTheDocument();
});
