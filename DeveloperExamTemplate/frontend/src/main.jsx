import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App.jsx';

// The entry point wires up the top-level React component with the DOM.
ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
