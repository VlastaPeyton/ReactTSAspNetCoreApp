import axios from "axios";
import {toast} from "react-toastify";

// Pogledaj na netu kako izgleda Axios Error object i koji deo zavisi od backenda u .NET i bice ti jasno. 
// Pogledaj u AccountController Create i GetAll i CommentController Login i Register metode i shvatices zasto je ovako ovde napisano.
// Ova klasa handluje samo errors from .NET backend, a ne i greske iz FinancialModelingPrep jer tamo ne znam koje vreste errora moze da se vrate i onda ih u api.tsx hanldujem kao obican axios error.

export const handleError = (error: any) => {
    if (axios.isAxiosError(error)){
        // Znam da Axios error ima razne delove poruke(response, data...).
        var err = error.response;  // Ovo koristim ako backend odgovorio sa StatusCode != 2XX, jer axios stavi response u response deo greske.
        
        // Check if the response part of error contains an array of validation errors i ovo zavisi od .NET backend ako sam uneo postojeci email/username prilikom Register/Login metode i bude StatusCode!=2XX
        if(Array.isArray(err?.data.errors)){  // data part of error zavisi od backenda sta je poslao server
            // Loop through each validation error
            for(let val of err?.data.errors){ // Koristim "of" da dohvatim objekat, a ne "in" da dohvatim njegov index
                toast.warning(val.description); 
            }
        }

        else if (typeof err?.data.errors == 'object'){  // Ako u .NET Register/Login method bude "if(!ModelState.IsValid) return BadRequest(ModelState);"
            for(let e in err.data.errors){
                toast.warning(err.data.errors[e][0]);
            }
        }
        else if (err?.data){ // U .NET Login method failure ako pogresan username/pasword unesem
            toast.warning(err.data);
        }
        else if (err?.status == 401){ // U .NET je 7 dana trajanje tokena namesteno i ako je isteko token, moram opet se login
            toast.warning("Please login");
            window.history.pushState({},"LoginPage","/login"); // Change current URL in browser and forces me to hit ENTER to redirect to LoginPage ili rucno da kliknem Login u Navbar.
            /* Ne moze useNavigate da nas automatski vrati na LoginPage, jer to moze samo u React Component or React Hook (ne i u funkciji), a ovo je funkcija + handleError pozivam takodje iz funkcija loginAPI i registerAPI. 
            Jedino da iz UserProvider Component tj registerUser i loginUser catch bloka prosledim navigation u loginAPI i registerAPI i onda odatle da prosledim u handleError. */
        }
        else if (err){ // U .NET Register method catch bloku moze doci do ove greske
            toast.warning(err.data);
        }
    }
    else {
        toast.warning("Non Axios error tj unexpected error. Try again. ")
    }
}