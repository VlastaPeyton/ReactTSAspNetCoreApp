import axios from "axios";
import { getInMemoryToken, setInMemoryToken } from "../Context/useAuthContext";
import {jwtDecode} from "jwt-decode";
import { toast } from "react-toastify";
 
// Create custom axios instance 
const apiBackendWithJWT = axios.create({
    baseURL: process.env.REACT_APP_BASE_BACKEND_API, // Bolja praksa, nego da hardcodujem vrednost https://localhost:7045/api/.  Mora baseURL se bas ovako zvati u axios.create
    withCredentials: true // Required to send HttpOnly Refresh Token cookie to BE
})

/* Ako je vise requests to protected Endpoint poslato, a Acces Token is about to expire (jer svi request koriste isti token), 
osiguravam da samo jedan Request pozove RefreshToken Endpoint, dok ostalih 9 cekaju (subscribeTokenRefresh) da FE dobije novi JWT i kad ga dobiju, nastavljaju
sa novim tokenom. */
type TokenCallback = (token: string) => void; // U React, funkcija moze biti type. Ovo je samo potpis funkcije, a telo moze imati kako zelim
let isRefreshing = false;
let refreshSubscribers: TokenCallback[] = [];
function subscribeTokenRefresh(callback: TokenCallback): void {
    refreshSubscribers.push(callback); // push add one or more elems(TokenCallback functions) to end of array
}
function onRefreshed(newToken: string): void {
    refreshSubscribers.forEach(callback => callback(newToken)); // Svaku funkciju iz niza poziva sa newToken argumentom
    refreshSubscribers = [];
}

// Axios.post expected return type kao sto sam radio u Services folderu
export type NewAccessToken = {
  accessToken: string; // Jer BE vraca Ok(new {accessToken=...}), pa mora isto ime argumenta
};

// Function to refresh the access token (which is gonna be called by only first Request made to protected Endpoint)
async function refreshAccessToken() {
    try {
         // Use the SAME axios instance to ensure cookies are sent properly
        const response = await apiBackendWithJWT.post<NewAccessToken>(
            'account/refresh-token', // Relative URL since we have baseURL
            {}, // Empty body jer RefreshToken endpoint ne prima argumente
        );

        const newToken = response.data.accessToken; // jer NewAccessToken ima ovo polje
        console.log(`new toke ${newToken}`);
        setInMemoryToken(newToken); 
        toast.success("New token set");
        return newToken;

    } catch (error) {
        console.warn("Token refresh failed", error);  
        toast.warn("Nije refreshovan token");
        setInMemoryToken(null);
        return null;
    }
}



// Add Request Axios iterceptor kako ne bih pri svakom Backend API pozivu to protected Endpoint(koji u .NET ima [Authenticate]) prosledjivao JWT u Authorization header rucno. 
// apiBackendWithJWT.interceptors.request.use(
//     /* Arrow function called before Request is sent. Config is automatically passed to arrow function by the Axios interceptor and it's the request configuration object that Axios creates before sending the HTTP request.
//     Ovime modifikujem config objekat, pre nego sto Axios i posalje Request to BE.*/
//     (config) => {
//         //const token = localStorage.getItem("token"); - U Context loginUser/registerUser funkciji sam ga setovao. Ne koristim vise, jer tokenInMemory koristim posto je bezbednije.
//         const token = getInMemoryToken(); // U Context, token nije skladisten u localStorage vise, jer nesigurno je, vec je u in-memory variable
//         if (token){
//             config.headers.Authorization = `Bearer ${token}`; // Mora sa Bearer zbog .NET ja msm
//         }
//         return config;
//     },
//     // Arrow function called if error is present, while modifying config object, before Request is sent. Error is automatically passed to arrow function by the Axios interceptor
//     (error) =>{
//         return Promise.reject(error);
//     }
// )
// Ovo iznad je stavljeno u ovo ispod 

