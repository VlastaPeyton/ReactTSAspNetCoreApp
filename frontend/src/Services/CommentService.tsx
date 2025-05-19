import axios from "axios";
import { CommentPost } from "../Models/CommentPost";
import { handleError } from "../ErrorHandler/ErrorHandler";
import { CommentGetFromBackend } from "../Models/CommentGetFromBackend";

const apiEndpoint = "https://localhost:7045/api/comment/"; // Mogo sam i HTTP, jer u .NET Program.cs ima onaj HttpsRedirection da ako gadjam http://localhost:5110/api/ da odma me baci na https

export const commentPostAPI = async (title: string, content: string, symbol: string) => {
    // try-catch zbog axios
    try{ 
        // Potreban je JWT, jer sve osim Login i Register u .NET zahteva Authnetication jer ti Endpoints imaju [Authenticate] tj onu metodu User.GetUserName koja zahteva JWT 
        const token = localStorage.getItem("token"); // Jer u Context sam ga upisao u localStorage
        // Odgovor u https://localhost:7045/api/comment/{symbol} Endpoint u .NET vraca svasta, ali mene zanima samo Title i Content i zato CommentPost je bas ovakav
        const response = await axios.post<CommentPost>(apiEndpoint + `${symbol}`, 
            // Mora `${symbol}, jer u https://localhost:7045/api/comment/{symbol} Endpoint napisano da symbol kroz URL se prosledjuje.
            // Body of request jer ovaj Endpoint u .NET zahteva,pored symbol in URL, i Title i Content from Body (CreateCommentRequestDTO)
            {
            // <CommentPost> is return type 
            Title: title,
            Content: content
            // Moraju ovaj redosled i imena polja kao u CreateCommentRequestDTO
            },
        // Header of request jer "https://localhost:7045/api/comment/{symbol} u .NET ima [Authenticate] tj ima User.GetUserName() koja zahteva JWT
            {
            headers:{
                Authorization: `Bearer ${token}`
                }
            }
        )

        return response; // type = AxiosResponse<CommentPost>

    } catch (error){
        handleError(error);
    }
}

export const commentsGetAPI = async (symbol: string) => {
    // try-catch zbog axios
    try{ 
        // Potreban je JWT, jer sve osim Login i Register u .NET zahteva Authnetication jer ti Endpoints imaju [Authenticate] tj onu metodu User.GetUserName koja zahteva JWT 
        const token = localStorage.getItem("token"); // Jer u Context sam ga upisao u localStorage
        // Odgovor u https://localhost:7045/api/comment/ GetAll Endpoint u .NET vraca listu of CommentDTOs i zato <CommentGetFromBackend[]>
        const response = await axios.get<CommentGetFromBackend[]>(apiEndpoint + `?Symbol=${symbol}&IsDescending=true`,
           // Samo Header moze u Axios GET, a body ne moze i zato ovaj Endpoint u .NET ima [FromQuery], a ne [FromBody]
            {
            headers:{
                Authorization: `Bearer ${token}`
                }
            }
        )

        return response; // type = AxiosResponse<CommentGetFromBackend[]>

    } catch (error){
        handleError(error);
    }
}
