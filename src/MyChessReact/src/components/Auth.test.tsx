import React from "react";
import { render } from "@testing-library/react";
import { Auth } from "./Auth";
import { Provider } from "react-redux";
import { createStore } from "redux";
import rootReducer from "../reducers";

const store = createStore(rootReducer);

test("renders sign in", () => {
  const { getByText } = render(<Provider store={store}><Auth clientId="a" applicationIdURI="b" /></Provider>);
  const signInElement = getByText(/Sign In/i);
  expect(signInElement).toBeInTheDocument();
});
