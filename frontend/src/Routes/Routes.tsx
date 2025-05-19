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

// createBrowserRouter je noviji nacin umesto u App.tsx da stavim <Routes> <Route path="/" element={<HomePage />} /> .... </Routes> kao u ReactApp sto sam uradio.
// Components koje nisu navedene ovde, a jesu u App.tsx, bice prisutne na svakoj stranici ovde navedenoj.
// Sve ove children route(components) su u App.tsx inside <Outlet /> 
// CompanyPage je ProtectedRoute tj treba login da se uradi da bih tu uso, ali je i svi njeni children routes protected automatski
export const router = createBrowserRouter([
    {   
        path:"/",         // Base path when opening app
        element: <App />, // Uvek App.tsx ovde
        children: [       // Gde sve mozemo otici iz pocetne strane. Zbog childer za <App> , u App.tsx mora <Outlet /> postojati. 
            {path: "", element: <HomePage/>},         // http://localhost:3000/
            {path: "search", element:<ProtectedRoute><SearchPage /></ProtectedRoute>}, // http://localhost:3000/search
            {path: "design-guide", element: <DesignGuide />}, // http://localhost:3000/design-guide
            {path: "login", element: <LoginPage />}, //http://localhost:3000/login
            {path: "register", element: <RegisterPage />}, //http://localhost:300/register
            {path: "company/:ticker",                   
             element:  <ProtectedRoute><CompanyPage /> </ProtectedRoute>,               // http://localhost:3000/company/:ticker
             children: [{path:"company-profile", element: <CompanyProfile />},   // http://localhost:3000/company/:ticker/company-profile
                        {path:"income-statement", element: <IncomeStatement />}, // http://localhost:3000/company/:ticker/income-statement
                        {path:"balance-sheet", element: <BalanceSheet />},       // http://localhost:3000/company/:ticker/balance-sheet
                        {path:"cashflow", element: <Cashflow />}                 // http://localhost:3000/company/:ticker/cashflow
             ], 
            } /* Zbog children Routes, CompanyPage.tsx tj CompanyDashboard.tsx mora imati children: React.ReactNode i <Outlet>. 
                 Zbog nemanja path: "", element <CompanyProfileDashboardPage />, na sve children routes of company/:ticker, sve iz CompanyPage bice takodje pristuno. */
        ],
    },
]);