// Ako imam 10 request i svima naravno isti token istice uskoro, i svaki request ide kroz interceptor
apiBackendWithJWT.interceptors.request.use(
    async (config) => {
        let token = getInMemoryToken();
        if (token){
            try{
                const decoded = jwtDecode(token);
                const now = Date.now() / 1000;
                if (decoded.exp && typeof decoded.exp === 'number'){
                    //const timeUntilExpiry = decoded.exp ? decoded.exp - now : null; // Mora ovako jer decoded.exp moze biti undefined
                    const timeUntilExpiry = decoded.exp - now; 
                    console.log(`Token expiers in ${timeUntilExpiry}`);

                    if( timeUntilExpiry <= 30){
                        // Kada prvi Request naidje, isRefreshing===false, i ide u else, ali ostali request udju ovde jer je njima isRefreshing===true
                        if (isRefreshing){
                            // Ostali Requests cekaju until resolve(config) (zbog Promise), dok se onRefreshed(newToken) ne zavrsi i tek tada nastavljaju sa azuriranim tokenom
                            // Promise is same as Task in ASP.NET Core
                            return new Promise((resolve, reject) => {
                                // i svaki od njih pozove subscribeTokenRefresh kome prosledi telo funkcije koju jos uvek nije aktivirao
                                subscribeTokenRefresh((newToken: string) =>{ //newToken iz onRefreshed(newToken)
                                    config.headers.Authorization = `Bearer ${newToken}`;
                                    resolve(config); // Ublocks paused Request and sends it with newToken
                                });
                            });
                        }
                        else{
                            // Only first request enter here and start refresh process 
                            isRefreshing = true;
                            try{
                                const newToken = await refreshAccessToken();
                                if (newToken){
                                    token = newToken;
                                    onRefreshed(newToken); // Notify waiting requests tako sto svaki Request je upisao svoju funkciju u subscribeTokenRefresh i svaka je upravo pozvana ovom komandom
                                }
                                else{
                                    // Refresh failed, let reqeust proceed with old expired token as Response interceptor will handle 401
                                }
                            } catch(error){
                                toast.warn("Failed to refresh token");
                                // Token refresh failed, let the request proceed with old token
                                // The response interceptor will handle the 401 error
                            } finally{
                                isRefreshing = false;
                            }
                        }
                    }
                }
            } catch (error){
                toast.warn("error decoding token");
                setInMemoryToken(null);
            }
        }
        // Set authorization header with current token (could be refreshed or original)
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }

        return config;
    },
    // Arrow function called if error is present, while modifying config object, before Request is sent. Error is automatically passed to arrow function by the Axios interceptor
    (error) =>{
        return Promise.reject(error);
    }

)

// Add Response Interceptor to handle 401 errors as fallback jer moze se desi da FE nije poslao Request (user nije kliknuo nista) i token expired i onda request interceptor izbacice gresku jer protected Endpoints ne mogu pozvati bez tokena
// Npr ako sam poslao 10 Requests, a token vec istekao, svih 10 ce da dobije 401 status pre toga, ali samo prvi ce da pozove refreshAccessToken,a ostalih 9 cekaju,
apiBackendWithJWT.interceptors.response.use(
    (response) => response, // If Response is success
    // If Response is 401 
    async (error) => {
        const originalRequest = error.config;
        
        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;
            
            if (isRefreshing) {
                // If already refreshing (1st Request activated refreshAccessToken), pause all waiting requests until the new token is set
                return new Promise((resolve, reject) => {
                    subscribeTokenRefresh((newToken: string) => {
                        originalRequest.headers.Authorization = `Bearer ${newToken}`;
                        resolve(apiBackendWithJWT(originalRequest)); // Unblock paused reqeust and send it again with new token
                    });
                });
            }
            // If no refresh in progress
            isRefreshing = true;
            try {
                const newToken = await refreshAccessToken();
                if (newToken) {
                    originalRequest.headers.Authorization = `Bearer ${newToken}`;
                    onRefreshed(newToken); 
                    return apiBackendWithJWT(originalRequest); // Retry the original request
                }
            } catch (refreshError) {
                toast.warn("token refresh failed");
                // Redirect to login handled in refreshAccessToken function
            } finally {
                isRefreshing = false;
            }
        }
        
        return Promise.reject(error);
    }
);

// Response interceptor for 401 as fallback for request interceptor 
export default apiBackendWithJWT; 