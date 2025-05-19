import { Outlet } from 'react-router';
import './App.css';
import Navbar from './Components/Navbar/Navbar';
import { ToastContainer } from 'react-toastify';
import "react-toastify/dist/ReactToastify.css"; // Default Css for ToastContainer
import { UserProvider } from './Context/useAuthContext';

function App() {

  return (
    /* Navbar ocu da bude prisutan na svakoj stranici aplikacije i zato ga stavljam u App.tsx tj izvan Routes.tsx.
       Mora ovde biti <Outlet /> zbog childer for <App> in Routes.tsx + da renderuje Component of current route
       Routes.tsx ima createBrowserRouter, jer je to novija verzija od <Routes><Route></Routes> iz ReactApp.  */
    <>
    <UserProvider>
      <Navbar />
      <Outlet />
      <ToastContainer />
    </UserProvider>
    </>
  );
}

export default App;


// Da bi razumeo aplikaciju, pocni od Pages foldera gde je svaka stranica definisana...