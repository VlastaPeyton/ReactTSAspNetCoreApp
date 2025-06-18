import axios from "axios";
import { PortfolioGetFromBackend } from "../Models/PortfolioGetFromBackend";
import { handleError } from "../ErrorHandler/ErrorHandler";
import { PortfolioAddDelete } from "../Models/PortfolioPost";
import apiBackendWithJWT from "../Axios/AxiosWithJWTForBackend"; 

//const apiEndpoint = "https://localhost:7045/api/portfolio/"; // Mogo sam i HTTP, jer u .NET Program.cs ima onaj HttpsRedirection da ako gadjam http://localhost:5110/api/ da odma me baci na https
// Koristim iz .env, jer bolja praksa nego da hardcodujem API u kodu 
const apiEndpoint = `${process.env.REACT_APP_BASE_BACKEND_API}portfolio/`;

export const portfolioAddApi = async (symbol: string) => {
    try{
        // Potreban je JWT, jer sve osim Login i Register u .NET zahteva Authnetication jer ti Endpoints imaju [Authenticate] tj onu metodu User.GetUserName koja zahteva JWT 
        const token = localStorage.getItem("token"); // Jer u Context sam ga upisao u localStorage
        // Odgovor u AddPortfolio Endpoint je Created() i to fakticki bez oblika ali mora stojati neki oblik kao return type
        const response = await axios.post<PortfolioAddDelete>(apiEndpoint + `?symbol=${symbol}`, {}, 
            {
            // Nema body zbog Endpoint definicije, ali header mora zbog JWT, mogo sam i bez {} za body, vecs samo da preskocim taj argument
            headers:{
                Authorization: `Bearer ${token}`
                }
            });
        return response; 
        
    } catch (error){
        handleError(error);
    }
    // Ovde nisam koristio apiBackendWithJWT iako bih trebao, jer ocu da vidim kako se rucno salje Authorization Header 
}

export const portfolioDeleteApi = async (symbol: string) => {
    try{
        // Potreban je JWT, jer sve osim Login i Register u .NET zahteva Authnetication jer ti Endpoints imaju [Authenticate] tj onu metodu User.GetUserName koja zahteva JWT 
        const token = localStorage.getItem("token"); // Jer u Context sam ga upisao u localStorage
        // Odgovor u DeletePortfolio Endpoint je Ok() i to fakticki bez oblika ali mora stojati neki oblik ovde kao return type
        // const result = await axios.delete<PortfolioAddDelete>(apiEndpoint + `?symbol=${symbol}`, {
        //     // Nema body zbog Endpoint definicije, ali header mora zbog JWT. Mogo sam i bez {} za body, vec samo da preskocim taj argument
        //     headers:{
        //         Authorization: `Bearer ${token}`
        //         }
        //     });
        
        // Ovde cu, mada bih trebao u svakoj funkciji ovog fajla koja zahteva JWT, da koristim apiBackendWithJWT kako ne bih rucno pisao Authorization header svaki put
        const result = await apiBackendWithJWT.delete<PortfolioAddDelete>(apiEndpoint + `?symbol=${symbol}`)

        return result; 
        
    } catch (error){
        handleError(error);
    }
}

export const portfolioGetApi = async () => {
    try{
        // Potreban je JWT, jer sve osim Login i Register u .NET zahteva Authnetication jer ti Endpoints imaju [Authenticate] tj onu metodu User.GetUserName koja zahteva JWT 
        const token = localStorage.getItem("token"); // Jer u Context sam ga upisao u localStorage
        // Odgovor u GetUserPortfolios je lista of Stock.cs koja se automatski preslika u PortfolioGetFromBackend listu jer su "ista" imena polja u oba objekta
        // const result = await axios.get<PortfolioGetFromBackend[]>(apiEndpoint, {
        //     //  Nema body zbog GET (i Endpoint definicije koja mora znati da AXIOS GET nema body), ali header mora zbog JWT
        //      headers:{
        //         Authorization: `Bearer ${token}`
        //         }
        // }); 
        // Ne koristim rucni unos Authorization Headera jer bolja praksa je ovako 
        const result = await apiBackendWithJWT.get<PortfolioGetFromBackend[]>(apiEndpoint);
        return result; 
        
    } catch (error){
        handleError(error);
    }
}