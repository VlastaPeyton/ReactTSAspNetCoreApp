import { createBrowserRouter} from "react-router-dom";
import App from "../App";
import HomePage from "../Pages/HomePage/HomePage";
import SearchPage from "../Pages/SearchPage/SearchPage";
import CompanyPage from "../Pages/CompanyPage/CompanyPage";
import CompanyProfile from "../Components/CompanyProfile/CompanyProfile";
import IncomeStatement from "../Components/IncomeStatement/IncomeStatement";
import DesignGuide from "../Pages/DesignGuide/DesignGuide";
import BalanceSheet from "../Components/BalanceSheet/BalanceSheet";
import Cashflow from "../Components/Cashflow/Cashflow";
import LoginPage from "../Pages/LoginPage/LoginPage";
import RegisterPage from "../Pages/RegisterPage/RegisterPage";
import ProtectedRoute from "./ProtectedRoute";

// createBrowserRouter je noviji nacin umesto u App.tsx da stavim <Routes> <Route path="/" element={<HomePage />} /> .... </Routes> kao u ReactApp projektu sto sam uradio.
// Components, koje nisu navedene ovde, a jesu u App.tsx, bice prisutne na svakoj stranici ovde navedenoj.
// Sve ove children routes nekog parent route (elementa) ( <App /> ili <CompanyPage>) moraju biti inside <Outlet /> u definicije parenta (tj App / CompanyPage). 
/* Nijedna route se ne pise sa / na pocetku,a razlog pogledaj u Sidebar.tsx - zato sto u <Link> ako kucam "/route" onda je to apsolutna ruta pod uslovom da je definisana ovde kao parent ili child 
 dok ako kucam "childRoute" to je relativna route koja se nadoveze na current route pod uslovom da je ta relativna child of parent u kom smo u trenutku poziv <Link to="childRoute"> */
// CompanyPage je ProtectedRoute, pa treba login da bih joj pristupio + i svi njeni children routes su protected automatski. 
export const router = createBrowserRouter([
    {   
        path:"/",         // Base path when opening app = htpp://localhost:3000/
        element: <App />, // Uvek App.tsx ovde za path:"/"
        children: [       // Gde sve mozemo otici iz pocetne strane. Zbog childer za <App /> , u App.tsx mora <Outlet /> postojati. 
            {path: "", element: <HomePage/>},                                          // http://localhost:3000/
            {path: "search", element:<ProtectedRoute><SearchPage /></ProtectedRoute>}, // http://localhost:3000/search
            {path: "design-guide", element: <DesignGuide />},                          // http://localhost:3000/design-guide
            {path: "login", element: <LoginPage />},                                   // http://localhost:3000/login
            {path: "register", element: <RegisterPage />},                             // http://localhost:300/register
            {path: "company/:ticker",                   
             element:  <ProtectedRoute><CompanyPage /></ProtectedRoute>,               // http://localhost:3000/company/:ticker
             children: [{path:"company-profile", element: <CompanyProfile />},         // http://localhost:3000/company/:ticker/company-profile
                        {path:"income-statement", element: <IncomeStatement />},       // http://localhost:3000/company/:ticker/income-statement
                        {path:"balance-sheet", element: <BalanceSheet />},             // http://localhost:3000/company/:ticker/balance-sheet
                        {path:"cashflow", element: <Cashflow />}                       // http://localhost:3000/company/:ticker/cashflow
             ], 
            } /* Zbog children routes za CompanyPage (koje su takodje ProtectedRoute kao CompanyPage), CompanyPage.tsx (tj CompanyDashboard.tsx element koji je glavni za tu stranicu) mora imati children: React.ReactNode u Props i <Outlet /> jer kad pristupam nekoj child route 
               of CompanyPage, prvo se renderuje CompanyPage i CompanyDashboard u njoj, pa onda pomocu <Outlet ... /> u CompanyDashboard se renderuje zeljeni child route kad u Sidebar kliknem Company Profile/ Income Statement / Balance Sheet / Cashflow. 
                Isto tako <Outlet> u App.tsx renderuje sve child routes of App. 
                U child route of CompanyPage, ako zelimo da dohvatimo context iz <Outlet context=...> from parent route (CompanyPage tj CompanyDashboard) moram koristiti useOutletContext da dohvatim context vrednost.  
                Zbog nemanja "path: "", element: <CompanyProfileDashboardPage />" " u children za <CompanyPage />, na svaku child route of company/:ticker (tj CompanyPage ), bice pristuno sve iz <CompanyPage /> + pojedinosti za tog child route. Dok ovaj path:"" za App ima i tamo to nije slucaj. */
        ],
    },
]);

/* U App.tsx, unutar <UserProvider>(context) imamo <Navbar>, <ToastContainer> i <Outlet>, gde <Outlet> je sve children routes for <App>, stoga, sve te children routes
mogu, kao i <Navbar> i <ToastContainer>, pristupiti direknto svemu iz useAuthContext.tsx.  */