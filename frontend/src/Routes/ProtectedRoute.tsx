import React, { Component } from 'react'
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../Context/useAuthContext';

type Props = {children : React.ReactNode} // Sve stranice(routes) koje ce biti unutar <ProtectedRoute> a njih u Routes.tsx okruzimo sa ProtectedRoute 

const ProtectedRoute = ({children} : Props) => {
    const location = useLocation(); 
    const { isLoggedIn} = useAuth(); // Moze da pristupi Context, jer u App.tsx je children route of <App> tj u <Outlet>, a <outlet> je unutar <UserProvider> u App.tsx
    return (
        isLoggedIn() ? <>{children}</> : <Navigate to="/login"  state={{ from: location}} replace/> 
    )
}
// Navigira nas na login ako pokusam da kliknem na nesto unutar ProtectedROute (pogledaj Routes.tsx) ako pre toga nisam login
export default ProtectedRoute;