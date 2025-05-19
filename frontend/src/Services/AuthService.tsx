import axios from "axios";
import { handleError } from "../ErrorHandler/ErrorHandler";
import { UserProfileToken } from "../Models/UserProfileToken";
import { useNavigate } from "react-router-dom";

const apiEndpoint = "https://localhost:7045/api/"; // Mogo sam i HTTP, jer u .NET Program.cs ima onaj HttpsRedirection da ako gadjam http://localhost:5110/api/ da odma me baci na https

export const loginAPI = async (username: string, password: string) => {
    // try-catch zbog axios
    try{
        // Moram navesti <UserProfileToken>, jer Login method u AccountController u .NET vraca objekat sa ova 3 polja navedena u UserProfileToken pa automatski ih mapira zbog "istih" imena 
        const response = await axios.post<UserProfileToken>(apiEndpoint + "account/login", {
            // Body of request
            Username: username,
            Password: password
            // Ovo je redosled i imena argumenata za LoginDTO u Login endpoint u .NET i mora da ovde ispratim to 
        });
        
        return response; // type = AxiosResponse<UserProfileToken> 

    } catch (error){
        handleError(error);
    }
}

export const registerAPI = async (email: string, username: string, password: string) => {
    // try-catch zbog axios
    try{
        // Moram navesti <UserProfileToken>, jer Register method u AccountController u .NET vraca objekat sa 3 polja navedena u UserProfileToken pa automatski ih mapira zbog "istih" imena
        const response = await axios.post<UserProfileToken>(apiEndpoint + "account/register", {
            // Body of request
            UserName: username,
            EmailAddress: email,
            Password: password
            // Ovo je redosled  i imena argumenata za RegisterDTO tj za Register endpoint u .NET i mora da ovde ispratim to
        });
        
        return response; // type = AxiosResponse<UserProfileToken>

    } catch (error){
        handleError(error);
    }
}
