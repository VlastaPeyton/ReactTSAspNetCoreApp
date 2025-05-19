import axios from "axios";
import {toast} from "react-toastify";

export const handleError = (error: any) => {
    if (axios.isAxiosError(error)){
        var err = error.response; // Znam da Axios error ima delove poruke. Ovo korisitm ako backend odgovorio sa StatusCode != 2XX
        if(Array.isArray(err?.data.errors)){
            for(let val of err?.data.errors){ // Koristim "of" da dohvatim objekat, a ne njegov index
                toast.warning(val.description); // npm install react-toastify
            }
        }
        else if (typeof err?.data.errors == 'object'){
            for(let e in err.data.errors){
                toast.warning(err.data.errors[e][0]);
            }
        }
        else if (err?.data){
            toast.warning(err.data);
        }
        else if (err?.status == 401){ // U .NET 7 dana trajanje tokena namesteno
            toast.warning("Please login");
            window.history.pushState({},"LoginPage","/login"); // Change current URL in browser and forces me to hit ENTER to redirect to LoginPage
            /* Ne moze useNavigate, jer to moze samo u React Component or Hook, a ovo je funkcija + handleError pozivam takodje iz funkcija loginAPI i registerAPI. Jedino da 
            iz UserProvider Component tj registerUser i loginUser catch bloka prosledim navigation u loginAPI i registerAPI i onda odatle u handleErro. jer*/
        }
        else if (err){
            toast.warning(err.data);
        }
    }
}