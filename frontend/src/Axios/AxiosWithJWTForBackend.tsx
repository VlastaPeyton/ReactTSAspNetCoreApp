import axios from "axios";

// Create axios instance
const apiBackendWithJWT = axios.create({
    //baseURL: "https://localhost:7045/api/" // Base URL za .NET Backend ali hardcoced version of url sto nije dobra praksa, vec iz .env uzimam
    // Mora baseURL se bas ovako zvati
    baseURL: process.env.REACT_APP_BASE_BACKEND_API 
})

// Add request iterceptor kako ne bih pri svakom Backend API pozivu (osim Login i Register), koji u .NET ima [Authenticate], prosledjivao JWT
apiBackendWithJWT.interceptors.request.use(
    // Arrow function called before Request is sent
    (config) => {
        const token = localStorage.getItem("token"); // U Context ja msm je ovo setovano
        if (token){
            config.headers.Authorization = `Bearer ${token}`; // Mora sa Bearer zbog .NET ja msm
        }
        return config;
    },
    // Arrow function called iF error is present before Request is sent
    (error) =>{
        return Promise.reject(error);
    }
)

export default apiBackendWithJWT; 

// Jos nigde nisam ubacio ovo, a moram