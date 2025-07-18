import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import reportWebVitals from './reportWebVitals';
import { RouterProvider } from 'react-router-dom';
import { router } from './Routes/Routes'; // createBrowserRouter from "react-router-dom" koji pruza routes i routing
import * as serviceWorkerRegistration from './serviceWorkerRegistration'; // Zbog pravljenja PWA

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement // Ovo je =  <div id="root"></div> jer React je SPA.
                                                 // MPA (poput .NET MVC) ima .html za svaki route a ja ovde imam gomilu route na 1 .html file
);

root.render(
  <React.StrictMode> 
    <RouterProvider router={router} />
  </React.StrictMode>
);
// StrictMode only runs in development - it's automatically removed in production builds. It doesn't render any visible UI, just adds extra checks.

serviceWorkerRegistration.register(); // Zbog pravljenja PWA jer sam prethodno napravio serviceWorkerRegistration.ts file

reportWebVitals(); // Ovo je built-in i nisam ga skidao iako mogo sam.

// Da bi razumeo sve, kreni iz App.tsx i Pages foldera, jer citajuci sta ima na kojoj strani, shvatices ceo kod.