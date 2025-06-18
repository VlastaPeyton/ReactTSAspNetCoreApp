import axios from "axios";
import { CommentPost } from "../Models/CommentPost";
import { handleError } from "../ErrorHandler/ErrorHandler";
import { CommentGetFromBackend } from "../Models/CommentGetFromBackend";
import apiBackendWithJWT from "../Axios/AxiosWithJWTForBackend"; 

//const apiEndpoint = "https://localhost:7045/api/comment/"; // Mogo sam i HTTP, jer u .NET Program.cs ima onaj HttpsRedirection da ako gadjam http://localhost:5110/api/ da odma me baci na https
// Koristim iz .env, jer bolja praksa nego da hardcodujem API u kodu 
const apiEndpoint = `${process.env.REACT_APP_BASE_BACKEND_API}comment/`;

// Axios koristim umesto Fetch, jer bolji i more user-friendly than Fetch. 

// For StockComment.tsx. 
export const commentPostAPI = async (title: string, content: string, symbol: string) => {
    // try-catch zbog axios. Try se izvrsi ako StatusCode=2XX from backend. Catch se izvrsi ako StatusCode!=2XX from backend.
    try{ 
        // Potreban je JWT poslati iz Frontend to CommentController Create Endpoint, jer on ima User.GetUserName koja zahteva JWT da se posalje from Frontend. Ovaj Endpoint, u .NET i da nema [Authorize], zbog User.GetUserName zahteva JWT.
        const token = localStorage.getItem("token"); // Jer u useAuthContext sam ga upisao u localStorage, pa pristup tome moze kroz sve fajlove u projektu.
        // Moram navesti <CommentPost> kao POTENCIJALNI return ako backend Endpoint ne vrati error, jer uspesan CommentController Create metod vraca CommentDTO objekat sa poljima navedenim u CommentPost - objasnjeno u CommentPost.
        const response = await axios.post<CommentPost>(apiEndpoint + `${symbol}`, 
            // Mora `${symbol}, jer u https://localhost:7045/api/comment/{symbol} Endpoint napisano da symbol kroz URL se prosledjuje u Request.
            // Body of POST request jer ovaj Endpoint zahteva, pored symbol in URL, Title i Content ( tj CreateCommentRequestDTO) kroz Request Body.
            {
            Title: title,
            Content: content
            // Moraju ovaj redosled i imena polja kao u CreateCommentRequestDTO.
            },
            // Header of request jer "https://localhost:7045/api/comment/{symbol} u .NET ima ili [Authenticate] ili/i User.GetUserName(), pa zahteva JWT
            {
            headers:{
                Authorization: `Bearer ${token}`
                }
            }
        )

        return response; // type = AxiosResponse<CommentPost> ili undefined ako CommentController Create Endpoint vrati gresku jer onda u catch block ispod idem.
                         // Zbog ovakvog full response (AxiosResponse<CommentPost>) u StockComment moram da uradim response.data ako zelim da dohvatim payload.
                         
    } catch (error){
        handleError(error);
    }
    // Nema return u catch blok, pa ako error, response=undefined, pa u StockComment moramo da proverimo da li je result non-undefined pomocu if (result) ili result? 
    // Ovde nisam koristio apiBackendWithJWT jer ocu da pokazem kako se radi bez njega. 
}

// For StockComment.tsx
export const commentsGetAPI = async (symbol: string) => {
    // try-catch zbog axios. Try se izvrsi ako StatusCode=2XX from backend. Catch se izvrsi ako StatusCode!=2XX from backend.
    try{ 
        // Potreban je JWT poslati iz Frontend to CommentController GetAll Endpoint. Iako on nema User.GetUserName koja zahteva JWT da se posalje from Frontend. Ovaj Endpoint, u .NET da nema [Authorize], ne bih morao slati JWT iz Frontend.
        const token = localStorage.getItem("token"); // Jer u useAuthContext sam ga upisao u localStorage, pa pristup tome moze kroz sve fajlove u projektu.
        // Moram navesti <CommentGetFromBackend[]> kao POTENCIJALNI return ako backend ne vrati error, jer uspesan CommentController GetAll metod u vraca commentDTOs niz ciji element(CommentDTO) ima polja navedena u CommentGetFromBackend - objasnjeno u CommentGetFromBackend.
        // Da mi treba samo jedan element iz ovog Endpoint, moram <CommentGetFromBackend[]>, pa onda response.data[i] za zeljeni element.

        // Ovde cu, mada bih trebao u svakoj funkciji ovog fajla koja zahteva JWT, da koristim apiBackendWithJWT kako ne bih rucno pisao Authorization header svaki put
        // const response = await axios.get<CommentGetFromBackend[]>(apiEndpoint + `?Symbol=${symbol}&IsDescending=true`,
        //     // U Axios GET Request, samo Header moze, a Body ne moze i zato ovaj Endpoint u .NET ima [FromQuery], a ne [FromBody]
        //     // Header of request jer " GET https://locahost:7045/api/comment/" u .NET ima ili [Authenticate] ili/i User.GetUserName(), pa zahteva JWT
        //     {
        //     headers:{
        //         Authorization: `Bearer ${token}`
        //         }
        //     }
        // )

        const response = await apiBackendWithJWT.get<CommentGetFromBackend[]>(apiEndpoint + `?Symbol=${symbol}&IsDescending=true`,
            // U Axios GET Request, samo Header moze, a Body ne moze i zato ovaj Endpoint u .NET ima [FromQuery], a ne [FromBody]
            // Header of request jer " GET https://locahost:7045/api/comment/" u .NET ima ili [Authenticate] ili/i User.GetUserName(), pa zahteva JWT koji automatski je ubacen putem apiBackendWithJWT
        )

        return response; // type = AxiosResponse<CommentGetFromBackend[]> ili undefined ako CommentController GetAll Endpoint vrati gresku jer onda u catch blok ispod idem.
                         // Zbog ovakvog full response (AxiosResponse<CommentGetFromBackend[]>) u StockComment moram da uradim response.data ako zelim da dohvatim payload.

    } catch (error){
        handleError(error);
    }
    // Nema return u catch blok, pa ako error, response=undefined, pa u StockComment moramo da proverimo da li je result non-undefined pomocu if (result) ili result? 

}

// Poredi ovo sa api.tsx jer tamo se salje Request za Public API sa neta i drugacije je malo.