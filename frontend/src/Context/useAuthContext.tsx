import { createContext } from "react";
import { UserProfile } from "../Models/UserProfile";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { registerAPI, loginAPI, forgotPasswordAPI } from "../Services/AuthService";
import { toast } from "react-toastify";
import React from "react";

// Context je global state which allows me to share data across components (inside <UserProvider> in App.tsx) without passing props manually to them 

type UserContextType = {
    user: UserProfile | null; 
    token: string | null; // When you are NOT logged in this is NULL
    registerUser: (email: string, username: string, password: string) => void; // Register metoda u .NET zahteva ove parametre
    loginUser: (username: string, password: string) => void;                   // Login metoda u .NET zahteva ove parametre 
    logout: () => void;
    isLoggedIn: () => boolean;
    forgotPassword: (email: string) => void;
    // Ova imena mora da se poklope sa imenima u UserProvider + kod metoda mora da se poklopi redosled argumenata kao u UserProvider.
    // Sve navedeno ovde, moram da posaljem kroz value u UserContext.Provider ispod na dnu koda.
}

type Props = {children: React.ReactNode} // Mora ovako, jer u App.tsx izmedju <UserProvider> i </UserProvider> bice <Navbar/>, <Outlet /> i <ToastContainer />. Isto objasnjenje kao u ProtectedRoute.tsx 
                                         // React.ReactNode je 0,1 ili vise Components koju napisem izmedju <UserProvider> i </UserProvider> u App.tsx. Moze biti vise Components unutar <UserProvider>

const UserContext = createContext<UserContextType>({} as UserContextType); // Ovo u zagradi mora zbog TS da se ne buni. 
// createContext create empty box that holds shared data onih Components (Navbar, Outlet i ToastContainer) koje ce, u App.tsx, biti izmedju <UserProvider> i </UserProvider> 

