import React from "react";
import { BrowserRouter as Router } from "react-router-dom";
import { render } from "@testing-library/react";
import { Auth } from "./Auth";
import { Provider } from "react-redux";
import { createStore } from "redux";
import rootReducer from "../reducers";

const store = createStore(rootReducer);

test("renders sign in", () => {
  const { getByText } = render(<Provider store={store}><Router><Auth clientId="a" applicationIdURI="b" /></Router></Provider>);
  const signInElement = getByText(/Sign In/i);
  expect(signInElement).toBeInTheDocument();
});
