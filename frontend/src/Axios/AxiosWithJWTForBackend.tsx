import axios from "axios";

// Create axios instance kako u 
const apiBackendWithJWT = axios.create({
    //baseURL: "https://localhost:7045/api/" // Base URL za .NET Backend ali hardcoced version of url sto nije dobra praksa, vec iz .env uzecu.
    // Mora baseURL se bas ovako zvati u axios.create
    baseURL: process.env.REACT_APP_BASE_BACKEND_API 
})

/* Add request iterceptor kako ne bih pri svakom Backend API pozivu (osim Login i Register) u CommentService i PortfolioService, koji u .NET ima [Authenticate], prosledjivao JWT u Authorization header 
bez da rucno to radim */
apiBackendWithJWT.interceptors.request.use(
    // Arrow function called before Request is sent
    (config) => {
        const token = localStorage.getItem("token"); // U Context loginUser/registerUser funkciji sam ga setovao.
        if (token){
            config.headers.Authorization = `Bearer ${token}`; // Mora sa Bearer zbog .NET ja msm
        }
        return config;
    },
    // Arrow function called if error is present before Request is sent
    (error) =>{
        return Promise.reject(error);
    }
)

export default apiBackendWithJWT; 