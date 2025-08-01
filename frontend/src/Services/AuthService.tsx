import axios from "axios";
import { handleError } from "../ErrorHandler/ErrorHandler";
import { UserProfileToken } from "../Models/UserProfileToken";
import { toast } from "react-toastify";

// const apiEndpoint = "https://localhost:7045/api/"; // Mogo sam i HTTP, jer u .NET Program.cs ima onaj HttpsRedirection, pa ako gadjam http://localhost:5110/api/ da odma prebaci na https.
// Koristim iz .env, jer bolja praksa nego da hardcodujem API u kodu.
const apiEndpoint = process.env.REACT_APP_BASE_BACKEND_API;

/* U BE Endpoints, koje gadjaju ove funkcije pomocu axios, postoji cancellationToken, ali bez =default. Kada sam na LoginPage, koja koristi ove funkcije, i dok kad pokrenem neki Endpoint kliktanjem 
po njoj, ako user navigates away or closes app, BE ce autoamtski, zbog HTTP mehanizma, provaliti da sam prekinuo poziv, i prestace sa radom tako sto ce dodeliti RequestAborted vrednost za cancellationToken.
Da sam u Endpoint imao cancellationToken = default, u slucaju kad user navigates away or closes the app, u axios moram proslediti "signal:controller.signal" ali to nije Header or Body i jos uraditi neki cudni return 
koji ce aktivira controller.abort(). 

Ovde ne koristim axiosWithJWTBackend, jer loginAPI, registerAPI i forgotPasswordAPI i resetPasswordAPI se rade pre nego sto se user login/register i onda ne mogu ni da imam JWT jer backend salje JWT tek nakon user is logged/registered.
*/

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

// For useAuthContext.tsx  - objasnjenje isto kao iznad 
export const forgotPasswordAPI = async (email: string) =>{
    // Try-catch zbog axios
    try{
        // Nije potreban JWT poslati u BE ForgotPassword method, jer ovo se radi kad se jos nismo ni login.
        // Moram navesti <string> kao POTENCIJALNI return ako BE ne vrati error (ako nadje email u bazi), jer uspesan ForgotPassword method u AccountController u .NET vraca Ok("Reset password link is sent to your email") 
        const response = await axios.post<string>(apiEndpoint + "account/forgotpassword", {
            // Najbolje slati email u Request Body, a ne as Query parameter of URL => AccountController ForgotPassword method zahtva [FromBody]
            // Body of Request
            EmailAddress: email
            // Ovo je redosled i imena argumenata za ForgotPasswordDTO u ForgotPassword Endpoint i moram ovode da ispratim to
        });
        
        return response; // type= AxiosResponse<string> ili undefined ako je neocekivana greska, jer ForgotPassword method i u slucaju wrong email nece vrati gresku vec, zbog sigurnosti bice return OK("Reset password link is sent to email") 
                         // Zbog full response ovako, u useAuthContext moracu result.data da bih dohvatio payload.    

    } catch (error) {    // ForgotPassword ne moze da vrati gresku jer zbog sigurnosti i da je neisprvan email BE vrca Ok("Rest password link is sent to email"), ali mozda bude neka neocekivana greska pa ce catch da se izvrsi
        handleError(error);
    }
    // Nema return u catch, pa ako error bude, response = undefinded i zato u forgotPassowr radim if (ili proverim sa ?).
}

// For useAuthContext.tsx - objasnjeje kao iznad
export const resetPasswordAPI = async (newPassword: string, resetPasswordToken: string, email: string) =>{
    try{
        // Nije potreban JWT poslati u BE ResetPassword method, jer ovo se radi kad se nisam ni login jos
        // Moram navesti <string> kao POTENCIJALNI return ako BE ne vrati error 
        const response = await axios.post<string>(apiEndpoint + "account/resetpassword", {
            // Najbolje slati password u Request Body, a ne u Query => ResetPassword metod zahteva [FromBody]
            // Body of Request. 
            NewPassword: newPassword,
            ResetPasswordToken: resetPasswordToken,
            EmailAddress: email
            // Ovo je redosled i imena argumenata za ResetPasswordDTO u ResetPassword Endpoint i moram ovode da ispratim to
        });

        return response;  // ResetPassword sends Ok("If the email exists in our system, the password has been reset."") i da je dobro i da je lose  pa response.status=200

    } catch (error){ // Samo ako ResetPassword endpoint vrati BadRequest response=undefined in try block
        handleError(error); 
    }
}

// Poredi ovo sa api.tsx jer tamo Request saljem na Public API sa neta i drugacije je malo.