// This Component mounts on app start up as it is placed in App.tsx
export const UserProvider = ({children} : Props) => {
    // childer se odnosi na children u Props 

    const navigate = useNavigate(); 
    const [token, setToken] = useState<string | null>(null); // null ako se nismo logovali jos 
    const [user, setUser] = useState<UserProfile | null>(null); // null ako se nismo logovali jos
    const [isReady, setIsReady] = useState<boolean>(false);

    useEffect(() => {
        const user = localStorage.getItem("user"); 
        const token = localStorage.getItem("token"); 
        // Ako user i token nisu null (svaki put nakon very first time login) onda izvrsi ovo ispod, a to se desi samo ako smo ostali ulogovani i startovali app opet, jer on 7dana pamti login valjda
        if (user && token) { 
            setUser(JSON.parse(user)); // JSON.parse mora, jer ispod je skladisteno kao JSON.Stringify(user) u localStorage, posto localStorage samo string prihvata.
            setToken(token); // Jer ispod je skladisteno kao string u localStorage, jer localStorage samo string prihvata.
            //axios.defaults.headers.common["Authorization"] = "Bearer" + token; // Za svaki Axios request made to .NET backend, jer mora tako. Ali sam izbrsiao ovu liniju, jer nesto me zezalo, pa sam u svakom api Request dodao rucno token
        }               
        setIsReady(true); 
    }, []); /* Ne sme da bude [user, token], jer setUser/setToken je unutar useEffect i onda ce uci u infinite loop.
             useEffect se pokrece only kad UserProvider is mounted on app start up. */

    const registerUser = async (email: string, username: string, password: string) => {
        // Then-catch je isto kao da sam pisao try-catch. Catch mora ako dodje go greske u registerAPI koji je Frontend.
        await registerAPI(email, username, password).then( (result) => { 
            // result.data je tipa UserProfileToken koji ima userName, emailAddress i token polja, samo ako backend vratio StatusCode=2XX, jer u registerAPI smo onda u try block otisli
            // result je tipa undefined, pa ne moze result.data, ako backend vratio error (StatusCode!=2XX), jer onda u loginAPI u catch blok smo otisli gde nema return 
            if ( result && result.data && typeof result.data.userName === 'string' && typeof result.data.emailAddress === 'string' && typeof result.data.token === 'string') 
                { 
                /* if result is not undefined tj if result is UserProfileToken.
                   if result.data is not undefined tj if result.data is object tipa UserProfileToken.
                   if result.data.userName/emailAddress/token is string jer UserProfileToken ima userName/emailAddress/token:string
                */
                
                localStorage.setItem("token", result.data.token); // Skladisteno kao string, jer localStorage samo string prihvata.
                /* result? - jer result moze biti i undefined ako backend vratio error u registerAPI
                   result?.data - jer registerAPI vraca AxiosResponse<UserProfileToken>, a nas payload(tipa UserProfileToken) je data.
                   result?.data.token/userName/emailAddress - jer payload ima userName, emailAddress i token polja
                   Ovo ove upitnike vise ne koristim, jer sam u if stavio sve uslove.
                   Koristim localStorage, jer to je in Browser memory.

                */
                
                // user mora imati ISTA imena i redosled polja kao UserProfile inace nece hteti setUser(user) zbog useState<UserProfile...>
                const user = {
                    userName: result.data.userName, 
                    emailAddress: result.data.emailAddress 
                }

                localStorage.setItem("user", JSON.stringify(user)); // Moram UserProfile da pretvorim u string, jer localStorage samo string prihvata.
                setToken(result.data.token);  // FE uvek mora da sacuva JWT from BE kako bi u Authorization Header ga slao to BE za Endpoints koji zahtevaju to. React re-renders this component zbog set.
                setUser(user);    // React re-renders this component zbog set.
                // Posto su ovde 2 uzastopna set, react odradi oba, pa tek re-renders component.
                toast.success("Register successfull"); // Prikaze mali pop-up u gornji desni ugao ekrana 
                navigate("/search"); // Baca nas na SearcPage, jer u Routes.tsx definisano da SearchPage je u /search route.
            }
            // Nema else, jer nema potrebe obzirom da u registerAPI imamo handleError u catch. Da to nemam, ovde bih else imao da toast.warning prikaze nesto.

        }).catch((err) => toast.warn("Frontend error in registerAPI")); // Prikaze mali pop-up u gornji desni ugao ekrana ako bude frontend greska u registerAPI (ne u backendu)
    }

    const loginUser = async (username: string, password: string) => {
        // Then-catch isto kao da sam pisao try-catch. Catch se radi ako dodje do greske u loginAPI koji je Frontend.
        await loginAPI(username, password).then((result) => { 
            // result.data je tipa UserProfileToken koji ima userName, emailAddress i token polja, samo ako backend vratio StatusCode=2xx, jer u loginAPI smo u try block otisli
            // result je tipa undefined, pa ne moze result.data, ako backend vratio error (StatusCode!=2XX), jer onda u loginAPI u catch blok smo otisli gde nema return 
            if ( result && result.data && typeof result.data.userName === 'string' && typeof result.data.emailAddress === 'string' && typeof result.data.token === 'string') 
              { 
                /* if result is not undefined tj if result is UserProfileToken.
                   if result.data is not undefined tj if result.data is object tipa UserProfileToken.
                   if result.data.userName/emailAddress/token is string jer UserProfileToken ima userName/emailAddress/token:string
                */
                localStorage.setItem("token", result.data.token); 
                /* result? - jer result moze biti i undefined ako backend vratio error u registerAPI
                   result?.data - jer registerAPI vraca AxiosResponse<UserProfileToken>, a nas payload(tipa UserProfileToken) je data.
                   result?.data.token/userName/emailAddress - jer payload ima userName, emailAddress i token polja
                   Ove upitnike vise ne koristim jer sam u if stavio sve uslove. 
                   Koristim localStorage, jer to je in Browser memory.
                */

                // user mora imati ISTA imena i redosled polja kao UserProfile inace nece hteti setUser(user) zbog useState<UserProfile...>
                const user = {
                    userName: result.data.userName,
                    emailAddress: result.data.emailAddress
                }
                localStorage.setItem("user", JSON.stringify(user)); // Jer localStorage radi samo sa JSON
                setToken(result.data.token); // FE uvek mora da sacuva JWT from BE kako bi u Authorization Header ga slao to BE za Endpoints koji zahtevaju to. React re-renders this component zbog set.
                setUser(user); // React re-renders this component zbog set
                // Posto su ovde 2 uzastopna set, react odradi oba, pa tek re-renders component.
                toast.success("Login successfull");
                navigate("/search"); // Baca nas na SearchPage.
            }
            // Nema else, jer nema potrebe obzirom da u loginAPI imamo handleError u catch. Da to nemam, ovde bih else imao da toast.warning prikaze nesto.

        }).catch((err) => toast.warn("Frontend error in loginAPI"));  // Prikaze mali pop-up u gornji desni ugao ekrana ako bude frontend greska neka u loginAPI (ne u backendu)
    }

    const isLoggedIn = () => {
        return !!user; // Bolje ovo nego "if user === null", jer ovim pokrivam uslove da user nije null/undefined/false/0... 
                       // Mada moj user ima UserProfile | null, pa moze "if user !=== null".
    }

    const logout = () => {
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        setUser(null);
        setToken("");
        navigate("/"); // Baca me na homepage after logout
    }

    // Objasnjenje za ovu funkciju je isto kao za sve iznad. 
    const forgotPassword = async (email: string) => {
        await forgotPasswordAPI(email).then((result) => {
            if (result && typeof result.data === 'string'){
                toast.success("Email postoji u bazi, reset password link je poslat na vasu adresu"); // Ovo samo za Dev sluzi, dok za Prod brise jer necu da me odaje
            }
        }).catch((err) => toast.warn("Frontend error in forgotPasswordAPI"));
    }

    return (
        /* 
         UserContext.Provider allows you to share data (loginUser, user, token, logout, isLoggedIn, registerUser) between components without having to manually pass props down the component tree. 
         Ovo se odnosi u App.tsx na children components (<Navbar/>, <Outlet/> i <ToastContainer/>) koje su izmedju <UserProvider> i </UserProvider>. 
         UserContext = createContext<UserContextType>, a UserContextType ima loginUser, user, token, logout, isLoggedIn, registerUser polja i zato moramo u value sve da ih navedem. 
         isReady ? children : null znaci da ako isReady=true renderovace u App.tsx children components unutar <UserProvider>, a children samo preko useAuth mogu da pristupe to loginUser, user, token, logout, isLoggedIn, registerUser, stoga 
        moram definisati, van UserProvider, useAuth custom Hook.  */
        <UserContext.Provider value={{ loginUser, user, token, logout, isLoggedIn, registerUser, forgotPassword }}> {isReady ? children : null}</UserContext.Provider>
        /* Prosledim vrednosti za polja u UserContextType zbog UserContext = createContext<UserContextType> i onda mogu preko useAuth custom Hook da pristupim ovome bez ponavljanja koda u children componentama izmedju <UserProvider> i </UserProvider> u App.tsx
        {children} je isto kao da napisem  <Component1/> <Component2>... , jer children je React.ReactNode tj 0,1 ili vise Components 
        */
    )
}

// Custom Hook, jer koristi bar 1 built-in Hook (useContext). Gives access to everything stored in UserContext to children components (npr: const {loginUser} = useAuth() in child Component definiciji nekoj).
export const useAuth = () => React.useContext(UserContext); 

// U components, koje su unutar <UserProvider> u App.tsx (<Navbar>, <ToastContainer> i <Outlet> (sve children routes of <App>)), da bih pristupio necemu iz UserContexts, moram bas to ime da navedem (npr: loginUser)