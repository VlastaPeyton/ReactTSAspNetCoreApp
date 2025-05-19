import React from 'react'
import * as Yup from "yup";
import { useAuth } from '../../Context/useAuthContext';
import { useForm } from 'react-hook-form';
import { yupResolver} from "@hookform/resolvers/yup";
type Props = {}


type LoginFormInputs = {
    userName: string;
    password: string;
}

// Yup je za validaciju
const validation = Yup.object().shape({
    userName: Yup.string().required("Username is required"),
    password: Yup.string().required("Password is required")
});

// Ovo je child Component of UserProvider jer je u App.tsx smestena unutar UserProvider tj unutar <Outlet>
const LoginPage = (props: Props) => {

    const {loginUser} = useAuth();  // loginUser mora jer se tako zove unutar useAuthContext.tsx - Pogledaj useAuthContext.tsx tamo objasnjeno sve
    const {
        // Ova 3 objekta useForm vraca i zato moraju bas ovako da se zovu.
        register, 
        handleSubmit,  
        formState: {errors}, // Destruktuira formState koji sadrzi vise polja, pa mi samo treba errors polje. Ovo isto kao formState.errors 
    } = useForm<LoginFormInputs>({resolver: yupResolver(validation)})
    // Koristi yupResolver za validaciju forme 

    const handleLogin = (loginForm: LoginFormInputs) => {
        loginUser(loginForm.userName, loginForm.password);
    }
    return (
    <section className="bg-gray-50 dark:bg-gray-900">
      <div className="flex flex-col items-center justify-center px-6 py-8 mx-auto md:h-screen lg:py-0">
        <div className="w-full bg-white rounded-lg shadow dark:border md:mb-20 sm:max-w-md xl:p-0 dark:bg-gray-800 dark:border-gray-700">
          <div className="p-6 space-y-4 md:space-y-6 sm:p-8">
            <h1 className="text-xl font-bold leading-tight tracking-tight text-gray-900 md:text-2xl dark:text-white">
              Sign in to your account
            </h1>
            <form className="space-y-4 md:space-y-6" onSubmit={handleSubmit(handleLogin)}> {/* Ne treba *e) => handleSubmit jer on sam ima prevent default u sebi + sam izdvoji sve iz handlelogin */}
              <div>
                <label
                  htmlFor="email"
                  className="block mb-2 text-sm font-medium text-gray-900 dark:text-white"
                >
                  Username
                </label>
                <input
                  type="text"
                  id="username"
                  className="bg-gray-50 border border-gray-300 text-gray-900 sm:text-sm rounded-lg focus:ring-primary-600 focus:border-primary-600 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white dark:focus:ring-blue-500 dark:focus:border-blue-500"
                  placeholder="Username"
                  {...register("userName")}
                />
                {errors.userName ? <p>{errors.userName.message}</p> : ""}
              </div>
              <div>
                <label
                  htmlFor="password"
                  className="block mb-2 text-sm font-medium text-gray-900 dark:text-white"
                >
                  Password
                </label>
                <input
                  type="password"
                  id="password"
                  placeholder="••••••••"
                  className="bg-gray-50 border border-gray-300 text-gray-900 sm:text-sm rounded-lg focus:ring-primary-600 focus:border-primary-600 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white dark:focus:ring-blue-500 dark:focus:border-blue-500"
                  {...register("password")}
                />
                {errors.password ? <p>{errors.password.message}</p> : ""}
              </div>
              <div className="flex items-center justify-between">
                <a
                  href="#"
                  className="text-sm font-medium text-primary-600 hover:underline dark:text-primary-500"
                >
                  Forgot password?
                </a>
              </div>
              <button
                type="submit"
                className="w-full text-white bg-lightGreen hover:bg-primary-700 focus:ring-4 focus:outline-none focus:ring-primary-300 font-medium rounded-lg text-sm px-5 py-2.5 text-center dark:bg-primary-600 dark:hover:bg-primary-700 dark:focus:ring-primary-800"
              >
                Sign in
              </button>
              <p className="text-sm font-light text-gray-500 dark:text-gray-400">
                Don’t have an account yet?{" "}
                <a
                  href="#"
                  className="font-medium text-primary-600 hover:underline dark:text-primary-500"
                >
                  Sign up
                </a>
              </p>
            </form>
          </div>
        </div>
      </div>
    </section>
    )
}

export default LoginPage;
