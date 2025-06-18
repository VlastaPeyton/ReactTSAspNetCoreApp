import axios from "axios";
import { handleError } from "../ErrorHandler/ErrorHandler";
import { UserProfileToken } from "../Models/UserProfileToken";

// const apiEndpoint = "https://localhost:7045/api/"; // Mogo sam i HTTP, jer u .NET Program.cs ima onaj HttpsRedirection, pa ako gadjam http://localhost:5110/api/ da odma prebaci na https.
// Koristim iz .env, jer bolja praksa nego da hardcodujem API u kodu.
const apiEndpoint = process.env.REACT_APP_BASE_BACKEND_API;

// For useAuthContext.tsx
export const loginAPI = async (username: string, password: string) => {
    // try-catch zbog axios. Try se izvrsi ako StatusCode=2XX from backend. Catch se izvrsi ako StatusCode!=2XX from backend.
    try{
        // Nije potreban JWT poslati u backend Login method, jer on ne zahteva JWT.
        // Moram navesti <UserProfileToken> kao POTENCIJALNI return ako backend ne vrati error, jer uspesan Login method u AccountController u .NET vraca objekat sa poljima navedenim u UserProfileToken - objasnjeno u  UserProfileToken.
        const response = await axios.post<UserProfileToken>(apiEndpoint + "account/login", {
            // Body of Request, jer AccountController Login metod zahteva prosledjivanje parametara kroz Request Body.
            Username: username,
            Password: password
            // Ovo je redosled i imena argumenata za LoginDTO u Login endpoint u .NET i mora da ovde ispratim to 
        });
        
        return response; // type = AxiosResponse<UserProfileToken> ili undefined ako Login method backend vrati gresku(BadRequest, Unauthorized ili nepoznati server error koji nije def), jer onda idem u catch
                         // Zbog full response ovako, u useAuthContext.tsx moracu result.data da dohvatim payload.

    } catch (error){ // Ako Login method u backend vrati BadRequest, Unauthorized ili nepoznati server error, ulazim u catch, a response=undefined u try block 
        handleError(error);
    }
    // Nema return u catch, pa ako error bude, response=undefined i zato u loginUser radim onaj veliki if (ili proverim sa ?). U api.tsx metodama isti slucaj je ko ovde, ali u npr CompanyProfile.tsx ne radim veliki if, vec result?. 
}

// For useAuthContext.tsx
export const registerAPI = async (email: string, username: string, password: string) => {
    // try-catch zbog axios. Try se izvrsi ako StatusCode=2XX from backend. Catch se izvrsi ako StatusCode!=2XX from backend.
    try{
        // Nije potreban JWT poslati u backend Register method, jer on ne zahteva JWT.
        // Moram navesti <UserProfileToken> kao POTENCIJALNI return ako backend ne vrati error, jer uspesan Register method u AccountController u .NET vraca objekat sa poljima navedenim u UserProfileToken - objasnjeno u UserProfileToken.
        const response = await axios.post<UserProfileToken>(apiEndpoint + "account/register", {
            // Body of Request, jer AccountController Register metod zahteva prosledjivanje parametara kroz Request Body.
            UserName: username,
            EmailAddress: email,
            Password: password
            // Ovo je redosled i imena argumenata za RegisterDTO u Register endpoint u .NET i mora da ovde ispratim to
        });
        
        return response; // type = AxiosResponse<UserProfileToken> ili undefined ako Register method backend vrati gresku(BadRequest, Unauthorized ili nepoznati server error koji nije def)
                         // Zbog full response ovako, u useAuthContext.tsx moracu result.data da dohvatim payload.

    } catch (error){ // Ako Register method u backend vrati BadRequest, Unauthorized ili nepoznati server error, ulazim u catch, a response=undefined u try block 
        handleError(error);
    }
    // Nema return u catch, pa ako error bude, response=undefined i zato u registerUser radim onaj veliki if (ili proverim sa ?). U api.tsx metodama isti slucaj je ko ovde, ali u npr CompanyProfile.tsx ne radim veliki if, vec result?. 

}

// Poredi ovo sa api.tsx jer tamo Request saljem na Public API sa neta i drugacije je malo.