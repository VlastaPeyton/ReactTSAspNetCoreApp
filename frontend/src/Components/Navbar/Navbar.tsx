import React from 'react'
import logo from "./logo.jpg";
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../Context/useAuthContext';

type Props = {}

// Svaki Component kodiram pomocu tailwindcss. Navbar, zbog prisustva u App.tsx i ne prisustva u Routes.tsx , bice prisutan na svakoj stranici. 
const Navbar = (props: Props) => {
    const {isLoggedIn, user, logout} = useAuth();  // Moze da pristupi Context, jer u App.tsx je children route of <App> tj u <Outlet>, a <outlet> je unutar <UserProvider> u App.tsx

  return (
    <nav className="relative container mx-auto p-6">
        <div className="flex items-center justify-between">
            <div className="flex items-center space-x-20">
                <Link to="/">
                <img src={logo} alt="" />  {/* Klik na sliku da odvede u path="http://localhost:3000/" tj u HomePage.tsx */}
                </Link>
                <div className="hidden font-bold lg:flex">
                    <Link to="/search" className="text-black hover:text-darkBlue">
                        Search            {/* Klik na Search da odvede u path="http://locahlost:3000/search" tj u SearchPage.tsx*/}
                    </Link>
                </div>
            </div>
            {isLoggedIn() ? ( <div className="hidden lg:flex items-center space-x-6 text-back">
                                <button
                                    onClick={logout}
                                    className="hover:text-darkBlue"
                                >
                                    Logout
                                </button>
                              </div>) : 
                                ( 
                                <div className="hidden lg:flex items-center space-x-6 text-back">
                                    <Link to="/login" className="hover:text-darkBlue">Login</Link>
                                        <Link to="/register" className="px-8 pv-3 font-hold rounded text-white bg-lightGreen">Signup</Link>
                                 </div>
                                )}

        </div>
    </nav>
  )
};

export default Navbar;