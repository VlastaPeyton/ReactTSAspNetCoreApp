import { createContext } from "react";
import { UserProfile } from "../Models/UserProfile";
import { Children, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { registerAPI, loginAPI } from "../Services/AuthService";
import { toast } from "react-toastify";
import React from "react";
import axios from "axios";

// Context je global state which allows me to share data across components (inside <UserProvider> in App.tsx) without passing props manually to them 

type UserContextType = {
    user: UserProfile | null;
    token: string | null; // When you are NOT log in this is NULL
    registerUser: (email: string, username: string, password: string) => void; // Register metoda u .NET zahteva ove parametre
    loginUser: (username: string, password: string) => void; // Login metoda u .NET zahteva ove parametre 
    logout: () => void;
    isLoggedIn: () => boolean;
    // Ova imena da se poklope sa imenima u UserProvider
}

type Props = {
    children: React.ReactNode
} // Jer u App.tsx unutar <UserProvider> bice <Navbar/>, <Outlet />, <LoginPage /> ... i onda je su children Components

const UserContext = createContext<UserContextType>({} as UserContextType); // Ovo u zagradi mora zbog TS da se ne buni
// createContext create empty box that holds shared data onih Components koje ce u App.tsx bici unutar <UserProvider> i </UserProvider> 

// Ovaj Component mounts on app start up as it is placed in App.tsx
export const UserProvider = ({ children} : Props) => {
    // childer se odnosi na children u Props 

    const  navigate = useNavigate(); 
    const [token, setToken] = useState<string | null>(null); // null ako se nismo logovali jos 
    const [user, setUser] = useState<UserProfile | null>(null); // null ako se nismo logovali jos
    const [isReady, setIsReady] = useState<boolean>(false);

    useEffect(() => {
        const user = localStorage.getItem("user"); 
        const token = localStorage.getItem("token"); 
        // Ako user i token nisu null (svaki put nakon very first time login) onda izvrsi ovo a to se desi samo ako smo ostali ulogovani i startovali app opet 
        if (user && token) { 
            setUser(JSON.parse(user)); // JSON.parse jer ispod je skladisteno kao JSON.Stringify(user) u localStorages
            setToken(token); // jer ispod je skladisteno kao string u localStorage
            //axios.defaults.headers.common["Authorization"] = "Bearer" + token; // Za svaki Axios request made to .NET backend, jer mora tako ali sam izbrisao jer nesto me zezalo, pa sam u svakom api Request dodao rucno token
        }               
        setIsReady(true); 
    }, []); /* Ne sme da bude [user, token], jer setUser/setToken je unutar useEffect i onda ce upasti infinite loop 
             useEffect zato pokrece se samo kad UserProvider is mounted on app start up */

    const registerUser = async (email: string, username: string, password: string) => {
        // Kao da sam psiao try-catch 
        await registerAPI(email, username, password).then( (result) => { // result je oblika UserProfileToken koji ima username, email i token polja
            if (result) { // if not null 
                localStorage.setItem("token", result?.data.token); // Skladisteno kao string 
                /* result? jer registerAPI tj Register metoda u .NET moze da vrati gresku (StatusCode 500), pa da proverim, jer onda je undefined return type
                   result?.data jer registerAPI vraca AxiosResponse<UserProfileToken>, a nas payload = data .
                   Koristim localStorage, jer to je in Browser memory i onda 
                */
                // user mora imati ISTA imena polja kao UserProfile inace nece hteti setUser(user) zbog useState<UserProfile...>
                const user = {
                    userName: result?.data.userName,
                    emailAddress: result?.data.emailAddress
                }
                localStorage.setItem("user", JSON.stringify(user)); // Jer localStorage radi samo sa string pa moram UserProfile da pretvorim u string
                setToken(result?.data.token!);
                setUser(user!);
                toast.success("Login successfull");
                navigate("/search"); // Baca nas na search page
            }
        }).catch((err) => toast.warn("Server errro")); 
    }

    const loginUser = async (username: string, password: string) => {
        // Kao da sam psiao try-catch 
        await loginAPI(username, password).then((result) => { // result je oblika UserProfileToken koji ima username, email i token polja
            if (result) { // if not null 
                localStorage.setItem("token", result?.data.token); 
                /* result? jer registerAPI tj Register metoda u .NET moze da vrati gresku (StatusCode 500), pa da proverim, jer onda je undefined return type
                   result?.data jer registerAPI vraca AxiosResponse<UserProfileToken>, a nas payload = data 
                */
                // user mora imati ISTA imena polja kao UserProfile inace nece hteti setUser(user) zbog useState<UserProfile...>
                const user = {
                    userName: result?.data.userName,
                    emailAddress: result?.data.emailAddress
                }
                localStorage.setItem("user", JSON.stringify(user)); // Jer localStorage radi samo sa JSON
                setToken(result?.data.token!);
                setUser(user!);
                toast.success("Login successfull");
                navigate("/search"); // Baca nas na SearchPage.
            }
        }).catch((err) => toast.warn("Server errro")); 
    }

    const isLoggedIn = () => {
        return !!user; // Bolje ovo nego if user == null, jer ovim pokrivam uslove da user nije null/undefined/false/0... 
                       // Mada moj user ima UserProfile | null, pa moze if user !== null 
    }

    const logout = () => {
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        setUser(null);
        setToken("");
        navigate("/"); // Baca me na homepage
    }

    return (
        /* Mora UserContext.Provider 
         UserContext.Provider allows you to share data (loginUser, user, token, logout, isLoggedIn, registerUser) between components without having to manually pass props down the component tree. 
        Ovo se odnosi u App.tsx na children Components koje su unutar <UserProvider>. 
         UserContext = createContext<UserContextType>, a UserContextType ima loginUser, user, token, logout, isLoggedIn, registerUser polja i zato moramo u value sve da ih navedem. 
         isReady ? children : null znaci da ako isReady=true renderovace  App.tsx children Components unutar <UserProvider>, ali te Components(Navbar, Outler, LoginPage...) samo preko useAuth mogu da pristupe to loginUser, user, token, logout, isLoggedIn, registerUser  */
        <UserContext.Provider value={{ loginUser, user, token, logout, isLoggedIn, registerUser}}> {isReady ? children : null}</UserContext.Provider>
        // Prosledim vrednosti za polja u UserContextType zbog UserContext = createContext<UserContextType> i onda mogu preko useAuth custom Hook da pristupim ovome bez ponavljanja koda u Componentama unutar <UserProvider> i </UserProvider> u App.tsx
    )
}

// Custom Hook jer koristi bar 1 built-in Hook (useContext). Gives access to everything stored in UserContext to children components ako navedem npr "const {loginUser} = useAuth()" in child Component
export const useAuth = () => React.useContext(UserContext); 