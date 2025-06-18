import { Outlet } from 'react-router'; // Isto kao u CompanyDashboard.tsx gde import from 'react-router-dom'
import './App.css';
import Navbar from './Components/Navbar/Navbar';
import { ToastContainer } from 'react-toastify'; // For small pop-up which will be seen while login/register....
import "react-toastify/dist/ReactToastify.css"; // Default Css for ToastContainer
import { UserProvider } from './Context/useAuthContext';

function App() {

  return (
    /* Navbar ocu da bude prisutan na svakoj stranici aplikacije i zato ga stavljam ovde u App.tsx (u Routes.tsx nisam ga stavio jer path:"" za <App> bi trazilo da u svakoj children route of <App> stavim Navbar),
    jer <App /> jer glavna component u Routes.tsx
       Mora ovde biti <Outlet /> zbog childer for <App /> in Routes.tsx + da renderuje Component of current route.
       Routes.tsx ima createBrowserRouter, jer je to novija verzija od <Routes><Route></Routes> koje sam, u ReactApp projektu, pisao u App.tsx. 
       <UserProvider> okruzuje <Navbar />, <ToastContainer /> i <Outlet /> (Outlet = sve children routes(components) za <App /> u Routes.tsx ), stoga u tim components mogu da pristupim direktno svemu iz Context 
      jer u UserProvider imam childer:React.ReactNode i renderovanje children. 
       */
    <>
    <UserProvider>
      <Navbar />
      <Outlet /> {/* Children routes, and also <ProtectedRoute>, of <App /> render here according to Routes.tsx. Takodje imaju pristup svemu iz UserProvider without Prop passing. */}
      <ToastContainer />
    </UserProvider>
    </>
  );
}

export default App;

// Da bi razumeo aplikaciju, pocni od Pages foldera...