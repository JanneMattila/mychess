import React from 'react';
import { render } from '@testing-library/react';
import App from './App';
import { Provider } from "react-redux";
import { createStore } from "redux";
import rootReducer from "./reducers";

const store = createStore(rootReducer);

test('renders My Chess is coming', () => {
  const { getByText } = render(<Provider store={store}><App /></Provider >);
  const linkElement = getByText(/My Chess is coming/i);
  expect(linkElement).toBeInTheDocument();
});
